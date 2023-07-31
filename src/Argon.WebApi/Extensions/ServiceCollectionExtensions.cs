namespace Argon.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
  /// <summary>
  ///   Adds the WebApi services to the Dependency Injection container.
  /// </summary>
  /// <param name="services">The service collection container</param>
  /// <param name="configuration">The configuration object</param>
  public static void AddWebApiServices(this IServiceCollection services, IConfiguration configuration)
  {
    // add debug utilities 
    services.AddDatabaseDeveloperPageExceptionFilter();

    // add some utility services
    services.AddHttpContextAccessor();

    // add health checks endpoints
    services.AddHealthChecks()
      .AddDbContextCheck<ApplicationDbContext>();

    // add the API controllers and configure the pipeline filters for the exception handling
    services.AddControllers(options => options.Filters.Add<ApiExceptionFilterAttribute>());

    // see https://github.com/FluentValidation/FluentValidation/issues/1965
    services.AddFluentValidationClientsideAdapters();

    services.Configure<ApiBehaviorOptions>(options =>
      options.SuppressModelStateInvalidFilter = true);

    // add the Swagger definitions as an OpenAPI document
    services.AddOpenApiDocument(options =>
    {
      options.Title = "Argon API";
    });

    services.AddCors(options =>
      options.AddPolicy("CorsPolicy", policy =>
        {
          string[] origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? throw new InvalidOperationException("Missing CORS configuration");

          policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithOrigins(origins)
            .WithExposedHeaders("Content-Disposition");
        }
      )
    );
  }
}
