namespace Argon.Application.Transactions.Get;

/// <summary>
///   The result of the get request of a Transaction entity
/// </summary>
/// <param name="Id">The id of the transaction</param>
/// <param name="Date">The date of the transaction (currency/value date)</param>
/// <param name="AccountingDate">The accounting (booking) date, or null for manual entries</param>
/// <param name="CounterpartyId">The id of the counterparty of the transaction</param>
/// <param name="CounterpartyName">The name of the counterparty of the transaction</param>
/// <param name="TransactionRows">The rows of the transaction</param>
/// <param name="RawImportData">The JSON representation of the raw import data of a bank statement</param>
/// <param name="Status">The status of the transaction</param>
/// <param name="PotentialDuplicateOfTransactionId">The id of the potential duplicate of the transaction</param>
[PublicAPI]
public record TransactionsGetResponse(
  Guid Id,
  DateOnly Date,
  DateOnly? AccountingDate,
  Guid? CounterpartyId,
  string CounterpartyName,
  List<TransactionRowsGetResponse> TransactionRows,
  string? RawImportData,
  TransactionStatus Status,
  Guid? PotentialDuplicateOfTransactionId
);