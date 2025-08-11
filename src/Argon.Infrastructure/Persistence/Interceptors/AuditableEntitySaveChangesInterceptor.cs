namespace Argon.Infrastructure.Persistence.Interceptors;

/// <summary>
///   This interceptor intercepts all the SaveChanges calls to the DbContext and updates the
///   <see cref="BaseAuditableEntity" /> properties.
/// </summary>
[ExcludeFromCodeCoverage]
public class AuditableEntitySaveChangesInterceptor(
  IClock clock
) : SaveChangesInterceptor
{
  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    UpdateEntities(eventData.Context);

    return base.SavingChanges(eventData, result);
  }

  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
  {
    UpdateEntities(eventData.Context);

    return base.SavingChangesAsync(eventData, result, cancellationToken);
  }

  private void UpdateEntities(DbContext? context)
  {
    if (context == null)
    {
      return;
    }

    // loop on every tracked entity that inherits from BaseAuditableEntity
    foreach (EntityEntry<BaseAuditableEntity> entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
    {
      if (entry.State == EntityState.Added)
      {
        entry.Entity.Created = clock.GetCurrentInstant();
      }

      if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
      {
        entry.Entity.LastModified = clock.GetCurrentInstant();
      }
    }
  }
}
