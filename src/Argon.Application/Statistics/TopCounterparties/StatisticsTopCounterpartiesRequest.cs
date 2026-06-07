namespace Argon.Application.Statistics.TopCounterparties;

/// <summary>
///   The request to fetch the top counterparties by spend over a period, ranked descending.
///   Spend is the net of the Expense rows of each counterparty's transactions, so income and
///   transfers don't appear. Transactions with no linked counterparty are aggregated under a
///   single "unlinked" bucket so the totals stay honest.
/// </summary>
/// <param name="From">The start of the period (inclusive). Null means from the first transaction.</param>
/// <param name="To">The end of the period (inclusive). Null means up to the last transaction.</param>
/// <param name="Take">How many top counterparties to return. Defaults to 10.</param>
[PublicAPI]
public record StatisticsTopCounterpartiesRequest(
  DateTimeOffset? From,
  DateTimeOffset? To,
  int Take = 10
) : IRequest<List<StatisticsTopCounterpartiesResponse>>;
