namespace Argon.Application.Statistics.NetWorth;

[UsedImplicitly]
public class StatisticsNetWorthHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<StatisticsNetWorthRequest, StatisticsNetWorthResponse>
{
  public async Task<StatisticsNetWorthResponse> Handle(
    StatisticsNetWorthRequest request, CancellationToken cancellationToken)
  {
    // The balance-sheet accounts only. Cash/Asset/Receivable carry a positive debit − credit
    // (assets you hold); Liability accounts carry a negative one (they are net-credited), so
    // adding them straight in subtracts what is still owed. Expense/Revenue (income statement)
    // and Equity (the opening contra-entry) are deliberately excluded.
    decimal total = await dbContext.TransactionRows
      .AsNoTracking()
      .Where(row => row.Account != null
        && (row.Account.Type == AccountType.Cash
          || row.Account.Type == AccountType.Asset
          || row.Account.Type == AccountType.Receivable
          || row.Account.Type == AccountType.Liability))
      .Select(row => (row.Debit ?? 0m) - (row.Credit ?? 0m))
      .SumAsync(cancellationToken);

    return new StatisticsNetWorthResponse(total);
  }
}
