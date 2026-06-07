namespace Argon.Application.Transactions.CategorizeRow;

/// <summary>
///   The request to assign an account to a single transaction row without resending
///   the rest of the transaction. Used by the import-review reconciliation flow.
/// </summary>
/// <param name="AccountId">The id of the account to assign to the row</param>
/// <param name="Description">
///   Optional description to set on the row being categorized. When null the row's
///   existing description is left untouched; pass an empty string to clear it.
/// </param>
[PublicAPI]
public record TransactionsCategorizeRowRequest(
  Guid AccountId,
  string? Description = null
) : IRequest
{
  /// <summary>
  ///   The id of the parent Transaction. Bound from the route by the controller.
  /// </summary>
  [OpenApiIgnore]
  [JsonIgnore]
  public Guid TransactionId { get; set; }

  /// <summary>
  ///   The id of the TransactionRow to update. Bound from the route by the controller.
  /// </summary>
  [OpenApiIgnore]
  [JsonIgnore]
  public Guid RowId { get; set; }
}
