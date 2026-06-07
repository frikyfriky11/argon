namespace Argon.Application.Statistics.TopCategories;

/// <summary>
///   The request to fetch the top spending categories (Expense accounts) over a period,
///   ranked by total spend descending, with the running cumulative percentage of the
///   overall spend for the period (the Pareto curve).
/// </summary>
/// <param name="From">The start of the period (inclusive). Null means from the first transaction.</param>
/// <param name="To">The end of the period (inclusive). Null means up to the last transaction.</param>
/// <param name="Take">How many top categories to return. Defaults to 10.</param>
[PublicAPI]
public record StatisticsTopCategoriesRequest(
  DateTimeOffset? From,
  DateTimeOffset? To,
  int Take = 10
) : IRequest<List<StatisticsTopCategoriesResponse>>;
