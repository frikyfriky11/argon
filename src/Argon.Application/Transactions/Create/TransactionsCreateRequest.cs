namespace Argon.Application.Transactions.Create;

/// <summary>
///   The request to create a new Transaction entity
/// </summary>
/// <param name="Date">The date of the transaction</param>
/// <param name="CounterpartyId">The id of the counterparty of the transaction</param>
/// <param name="TransactionRows">The rows of the transaction</param>
[PublicAPI]
public record TransactionsCreateRequest(
  DateOnly Date,
  Guid CounterpartyId,
  List<TransactionRowsCreateRequest> TransactionRows
) : IRequest<TransactionsCreateResponse>;