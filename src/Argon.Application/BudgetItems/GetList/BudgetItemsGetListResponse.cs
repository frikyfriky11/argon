namespace Argon.Application.BudgetItems.GetList;

/// <summary>
///   The result of the Budget Item entities get list
/// </summary>
[PublicAPI]
public record BudgetItemsGetListResponse(
  Guid Id,
  Guid AccountId,
  AccountType AccountType,
  int Year,
  int Month,
  decimal Amount
);