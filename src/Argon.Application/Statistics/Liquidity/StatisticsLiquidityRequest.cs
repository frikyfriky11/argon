namespace Argon.Application.Statistics.Liquidity;

/// <summary>
///   The request to fetch the liquid-asset balance over time: the running balance
///   of all Cash accounts at the end of each month. The running balance always
///   accumulates from the beginning of the ledger; <paramref name="From" /> and
///   <paramref name="To" /> only slice which months are returned, so the line is
///   correct even when a window is applied.
/// </summary>
/// <param name="From">The start date of the window to return (inclusive). Null returns from the first month.</param>
/// <param name="To">The end date of the window to return (inclusive). Null returns up to the last month.</param>
[PublicAPI]
public record StatisticsLiquidityRequest(
  DateTimeOffset? From,
  DateTimeOffset? To
) : IRequest<List<StatisticsLiquidityResponse>>;
