namespace Argon.Application.BankStatements.Parse;

[UsedImplicitly]
public class BankStatementsParseHandler(
  IParsersFactory parsersFactory,
  IApplicationDbContext dbContext
) : IRequestHandler<BankStatementsParseRequest, BankStatementsParseResponse>
{
  public async Task<BankStatementsParseResponse> Handle(BankStatementsParseRequest request,
    CancellationToken cancellationToken)
  {
    BankStatement bankStatement = new()
    {
      FileName = request.InputFileName,
      ParserId = request.ParserId,
      FileContent = request.InputFileContents,
      ImportedToAccountId = request.ImportToAccountId,
    };

    // get the parser with the specified name
    IParser parser = await parsersFactory.CreateParserAsync(request.ParserId);

    // transform the input content into a memory stream
    MemoryStream stream = new(request.InputFileContents);

    // call the parser and get the result
    List<BankStatementItem> result = await parser.ParseAsync(stream);

    List<string> warnings = [];

    foreach (BankStatementItem item in result)
    {
      if (item.ErrorMessage is not null)
      {
        warnings.Add($"Error parsing '{item.RawInput}': {item.ErrorMessage}");
        continue;
      }

      // try to find a counterparty
      List<Guid> counterpartiesByIdentifier = item.CounterpartyName is not null
        ? await dbContext
          .CounterpartyIdentifiers
          .AsNoTracking()
          .Where(counterpartyIdentifier => counterpartyIdentifier.IdentifierText.ToLower().Contains(item.CounterpartyName.ToLower())
                                           || item.CounterpartyName.ToLower().Contains(counterpartyIdentifier.IdentifierText.ToLower()))
          .Select(counterpartyIdentifier => counterpartyIdentifier.CounterpartyId)
          .ToListAsync(cancellationToken)
        : [];

      List<Guid> counterpartiesByName = item.CounterpartyName is not null
        ? await dbContext
          .Counterparties
          .AsNoTracking()
          .Where(counterparty => counterparty.Name.ToLower().Contains(item.CounterpartyName.ToLower())
                                 || item.CounterpartyName.ToLower().Contains(counterparty.Name.ToLower()))
          .Select(counterparty => counterparty.Id)
          .ToListAsync(cancellationToken)
        : [];

      List<Guid> counterparties = counterpartiesByIdentifier.Concat(counterpartiesByName).Distinct().ToList();

      Guid? counterpartyId;

      if (counterparties.Count > 1)
      {
        warnings.Add($"Found {counterparties.Count} counterparties matching '{item.CounterpartyName}' while parsing '{item.RawInput}'. No counterparty assigned.");
        counterpartyId = null;
      }
      else if (counterparties.Count == 0)
      {
        warnings.Add($"No counterparty found matching '{item.CounterpartyName}' while parsing '{item.RawInput}'. No counterparty assigned.");
        counterpartyId = null;
      }
      else
      {
        counterpartyId = counterparties[0];
      }

      // try to find a match for transactions that have the same date and account id (always)
      // and also any of the credit, debit or counterparty name fields the same as the item
      // for simplicity, only the first match will be used
      Guid? match = await dbContext
        .TransactionRows
        .AsNoTracking()
        .Where(row => row.AccountId == request.ImportToAccountId)
        .Where(row => row.Transaction.Date == item.Date)
        .Where(row => row.Credit == Math.Abs(item.Amount)
                      || row.Debit == Math.Abs(item.Amount)
                      || row.Transaction.CounterpartyId == counterpartyId)
        .Select(row => (Guid?)row.Transaction.Id)
        .FirstOrDefaultAsync(cancellationToken);

      Transaction newTransaction = new()
      {
        Date = item.Date,
        CounterpartyId = counterpartyId,
        Status = match is not null ? TransactionStatus.PotentialDuplicate : TransactionStatus.PendingImportReview,
        RawImportData = JsonSerializer.Serialize(item.SpecificParsedItem),
        PotentialDuplicateOfTransactionId = match,
        TransactionRows =
        [
          new TransactionRow
          {
            RowCounter = item.Amount > 0 ? 1 : 2,
            AccountId = request.ImportToAccountId,
            Description = null,
            Debit = item.Amount > 0 ? item.Amount : null,
            Credit = item.Amount < 0 ? -item.Amount : null,
          },
          new TransactionRow
          {
            RowCounter = item.Amount > 0 ? 2 : 1,
            AccountId = null,
            Description = null,
            Debit = item.Amount > 0 ? null : -item.Amount,
            Credit = item.Amount < 0 ? null : item.Amount,
          },
        ],
      };

      bankStatement.Transactions.Add(newTransaction);
    }

    await dbContext.BankStatements.AddAsync(bankStatement, cancellationToken);

    await dbContext.SaveChangesAsync(cancellationToken);

    return new BankStatementsParseResponse(bankStatement.Id, warnings);
  }
}
