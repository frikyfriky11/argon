namespace Argon.Application.Transactions.Get;

[UsedImplicitly]
public class TransactionsGetHandler : IRequestHandler<TransactionsGetRequest, TransactionsGetResponse>
{
  private readonly IApplicationDbContext _dbContext;

  public TransactionsGetHandler(IApplicationDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<TransactionsGetResponse> Handle(TransactionsGetRequest request, CancellationToken cancellationToken)
  {
    TransactionsGetResponse? result = await _dbContext
      .Transactions
      .AsNoTracking()
      .Where(transaction => transaction.Id == request.Id)
      .Select(transaction => new TransactionsGetResponse(
        transaction.Id,
        transaction.Date,
        transaction.Description,
        transaction.TransactionRows
          .OrderBy(row => row.RowCounter)
          .ThenBy(row => row.Id)
          .Select(row => new TransactionRowsGetResponse(
            row.Id,
            row.RowCounter,
            row.AccountId,
            row.Debit,
            row.Credit,
            row.Description
          ))
          .ToList()
      ))
      .FirstOrDefaultAsync(cancellationToken);

    if (result is null)
    {
      throw new NotFoundException();
    }

    return result;
  }
}
