namespace Argon.Application.BudgetItems;

/// <summary>
///   The request to get a list of Budget Item entities
/// </summary>
[PublicAPI]
public record BudgetItemsGetListRequest(int Year, int Month) : IRequest<List<BudgetItemsGetListResponse>>;

/// <summary>
///   The result of the Budget Item entities get list
/// </summary>
[PublicAPI]
public record BudgetItemsGetListResponse(Guid Id, Guid AccountId, AccountType AccountType, int Year, int Month, decimal Amount);

[UsedImplicitly]
public class BudgetItemsGetListRequestHandler : IRequestHandler<BudgetItemsGetListRequest, List<BudgetItemsGetListResponse>>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public BudgetItemsGetListRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<List<BudgetItemsGetListResponse>> Handle(BudgetItemsGetListRequest request, CancellationToken cancellationToken)
  {
    return await _dbContext
      .BudgetItems
      .AsNoTracking()
      .Where(budgetItem => budgetItem.Year == request.Year)
      .Where(budgetItem => budgetItem.Month == request.Month)
      .ProjectTo<BudgetItemsGetListResponse>(_mapper.ConfigurationProvider)
      .ToListAsync(cancellationToken);
  }
}
