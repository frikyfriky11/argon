using Argon.Cli.Generated;

namespace Argon.Cli;

/// <summary>
///   Resolves CLI tokens that can be either a GUID or an entity name into a GUID.
///   Lists are fetched once per process invocation and cached in-memory.
/// </summary>
internal sealed class ReferenceResolver
{
  private readonly AccountsClient _accounts;
  private readonly CounterpartiesClient _counterparties;
  private List<AccountsGetListResponse>? _accountsCache;
  private List<CounterpartiesGetListResponse>? _counterpartiesCache;

  public ReferenceResolver(AccountsClient accounts, CounterpartiesClient counterparties)
  {
    _accounts = accounts;
    _counterparties = counterparties;
  }

  public async Task<Guid> ResolveAccountAsync(string nameOrId, CancellationToken cancellationToken)
  {
    if (Guid.TryParse(nameOrId, out Guid id))
    {
      return id;
    }

    _accountsCache ??= (await _accounts.GetListAsync(null, null, cancellationToken)).ToList();
    return ResolveByName(_accountsCache, "account", nameOrId, a => a.Name, a => a.Id);
  }

  public async Task<Guid> ResolveCounterpartyAsync(string nameOrId, CancellationToken cancellationToken)
  {
    if (Guid.TryParse(nameOrId, out Guid id))
    {
      return id;
    }

    _counterpartiesCache ??= (await _counterparties.GetListAsync(name: null, pageNumber: 1, pageSize: -1, cancellationToken)).Items.ToList();
    return ResolveByName(_counterpartiesCache, "counterparty", nameOrId, c => c.Name, c => c.Id);
  }

  private static Guid ResolveByName<T>(
    List<T> cache,
    string label,
    string nameOrId,
    Func<T, string> nameSelector,
    Func<T, Guid> idSelector)
  {
    List<T> matches = cache
      .Where(item => string.Equals(nameSelector(item), nameOrId, StringComparison.OrdinalIgnoreCase))
      .ToList();

    if (matches.Count == 0)
    {
      throw new ArgumentException($"No {label} matching '{nameOrId}'.");
    }

    if (matches.Count > 1)
    {
      string ids = string.Join(", ", matches.Select(m => idSelector(m).ToString()));
      throw new ArgumentException(
        $"Multiple {label} entries match '{nameOrId}' (ids: {ids}). Disambiguate with the GUID.");
    }

    return idSelector(matches[0]);
  }
}
