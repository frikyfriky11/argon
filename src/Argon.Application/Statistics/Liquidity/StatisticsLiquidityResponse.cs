namespace Argon.Application.Statistics.Liquidity;

/// <summary>
///   A single point of the liquid-asset balance time series.
/// </summary>
/// <param name="Year">The calendar year of the point</param>
/// <param name="Month">The calendar month of the point (1-12)</param>
/// <param name="Balance">The running balance of all Cash accounts at the end of this month</param>
[PublicAPI]
public record StatisticsLiquidityResponse(
  int Year,
  int Month,
  decimal Balance
);
