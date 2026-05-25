namespace Argon.Application.Counterparties.AccountHistory;

/// <summary>
///   The request to fetch the frequency table of accounts a counterparty has been
///   posted against. Used during reconciliation to see which expense/revenue
///   account this counterparty typically maps to.
/// </summary>
/// <param name="CounterpartyId">The id of the counterparty</param>
[PublicAPI]
public record CounterpartiesAccountHistoryRequest(
  Guid CounterpartyId
) : IRequest<List<CounterpartiesAccountHistoryResponse>>;
