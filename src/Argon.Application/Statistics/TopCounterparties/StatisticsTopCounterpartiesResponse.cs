namespace Argon.Application.Statistics.TopCounterparties;

/// <summary>
///   A single ranked counterparty by spend.
/// </summary>
/// <param name="CounterpartyId">The id of the counterparty, or null for the unlinked bucket</param>
/// <param name="CounterpartyName">The name of the counterparty, or a placeholder for the unlinked bucket</param>
/// <param name="Total">The total amount spent with this counterparty in the period</param>
[PublicAPI]
public record StatisticsTopCounterpartiesResponse(
  Guid? CounterpartyId,
  string CounterpartyName,
  decimal Total
);
