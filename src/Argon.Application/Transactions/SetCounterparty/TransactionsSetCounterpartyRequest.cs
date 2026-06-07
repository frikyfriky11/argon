namespace Argon.Application.Transactions.SetCounterparty;

/// <summary>
///   The request to reassign the counterparty of a Transaction without resending the
///   rest of the transaction. Used when the parser auto-matched the wrong counterparty.
/// </summary>
/// <param name="CounterpartyId">The id of the counterparty to assign to the transaction</param>
[PublicAPI]
public record TransactionsSetCounterpartyRequest(
  Guid CounterpartyId
) : IRequest
{
  /// <summary>
  ///   The id of the Transaction. Bound from the route by the controller.
  /// </summary>
  [OpenApiIgnore]
  [JsonIgnore]
  public Guid TransactionId { get; set; }
}
