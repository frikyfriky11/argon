namespace Argon.Application.BudgetItems.GetList;

/// <summary>
///   The request to get a list of Budget Item entities
/// </summary>
[PublicAPI]
public record BudgetItemsGetListRequest(
  int Year,
  int Month
) : IRequest<List<BudgetItemsGetListResponse>>;