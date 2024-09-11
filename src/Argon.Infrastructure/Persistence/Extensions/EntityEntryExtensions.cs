namespace Argon.Infrastructure.Persistence.Extensions;

[ExcludeFromCodeCoverage]
public static class EntityEntryExtensions
{
  /// <summary>
  ///   Checks if the entity has changed related entities.
  /// </summary>
  /// <param name="entry">The base entity to check</param>
  /// <returns>True if the entity changed related entities, otherwise false</returns>
  public static bool HasChangedOwnedEntities(this EntityEntry entry)
  {
    return entry.References.Any(r =>
      r.TargetEntry != null &&
      r.TargetEntry.Metadata.IsOwned() &&
      r.TargetEntry.State is EntityState.Added or EntityState.Modified);
  }
}
