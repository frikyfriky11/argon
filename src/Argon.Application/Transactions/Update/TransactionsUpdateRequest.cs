namespace Argon.Application.Transactions.Update;

/// <summary>
///   The request to update an existing transaction
/// </summary>
/// <param name="Date">The date of the transaction</param>
/// <param name="CounterpartyId">The id of the counterparty of the transaction</param>
/// <param name="TransactionRows">The rows of the transaction</param>
[PublicAPI]
public record TransactionsUpdateRequest(
  DateOnly Date,
  Guid CounterpartyId,
  List<TransactionRowsUpdateRequest> TransactionRows
) : IRequest
{
  /// <summary>
  ///   This field is used only internally to manually bind the [FromRoute] Guid id attribute.
  ///   It is not displayed in the documentation because the user of the API should use the route parameter.
  ///   This cannot be made internal because it would cause conflicts since you couldn't ever set it.
  /// </summary>
  [OpenApiIgnore]
  [JsonIgnore]
  public Guid Id { get; set; }
}