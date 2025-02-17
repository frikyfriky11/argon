namespace Argon.Application.Transactions.Get;

/// <summary>
///   The result of the get request of a Transaction entity
/// </summary>
/// <param name="Id">The id of the transaction</param>
/// <param name="Date">The date of the transaction</param>
/// <param name="CounterpartyId">The id of the counterparty of the transaction</param>
/// <param name="TransactionRows">The rows of the transaction</param>
[PublicAPI]
public record TransactionsGetResponse(
  Guid Id,
  DateOnly Date,
  Guid CounterpartyId,
  List<TransactionRowsGetResponse> TransactionRows
);