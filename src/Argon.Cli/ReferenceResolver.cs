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

  public Task<Guid> ResolveAccountAsync(string nameOrId, CancellationToken cancellationToken)
    => ResolveAccountAsync(nameOrId, cancellationToken, exact: false);

  public async Task<Guid> ResolveAccountAsync(string nameOrId, CancellationToken cancellationToken, bool exact)
  {
    if (Guid.TryParse(nameOrId, out Guid id))
    {
      return id;
    }

    _accountsCache ??= (await _accounts.GetListAsync(null, null, cancellationToken)).ToList();
    return ResolveByName(_accountsCache, "account", nameOrId, a => a.Name, a => a.Id, exact);
  }

  public Task<Guid> ResolveCounterpartyAsync(string nameOrId, CancellationToken cancellationToken)
    => ResolveCounterpartyAsync(nameOrId, cancellationToken, exact: false);

  public async Task<Guid> ResolveCounterpartyAsync(string nameOrId, CancellationToken cancellationToken, bool exact)
  {
    if (Guid.TryParse(nameOrId, out Guid id))
    {
      return id;
    }

    _counterpartiesCache ??= (await _counterparties.GetListAsync(name: null, pageNumber: 1, pageSize: -1, cancellationToken)).Items.ToList();
    return ResolveByName(_counterpartiesCache, "counterparty", nameOrId, c => c.Name, c => c.Id, exact);
  }

  /// <summary>
  ///   Resolves a name to a single id. An exact (case-insensitive) name match always
  ///   wins. When there is no exact match and <paramref name="exact" /> is false, a
  ///   case-insensitive <i>substring</i> match is attempted so `Athesia` resolves
  ///   `Athesia Buch`. Either stage throws a disambiguation error (listing the
  ///   candidate names and ids) when more than one entry matches, so a fuzzy match
  ///   never silently picks the wrong entity.
  /// </summary>
  private static Guid ResolveByName<T>(
    List<T> cache,
    string label,
    string nameOrId,
    Func<T, string> nameSelector,
    Func<T, Guid> idSelector,
    bool exact)
  {
    List<T> exactMatches = cache
      .Where(item => string.Equals(nameSelector(item), nameOrId, StringComparison.OrdinalIgnoreCase))
      .ToList();

    if (exactMatches.Count == 1)
    {
      return idSelector(exactMatches[0]);
    }

    if (exactMatches.Count > 1)
    {
      throw Ambiguous(label, nameOrId, exactMatches, nameSelector, idSelector);
    }

    if (exact)
    {
      throw new ArgumentException($"No {label} matching '{nameOrId}'.");
    }

    List<T> fuzzyMatches = cache
      .Where(item => nameSelector(item).Contains(nameOrId, StringComparison.OrdinalIgnoreCase))
      .ToList();

    if (fuzzyMatches.Count == 0)
    {
      throw new ArgumentException($"No {label} matching '{nameOrId}'.");
    }

    if (fuzzyMatches.Count > 1)
    {
      throw Ambiguous(label, nameOrId, fuzzyMatches, nameSelector, idSelector);
    }

    return idSelector(fuzzyMatches[0]);
  }

  private static ArgumentException Ambiguous<T>(
    string label,
    string nameOrId,
    List<T> matches,
    Func<T, string> nameSelector,
    Func<T, Guid> idSelector)
  {
    string list = string.Join(", ", matches.Select(m => $"{nameSelector(m)} ({idSelector(m)})"));
    return new ArgumentException(
      $"Multiple {label} entries match '{nameOrId}': {list}. Be more specific, pass the GUID, or use --exact.");
  }
}
