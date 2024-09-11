namespace Argon.Application.BudgetItems.Upsert;

/// <summary>
///   The request to insert or update a budget item
/// </summary>
[PublicAPI]
public record BudgetItemsUpsertRequest(
  Guid AccountId,
  int Year,
  int Month,
  decimal? Amount
) : IRequest<BudgetItemsUpsertResponse>;