using Argon.Infrastructure.Persistence.Interceptors;
using NodaTime;

namespace Argon.Application.Tests;

public static class DatabaseTestHelpers
{
  /// <summary>
  ///   The fixed instant the test audit clock returns, so audit-field assertions are deterministic.
  /// </summary>
  public static readonly Instant FixedInstant = Instant.FromUtc(2026, 1, 1, 0, 0);

  public static IApplicationDbContext GetInMemoryDbContext()
  {
    DbContextOptions<ApplicationDbContext> dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    // Use the real auditing interceptor (with a fixed clock) instead of a no-op so that
    // Created/LastModified are populated exactly as in production and stay assertable.
    return new ApplicationDbContext(dbContextOptions, new AuditableEntitySaveChangesInterceptor(new FixedClock(FixedInstant)));
  }

  private sealed class FixedClock(Instant instant) : IClock
  {
    public Instant GetCurrentInstant() => instant;
  }
}
