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

    foreach (BankStatementItem item in result)
    {
      if (item.ErrorMessage is not null)
        // TODO handle this error somehow
        continue;

      // try to find a counterparty
      List<Guid> counterparties = item.CounterpartyName is not null ?
        await dbContext
        .CounterpartyIdentifiers
        .AsNoTracking()
        .Where(counterpartyIdentifier => counterpartyIdentifier.IdentifierText.ToLower() == item.CounterpartyName.ToLower())
        .Select(counterpartyIdentifier => counterpartyIdentifier.CounterpartyId)
        .ToListAsync(cancellationToken)
        : [];

      Guid? counterpartyId;

      if (counterparties.Count > 1)
        // TODO signal too many counterparties found
        counterpartyId = null;
      else if (counterparties.Count == 0)
        // TODO signal no counterparty found
        counterpartyId = null;
      else
        counterpartyId = counterparties[0];

      // try to find a match for transactions that have the same date and account id (always)
      // and also any of the credit, debit or counterparty name fields the same as the item
      List<Guid> matches = await dbContext
        .TransactionRows
        .AsNoTracking()
        .Where(row => row.AccountId == request.ImportToAccountId)
        .Where(row => row.Transaction.Date == item.Date)
        .Where(row => row.Credit == Math.Abs(item.Amount)
                      || row.Debit == Math.Abs(item.Amount)
                      || row.Transaction.CounterpartyId == counterpartyId)
        .Select(row => row.Transaction.Id)
        .ToListAsync(cancellationToken);

      Transaction newTransaction = new()
      {
        Date = item.Date,
        CounterpartyId = counterpartyId,
        Status = matches.Count != 0 ? TransactionStatus.PotentialDuplicate : TransactionStatus.PendingImportReview,
        RawImportData = JsonSerializer.Serialize(item.SpecificParsedItem),
        // TODO PotentialDuplicateOfTransactionId = matches,
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

    return new BankStatementsParseResponse(bankStatement.Id);
  }
}