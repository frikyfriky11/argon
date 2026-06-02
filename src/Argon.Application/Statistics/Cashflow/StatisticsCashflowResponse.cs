namespace Argon.Application.Statistics.Cashflow;

/// <summary>
///   A single month of income vs expense.
/// </summary>
/// <param name="Year">The calendar year of the point</param>
/// <param name="Month">The calendar month of the point (1-12)</param>
/// <param name="Income">The net income (Revenue accounts) booked in this month</param>
/// <param name="Expense">The net expense (Expense accounts) booked in this month</param>
[PublicAPI]
public record StatisticsCashflowResponse(
  int Year,
  int Month,
  decimal Income,
  decimal Expense
);
