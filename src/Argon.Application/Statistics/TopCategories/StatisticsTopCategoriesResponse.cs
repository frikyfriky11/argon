namespace Argon.Application.Statistics.TopCategories;

/// <summary>
///   A single ranked spending category.
/// </summary>
/// <param name="AccountId">The id of the Expense account</param>
/// <param name="AccountName">The name of the Expense account</param>
/// <param name="Total">The total amount spent on this category in the period</param>
/// <param name="CumulativePercentage">
///   The cumulative share of the period's total spend covered by this category and every
///   higher-ranked one (0-100). The last returned category's value is 100 only when all
///   categories are returned; with a Take limit it reflects the share of the full total.
/// </param>
[PublicAPI]
public record StatisticsTopCategoriesResponse(
  Guid AccountId,
  string AccountName,
  decimal Total,
  decimal CumulativePercentage
);
