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

    List<AccountCount> counts = await dbContext.Transactions
      .AsNoTracking()
      .Where(t => t.CounterpartyId == request.CounterpartyId)
      .SelectMany(t => t.TransactionRows)
      .Where(r => r.AccountId != null)
      .GroupBy(r => r.AccountId!.Value)
      .Select(g => new AccountCount(g.Key, g.Count()))
      .ToListAsync(cancellationToken);

    if (counts.Count == 0)
    {
      return new List<CounterpartiesAccountHistoryResponse>();
    }

    List<Guid> accountIds = counts.Select(c => c.AccountId).ToList();
    Dictionary<Guid, (string Name, AccountType Type)> accounts = await dbContext.Accounts
      .AsNoTracking()
      .Where(a => accountIds.Contains(a.Id))
      .ToDictionaryAsync(a => a.Id, a => (a.Name, a.Type), cancellationToken);

    return counts
      .Select(c => new CounterpartiesAccountHistoryResponse(
        c.AccountId,
        accounts[c.AccountId].Name,
        accounts[c.AccountId].Type,
        c.Count))
      .OrderByDescending(r => r.Count)
      .ThenBy(r => r.AccountName)
      .ToList();
  }

  private sealed record AccountCount(Guid AccountId, int Count);
}
