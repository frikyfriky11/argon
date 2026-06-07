namespace Argon.Application.Statistics.NetWorth;

/// <summary>
///   The current total net worth: Cash + Asset + Receivable balances, less what is owed on
///   Liability accounts.
/// </summary>
/// <param name="Total">Assets − Liabilities across all balance-sheet accounts</param>
[PublicAPI]
public record StatisticsNetWorthResponse(
  decimal Total
);
