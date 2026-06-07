namespace Argon.Application.Statistics.Cashflow;

[UsedImplicitly]
public class StatisticsCashflowHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<StatisticsCashflowRequest, List<StatisticsCashflowResponse>>
{
  public async Task<List<StatisticsCashflowResponse>> Handle(
    StatisticsCashflowRequest request, CancellationToken cancellationToken)
  {
    DateOnly? from = request.From == null ? null : DateOnly.FromDateTime(request.From.Value.Date);
    DateOnly? to = request.To == null ? null : DateOnly.FromDateTime(request.To.Value.Date);

    List<MonthlyFlow> flows = await dbContext.TransactionRows
      .AsNoTracking()
      .Where(row => row.Account != null
        && (row.Account.Type == AccountType.Expense || row.Account.Type == AccountType.Revenue))
      .Where(row => from == null || (row.Transaction.AccountingDate ?? row.Transaction.Date) >= from)
      .Where(row => to == null || (row.Transaction.AccountingDate ?? row.Transaction.Date) <= to)
      .Select(row => new MonthlyFlow(
        row.Transaction.AccountingDate ?? row.Transaction.Date,
        row.Account!.Type,
        (row.Debit ?? 0m) - (row.Credit ?? 0m)))
      .ToListAsync(cancellationToken);

    return flows
      .GroupBy(flow => new { flow.Date.Year, flow.Date.Month })
      .OrderBy(group => group.Key.Year)
      .ThenBy(group => group.Key.Month)
      .Select(group => new StatisticsCashflowResponse(
        group.Key.Year,
        group.Key.Month,
        // Revenue accumulates as credits, so income is credit - debit (the negation of debit - credit).
        group.Where(flow => flow.Type == AccountType.Revenue).Sum(flow => -flow.Amount),
        group.Where(flow => flow.Type == AccountType.Expense).Sum(flow => flow.Amount)))
      .ToList();
  }

  private sealed record MonthlyFlow(DateOnly Date, AccountType Type, decimal Amount);
}
