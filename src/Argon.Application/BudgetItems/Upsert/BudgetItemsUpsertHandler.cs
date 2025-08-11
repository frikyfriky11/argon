namespace Argon.Application.BudgetItems.Upsert;

[UsedImplicitly]
public class BudgetItemsUpsertHandler(
  IApplicationDbContext dbContext
): IRequestHandler<BudgetItemsUpsertRequest, BudgetItemsUpsertResponse>
{
  public async Task<BudgetItemsUpsertResponse> Handle(BudgetItemsUpsertRequest request, CancellationToken cancellationToken)
  {
    BudgetItem? entity = await dbContext
      .BudgetItems
      .Where(budgetItem => budgetItem.AccountId == request.AccountId)
      .Where(budgetItem => budgetItem.Year == request.Year)
      .Where(budgetItem => budgetItem.Month == request.Month)
      .FirstOrDefaultAsync(cancellationToken);

    if (request.Amount is null)
    {
      if (entity is not null)
      {
        dbContext.BudgetItems.Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
      }

      return new BudgetItemsUpsertResponse(null);
    }

    if (entity is null)
    {
      entity = new BudgetItem
      {
        AccountId = request.AccountId,
        Amount = request.Amount.Value,
        Year = request.Year,
        Month = request.Month,
      };

      await dbContext.BudgetItems.AddAsync(entity, cancellationToken);
    }
    else
    {
      entity.AccountId = request.AccountId;
      entity.Amount = request.Amount.Value;
      entity.Month = request.Month;
      entity.Year = request.Year;
    }

    await dbContext.SaveChangesAsync(cancellationToken);

    return new BudgetItemsUpsertResponse(entity.Id);
  }
}
