namespace Argon.Application.Transactions;

/// <summary>
///   The request to get an existing Transaction entity
/// </summary>
/// <param name="Id">The id of the transaction</param>
[PublicAPI]
public record TransactionsGetRequest(Guid Id) : IRequest<TransactionsGetResponse>;

/// <summary>
///   The row of a transaction get response
/// </summary>
/// <param name="Id">The id of the transaction row</param>
/// <param name="RowCounter">The progressive number of the transaction row in the scope of the transaction</param>
/// <param name="AccountId">The id of the account</param>
/// <param name="Debit">The debit amount of the transaction row</param>
/// <param name="Credit">The credit amount of the transaction row</param>
/// <param name="Description">The description of the transaction row</param>
[PublicAPI]
public record TransactionRowsGetResponse(Guid Id, int RowCounter, Guid AccountId, decimal? Debit, decimal? Credit,
  string? Description);

/// <summary>
///   The result of the get request of a Transaction entity
/// </summary>
/// <param name="Id">The id of the transaction</param>
/// <param name="Date">The date of the transaction</param>
/// <param name="Description">The description of the transaction</param>
/// <param name="TransactionRows">The rows of the transaction</param>
[PublicAPI]
public record TransactionsGetResponse(Guid Id, DateOnly Date, string Description,
  List<TransactionRowsGetResponse> TransactionRows);

[UsedImplicitly]
public class TransactionsGetRequestHandler : IRequestHandler<TransactionsGetRequest, TransactionsGetResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public TransactionsGetRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<TransactionsGetResponse> Handle(TransactionsGetRequest request, CancellationToken cancellationToken)
  {
    TransactionsGetResponse? result = await _dbContext
      .Transactions
      .AsNoTracking()
      .Where(transaction => transaction.Id == request.Id)
      .ProjectTo<TransactionsGetResponse>(_mapper.ConfigurationProvider)
      .FirstOrDefaultAsync(cancellationToken);

    if (result is null)
    {
      throw new NotFoundException();
    }

    return result;
  }
}
