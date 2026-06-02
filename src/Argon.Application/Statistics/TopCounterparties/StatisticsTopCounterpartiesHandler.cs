namespace Argon.Application.Statistics.TopCounterparties;

[UsedImplicitly]
public class StatisticsTopCounterpartiesHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<StatisticsTopCounterpartiesRequest, List<StatisticsTopCounterpartiesResponse>>
{
  private const string UnlinkedName = "(Senza controparte)";

  public async Task<List<StatisticsTopCounterpartiesResponse>> Handle(
    StatisticsTopCounterpartiesRequest request, CancellationToken cancellationToken)
  {
    DateOnly? from = request.From == null ? null : DateOnly.FromDateTime(request.From.Value.Date);
    DateOnly? to = request.To == null ? null : DateOnly.FromDateTime(request.To.Value.Date);

    List<CounterpartyMovement> movements = await dbContext.TransactionRows
      .AsNoTracking()
      .Where(row => row.Account != null && row.Account.Type == AccountType.Expense)
      .Where(row => from == null || (row.Transaction.AccountingDate ?? row.Transaction.Date) >= from)
      .Where(row => to == null || (row.Transaction.AccountingDate ?? row.Transaction.Date) <= to)
      .Select(row => new CounterpartyMovement(
        row.Transaction.CounterpartyId,
        row.Transaction.Counterparty != null ? row.Transaction.Counterparty.Name : UnlinkedName,
        (row.Debit ?? 0m) - (row.Credit ?? 0m)))
      .ToListAsync(cancellationToken);

    return movements
      .GroupBy(movement => new { movement.CounterpartyId, movement.CounterpartyName })
      .Select(group => new StatisticsTopCounterpartiesResponse(
        group.Key.CounterpartyId,
        group.Key.CounterpartyName,
        group.Sum(movement => movement.Amount)))
      .Where(response => response.Total > 0m)
      .OrderByDescending(response => response.Total)
      .Take(request.Take)
      .ToList();
  }

  private sealed record CounterpartyMovement(Guid? CounterpartyId, string CounterpartyName, decimal Amount);
}
