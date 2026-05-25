namespace Argon.Application.Counterparties.Common;

[UsedImplicitly]
public class CounterpartyResolver(IApplicationDbContext dbContext) : ICounterpartyResolver
{
  public async Task<List<CounterpartyResolution>> ResolveAsync(string? rawText, CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(rawText))
    {
      return new List<CounterpartyResolution>();
    }

    string lowered = rawText.ToLower();

    HashSet<Guid> byIdentifier = (await dbContext.CounterpartyIdentifiers
        .AsNoTracking()
        .Where(ci => ci.IdentifierText.ToLower().Contains(lowered)
                     || lowered.Contains(ci.IdentifierText.ToLower()))
        .Select(ci => ci.CounterpartyId)
        .ToListAsync(cancellationToken))
      .ToHashSet();

    List<Counterparty> byName = await dbContext.Counterparties
      .AsNoTracking()
      .Where(c => c.Name.ToLower().Contains(lowered)
                  || lowered.Contains(c.Name.ToLower()))
      .ToListAsync(cancellationToken);

    HashSet<Guid> byNameIds = byName.Select(c => c.Id).ToHashSet();

    List<Guid> remainingIdentifierIds = byIdentifier.Except(byNameIds).ToList();
    Dictionary<Guid, string> identifierNames = remainingIdentifierIds.Count == 0
      ? new Dictionary<Guid, string>()
      : await dbContext.Counterparties
        .AsNoTracking()
        .Where(c => remainingIdentifierIds.Contains(c.Id))
        .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

    List<CounterpartyResolution> result = new(byIdentifier.Count + byName.Count);
    foreach (Counterparty c in byName)
    {
      result.Add(new CounterpartyResolution(c.Id, c.Name, byIdentifier.Contains(c.Id), true));
    }

    foreach (Guid id in remainingIdentifierIds)
    {
      result.Add(new CounterpartyResolution(id, identifierNames[id], true, false));
    }

    return result;
  }
}
