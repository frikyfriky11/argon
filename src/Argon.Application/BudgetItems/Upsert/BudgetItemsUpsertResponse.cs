namespace Argon.Application.BudgetItems.Upsert;

/// <summary>
///   The result of the creation or update of a new Budget Item entity
/// </summary>
/// <param name="Id">The id of the newly created or updated Budget Item</param>
[PublicAPI]
public record BudgetItemsUpsertResponse(
  Guid? Id
);