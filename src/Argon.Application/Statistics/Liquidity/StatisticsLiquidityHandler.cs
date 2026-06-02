namespace Argon.Application.Statistics.Liquidity;

[UsedImplicitly]
public class StatisticsLiquidityHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<StatisticsLiquidityRequest, List<StatisticsLiquidityResponse>>
{
  public async Task<List<StatisticsLiquidityResponse>> Handle(
    StatisticsLiquidityRequest request, CancellationToken cancellationToken)
  {
    DateOnly? from = request.From == null ? null : DateOnly.FromDateTime(request.From.Value.Date);
    DateOnly? to = request.To == null ? null : DateOnly.FromDateTime(request.To.Value.Date);

    // Pull every Cash-account movement projected to its booking date (falling back to the
    // value date for manual entries) and signed amount. Grouping and accumulation happen in
    // memory afterwards so the running balance is exact regardless of the database provider.
    List<MonthlyMovement> movements = await dbContext.TransactionRows
      .AsNoTracking()
      .Where(row => row.Account != null && row.Account.Type == AccountType.Cash)
      .Select(row => new MonthlyMovement(
        row.Transaction.AccountingDate ?? row.Transaction.Date,
        (row.Debit ?? 0m) - (row.Credit ?? 0m)))
      .ToListAsync(cancellationToken);

    List<StatisticsLiquidityResponse> series = movements
      .GroupBy(movement => new { movement.Date.Year, movement.Date.Month })
      .OrderBy(group => group.Key.Year)
      .ThenBy(group => group.Key.Month)
      .Aggregate(
        (Running: 0m, Points: new List<StatisticsLiquidityResponse>()),
        (accumulator, group) =>
        {
          decimal running = accumulator.Running + group.Sum(movement => movement.Amount);
          accumulator.Points.Add(new StatisticsLiquidityResponse(group.Key.Year, group.Key.Month, running));
          return (running, accumulator.Points);
        })
      .Points;

    return series
      .Where(point => from == null || EndOfMonth(point.Year, point.Month) >= from)
      .Where(point => to == null || new DateOnly(point.Year, point.Month, 1) <= to)
      .ToList();
  }

  private static DateOnly EndOfMonth(int year, int month) =>
    new DateOnly(year, month, 1).AddMonths(1).AddDays(-1);

  private sealed record MonthlyMovement(DateOnly Date, decimal Amount);
}
