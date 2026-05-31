namespace Argon.Application.Counterparties.AccountHistory;

[UsedImplicitly]
public class CounterpartiesAccountHistoryHandler(
  IApplicationDbContext dbContext
) : IRequestHandler<CounterpartiesAccountHistoryRequest, List<CounterpartiesAccountHistoryResponse>>
{
  public async Task<List<CounterpartiesAccountHistoryResponse>> Handle(
    CounterpartiesAccountHistoryRequest request, CancellationToken cancellationToken)
  {
    bool counterpartyExists = await dbContext.Counterparties
      .AsNoTracking()
      .AnyAsync(c => c.Id == request.CounterpartyId, cancellationToken);

    if (!counterpartyExists)
    {
      throw new NotFoundException(nameof(Counterparty), request.CounterpartyId);
    }

    List<AccountAggregate> aggregates = await dbContext.Transactions
      .AsNoTracking()
      .Where(t => t.CounterpartyId == request.CounterpartyId)
      .SelectMany(t => t.TransactionRows
        .Where(r => r.AccountId != null)
        .Select(r => new { AccountId = r.AccountId!.Value, r.Debit, r.Credit, t.Date }))
      .GroupBy(x => x.AccountId)
      .Select(g => new AccountAggregate(
        g.Key,
        g.Count(),
        g.Sum(x => (x.Debit ?? 0m) - (x.Credit ?? 0m)),
        g.Max(x => x.Date)))
      .ToListAsync(cancellationToken);

    if (aggregates.Count == 0)
    {
      return new List<CounterpartiesAccountHistoryResponse>();
    }

    List<AccountDescriptionCount> descriptionCounts = await dbContext.Transactions
      .AsNoTracking()
      .Where(t => t.CounterpartyId == request.CounterpartyId)
      .SelectMany(t => t.TransactionRows)
      .Where(r => r.AccountId != null && r.Description != null && r.Description != "")
      .GroupBy(r => new { AccountId = r.AccountId!.Value, r.Description })
      .Select(g => new AccountDescriptionCount(g.Key.AccountId, g.Key.Description!, g.Count()))
      .ToListAsync(cancellationToken);

    Dictionary<Guid, string?> topDescription = descriptionCounts
      .GroupBy(d => d.AccountId)
      .ToDictionary(
        g => g.Key,
        g => (string?)g.OrderByDescending(d => d.Count).ThenBy(d => d.Description).First().Description);

    List<Guid> accountIds = aggregates.Select(a => a.AccountId).ToList();
    Dictionary<Guid, (string Name, AccountType Type)> accounts = await dbContext.Accounts
      .AsNoTracking()
      .Where(a => accountIds.Contains(a.Id))
      .ToDictionaryAsync(a => a.Id, a => (a.Name, a.Type), cancellationToken);

    return aggregates
      .Select(a => new CounterpartiesAccountHistoryResponse(
        a.AccountId,
        accounts[a.AccountId].Name,
        accounts[a.AccountId].Type,
        a.Count,
        a.Total,
        a.Count == 0 ? 0m : Math.Round(a.Total / a.Count, 2, MidpointRounding.AwayFromZero),
        a.LastDate,
        topDescription.GetValueOrDefault(a.AccountId)))
      .OrderByDescending(r => r.Count)
      .ThenBy(r => r.AccountName)
      .ToList();
  }

  private sealed record AccountAggregate(Guid AccountId, int Count, decimal Total, DateOnly LastDate);

  private sealed record AccountDescriptionCount(Guid AccountId, string Description, int Count);
}
