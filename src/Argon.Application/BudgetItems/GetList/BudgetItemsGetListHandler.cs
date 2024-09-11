namespace Argon.Application.BudgetItems.GetList;

[UsedImplicitly]
public class BudgetItemsGetListHandler : IRequestHandler<BudgetItemsGetListRequest, List<BudgetItemsGetListResponse>>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public BudgetItemsGetListHandler(IApplicationDbContext dbContext, IMapper mapper)
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
