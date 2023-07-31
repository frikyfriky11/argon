namespace Argon.Infrastructure.Persistence;

/// <summary>
///   A useful initializer class that can apply migrations and seed the database context
/// </summary>
public class ApplicationDbContextInitializer
{
  private readonly ApplicationDbContext _context;
  private readonly ILogger _logger;

  public ApplicationDbContextInitializer(ILogger logger, ApplicationDbContext context)
  {
    _logger = logger.ForContext<ApplicationDbContextInitializer>();
    _context = context;
  }

  /// <summary>
  ///   Initializes the database by applying the latest migrations
  /// </summary>
  public void Initialize()
  {
    try
    {
      _context.Database.Migrate();
    }
    catch (Exception ex)
    {
      _logger.Error(ex, "An error occurred while initialising the database");
       throw;
    }
  }

  /// <summary>
  ///   Seeds the database with default data that is usually needed right from the first deployment
  /// </summary>
  public void Seed()
  {
    try
    {
      TrySeed();
    }
    catch (Exception ex)
    {
      _logger.Error(ex, "An error occurred while seeding the database");
      throw;
    }
  }

  private void TrySeed()
  {
  }
}
