namespace Argon.Application.Transactions.GetList;

/// <summary>
///   The result of the Transaction entities get list
/// </summary>
/// <param name="Id">The id of the transaction</param>
/// <param name="Date">The date of the transaction</param>
/// <param name="CounterpartyId">The id of the counterparty of the transaction</param>
/// <param name="CounterpartyName">The name of the counterparty of the transaction</param>
/// <param name="TransactionRows">The rows of the transaction</param>
[PublicAPI]
public record TransactionsGetListResponse(
  Guid Id,
  DateOnly Date,
  Guid? CounterpartyId,
  string CounterpartyName,
  List<TransactionRowsGetListResponse> TransactionRows
);