namespace Argon.Application.Statistics.TopCategories;

[UsedImplicitly]
public class StatisticsTopCategoriesHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<StatisticsTopCategoriesRequest, List<StatisticsTopCategoriesResponse>>
{
  public async Task<List<StatisticsTopCategoriesResponse>> Handle(
    StatisticsTopCategoriesRequest request, CancellationToken cancellationToken)
  {
    DateOnly? from = request.From == null ? null : DateOnly.FromDateTime(request.From.Value.Date);
    DateOnly? to = request.To == null ? null : DateOnly.FromDateTime(request.To.Value.Date);

    List<CategoryTotal> totals = await dbContext.TransactionRows
      .AsNoTracking()
      .Where(row => row.Account != null && row.Account.Type == AccountType.Expense)
      .Where(row => from == null || (row.Transaction.AccountingDate ?? row.Transaction.Date) >= from)
      .Where(row => to == null || (row.Transaction.AccountingDate ?? row.Transaction.Date) <= to)
      .GroupBy(row => new { Id = row.AccountId!.Value, row.Account!.Name })
      .Select(group => new CategoryTotal(
        group.Key.Id,
        group.Key.Name,
        group.Sum(row => (row.Debit ?? 0m) - (row.Credit ?? 0m))))
      .ToListAsync(cancellationToken);

    // Only positive net spend ranks; a category that net-refunded over the period would
    // otherwise distort the Pareto curve and the grand total.
    List<CategoryTotal> ranked = totals
      .Where(category => category.Total > 0m)
      .OrderByDescending(category => category.Total)
      .ToList();

    decimal grandTotal = ranked.Sum(category => category.Total);

    decimal cumulative = 0m;
    List<StatisticsTopCategoriesResponse> response = new();
    foreach (CategoryTotal category in ranked.Take(request.Take))
    {
      cumulative += category.Total;
      decimal cumulativePercentage = grandTotal == 0m
        ? 0m
        : Math.Round(cumulative / grandTotal * 100m, 2, MidpointRounding.AwayFromZero);
      response.Add(new StatisticsTopCategoriesResponse(
        category.AccountId, category.AccountName, category.Total, cumulativePercentage));
    }

    return response;
  }

  private sealed record CategoryTotal(Guid AccountId, string AccountName, decimal Total);
}
