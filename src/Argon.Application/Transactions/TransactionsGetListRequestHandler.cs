using Argon.Application.Common.Models;
using Argon.Application.Extensions;

namespace Argon.Application.Transactions;

/// <summary>
///   The request to get a list of Transaction entities
/// </summary>
/// <param name="AccountIds">The account ids used in the transaction rows</param>
/// <param name="Description">The description used in the transaction</param>
/// <param name="DateFrom">The start date to use in the search of the transaction</param>
/// <param name="DateTo">The end date to use in the search of the transaction</param>
/// <param name="PageNumber">The page number (defaults to 1)</param>
/// <param name="PageSize">The page size (defaults to 25)</param>
[PublicAPI]
public record TransactionsGetListRequest(List<Guid>? AccountIds, string? Description,
    DateTimeOffset? DateFrom, DateTimeOffset? DateTo,
    int PageNumber = 1, int PageSize = 25)
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
      .Where(transaction => string.IsNullOrWhiteSpace(request.Description) || transaction.Description.ToLower().Contains(request.Description.ToLower()))
      .Where(transaction => request.DateFrom == null || transaction.Date >= DateOnly.FromDateTime(request.DateFrom.Value.Date))
      .Where(transaction => request.DateTo == null || transaction.Date <= DateOnly.FromDateTime(request.DateTo.Value.Date))
      .OrderByDescending(transaction => transaction.Date)
      .ThenByDescending(transaction => transaction.Created)
      .ThenByDescending(transaction => transaction.Id)
      .ProjectTo<TransactionsGetListResponse>(_mapper.ConfigurationProvider)
      .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    
    // order of the records must be deterministic and avoid random sorting
    // when two or more records have the same Date, so that when pagination occurs
    // no record is skipped. sorting by Id is sufficient because it is a primary key
    // and thus is unique, but adding the Created field shows the transaction in the
    // order they were entered too, so that's a bonus
  }
}
