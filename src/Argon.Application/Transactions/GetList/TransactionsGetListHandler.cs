using Argon.Application.Common.Models;
using Argon.Application.Extensions;

namespace Argon.Application.Transactions.GetList;

[UsedImplicitly]
public class TransactionsGetListHandler : IRequestHandler<TransactionsGetListRequest, PaginatedList<TransactionsGetListResponse>>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public TransactionsGetListHandler(IApplicationDbContext dbContext, IMapper mapper)
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
