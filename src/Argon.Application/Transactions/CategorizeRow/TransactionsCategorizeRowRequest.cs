namespace Argon.Application.Transactions.CategorizeRow;

/// <summary>
///   The request to assign an account to a single transaction row without resending
///   the rest of the transaction. Used by the import-review reconciliation flow.
/// </summary>
/// <param name="AccountId">The id of the account to assign to the row</param>
[PublicAPI]
public record TransactionsCategorizeRowRequest(
  Guid AccountId
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
