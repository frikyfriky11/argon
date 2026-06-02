namespace Argon.Application.Statistics.Cashflow;

/// <summary>
///   The request to fetch monthly income vs expense over a period. Income is the net of
///   Revenue accounts, expense is the net of Expense accounts; transfers between Cash
///   accounts never touch either type and are therefore naturally excluded.
/// </summary>
/// <param name="From">The start of the period (inclusive). Null means from the first transaction.</param>
/// <param name="To">The end of the period (inclusive). Null means up to the last transaction.</param>
[PublicAPI]
public record StatisticsCashflowRequest(
  DateTimeOffset? From,
  DateTimeOffset? To
) : IRequest<List<StatisticsCashflowResponse>>;
