using Argon.Application.Tests.Database;

namespace Argon.Application.Tests;

public static class DatabaseTestHelpers
{
  public static IApplicationDbContext GetInMemoryDbContext()
  {
    DbContextOptions<ApplicationDbContext> dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

    return new ApplicationDbContext(dbContextOptions, new NoOpSaveChangesInterceptor());
  }
}
