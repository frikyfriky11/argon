namespace Argon.Application.Transactions.Create;

[UsedImplicitly]
public class TransactionsCreateHandler : IRequestHandler<TransactionsCreateRequest, TransactionsCreateResponse>
{
  private readonly IApplicationDbContext _dbContext;

  public TransactionsCreateHandler(IApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<TransactionsCreateResponse> Handle(TransactionsCreateRequest request, CancellationToken cancellationToken)
  {
    Transaction entity = new()
    {
      Description = request.Description,
      Date = request.Date,
      TransactionRows = request
        .TransactionRows
        .Select(row => new TransactionRow
        {
          Description = row.Description,
          RowCounter = row.RowCounter,
          AccountId = row.AccountId,
          Debit = row.Debit,
          Credit = row.Credit,
        })
        .ToList(),
    };

    await _dbContext.Transactions.AddAsync(entity, cancellationToken);

    await _dbContext.SaveChangesAsync(cancellationToken);

    return new TransactionsCreateResponse(entity.Id);
  }
}
