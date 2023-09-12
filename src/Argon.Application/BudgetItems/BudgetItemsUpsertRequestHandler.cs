namespace Argon.Application.BudgetItems;

/// <summary>
///   The request to insert or update a budget item
/// </summary>
[PublicAPI]
public record BudgetItemsUpsertRequest(Guid AccountId, int Year, int Month, decimal? Amount) : IRequest<BudgetItemsUpsertResponse>;

/// <summary>
///   The result of the creation or update of a new Budget Item entity
/// </summary>
/// <param name="Id">The id of the newly created or updated Budget Item</param>
[PublicAPI]
public record BudgetItemsUpsertResponse(Guid? Id);

// [UsedImplicitly]
// public class AccountsUpdateRequestValidator : AbstractValidator<BudgetItemsUpsertRequest>
// {
//   public AccountsUpdateRequestValidator(IApplicationDbContext dbContext)
//   {
//     RuleFor(request => request.Name)
//       .NotEmpty()
//       .MaximumLength(50);
//
//     RuleFor(request => request.Type)
//       .IsInEnum();
//   }
// }

[UsedImplicitly]
public class BudgetItemsUpsertRequestHandler : IRequestHandler<BudgetItemsUpsertRequest, BudgetItemsUpsertResponse>
{
  private readonly IApplicationDbContext _dbContext;
  private readonly IMapper _mapper;

  public BudgetItemsUpsertRequestHandler(IApplicationDbContext dbContext, IMapper mapper)
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
