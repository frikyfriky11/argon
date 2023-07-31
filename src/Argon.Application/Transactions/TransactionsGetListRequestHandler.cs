using Argon.Application.Common.Models;
using Argon.Application.Extensions;

namespace Argon.Application.Transactions;

/// <summary>
///   The request to get a list of Transaction entities
/// </summary>
/// <param name="AccountIds">The account ids used in the transaction rows</param>
/// <param name="PageNumber">The page number (defaults to 1)</param>
/// <param name="PageSize">The page size (defaults to 25)</param>
[PublicAPI]
public record TransactionsGetListRequest(List<Guid>? AccountIds, int PageNumber = 1, int PageSize = 25)
  : PaginatedListRequest(PageNumber, PageSize),
    IRequest<PaginatedList<TransactionsGetListResponse>>; 

/// <summary>
///   The row of a transaction get list response
/// </summary>
/// <param name="Id">The id of the transaction row</param>
/// <param name="RowCounter">The progressive number of the transaction row in the scope of the transaction</param>
/// <param name="AccountId">The id of the account</param>
/// <param name="AccountName">The name of the account</param>
/// <param name="Debit">The debit amount of the transaction row</param>
/// <param name="Credit">The credit amount of the transaction row</param>
/// <param name="Description">The description of the transaction row</param>
[PublicAPI]
public record TransactionRowsGetListResponse(Guid Id, int RowCounter, Guid AccountId, string AccountName,
  decimal? Debit, decimal? Credit, string? Description);

/// <summary>
///   The result of the Transaction entities get list
/// </summary>
/// <param name="Id">The id of the transaction</param>
/// <param name="Date">The date of the transaction</param>
/// <param name="Description">The description of the transaction</param>
/// <param name="TransactionRows">The rows of the transaction</param>
[PublicAPI]
public record TransactionsGetListResponse(Guid Id, DateOnly Date, string Description,
  List<TransactionRowsGetListResponse> TransactionRows);

[UsedImplicitly]
public class TransactionsGetListRequestHandler : IRequestHandler<TransactionsGetListRequest, PaginatedList<TransactionsGetListResponse>>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public TransactionsGetListRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<PaginatedList<TransactionsGetListResponse>> Handle(TransactionsGetListRequest request, CancellationToken cancellationToken)
  {
    return await _dbContext
      .Transactions
      .AsNoTracking()
      .Where(transaction => request.AccountIds == null || request.AccountIds.Count == 0 || transaction.TransactionRows.Any(row => request.AccountIds.Contains(row.AccountId)))
      .OrderByDescending(transaction => transaction.Date)
      .ProjectTo<TransactionsGetListResponse>(_mapper.ConfigurationProvider)
      .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
  }
}
