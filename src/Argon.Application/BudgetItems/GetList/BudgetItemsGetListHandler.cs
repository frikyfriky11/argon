namespace Argon.Application.BudgetItems.GetList;

[UsedImplicitly]
public class BudgetItemsGetListHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<BudgetItemsGetListRequest, List<BudgetItemsGetListResponse>>
{
  public async Task<List<BudgetItemsGetListResponse>> Handle(BudgetItemsGetListRequest request, CancellationToken cancellationToken)
  {
    return await dbContext
      .BudgetItems
      .AsNoTracking()
      .Where(budgetItem => budgetItem.Year == request.Year)
      .Where(budgetItem => budgetItem.Month == request.Month)
      .Select(budgetItem => new BudgetItemsGetListResponse(
        budgetItem.Id,
        budgetItem.AccountId,
        budgetItem.Account.Type,
        budgetItem.Year,
        budgetItem.Month,
        budgetItem.Amount
      ))
      .ToListAsync(cancellationToken);
  }
}
