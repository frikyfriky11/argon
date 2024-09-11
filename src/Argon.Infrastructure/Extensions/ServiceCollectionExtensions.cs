namespace Argon.Infrastructure.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
  /// <summary>
  ///   Adds the infrastructure services to the Dependency Injection container.
  /// </summary>
  /// <param name="services">The service collection container</param>
  /// <param name="configuration">The configuration object</param>
  /// <param name="webHostEnvironment">The web host environment object</param>
  public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
  {
    // add the DbContext interceptors
    services.AddScoped<ISaveChangesInterceptor, AuditableEntitySaveChangesInterceptor>();

    // add the DbContext
    services.AddDbContext<ApplicationDbContext>(options =>
    {
      options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection") ??
        throw new InvalidOperationException("Missing DefaultConnection connection string configuration"),
        builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName).UseNodaTime());

      if (webHostEnvironment.IsDevelopment())
      {
        options.EnableSensitiveDataLogging();
      }
    });

    services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

    // add the DbContext initializer
    services.AddScoped<ApplicationDbContextInitializer>();

    // add the service implementations
    services.AddSingleton<IClock>(SystemClock.Instance);
  }
}
