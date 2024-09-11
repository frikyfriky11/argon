namespace Argon.Application.BudgetItems.Upsert;

[UsedImplicitly]
public class BudgetItemsUpsertHandler : IRequestHandler<BudgetItemsUpsertRequest, BudgetItemsUpsertResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public BudgetItemsUpsertHandler(IApplicationDbContext dbContext, IMapper mapper)
  {
    _dbContext = dbContext;
    _mapper = mapper;
  }

  public async Task<BudgetItemsUpsertResponse> Handle(BudgetItemsUpsertRequest request, CancellationToken cancellationToken)
  {
    BudgetItem? entity = await _dbContext
      .BudgetItems
      .Where(budgetItem => budgetItem.AccountId == request.AccountId)
      .Where(budgetItem => budgetItem.Year == request.Year)
      .Where(budgetItem => budgetItem.Month == request.Month)
      .FirstOrDefaultAsync(cancellationToken);

    if (request.Amount is null)
    {
      if (entity is not null)
      {
        _dbContext.BudgetItems.Remove(entity);

        await _dbContext.SaveChangesAsync(cancellationToken);
      }

      return new BudgetItemsUpsertResponse(null);
    }

    if (entity is null)
    {
      entity = _mapper.Map<BudgetItem>(request);

      await _dbContext.BudgetItems.AddAsync(entity, cancellationToken);
    }
    else
    {
      _mapper.Map(request, entity);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);

    return new BudgetItemsUpsertResponse(entity.Id);
  }
}
