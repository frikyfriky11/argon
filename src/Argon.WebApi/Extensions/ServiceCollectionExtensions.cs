namespace Argon.WebApi.Extensions;

[ExcludeFromCodeCoverage]
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

    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options =>
      {
        options.Authority = configuration.GetValue<string>("Auth:Authority");
        options.Audience = configuration.GetValue<string>("Auth:ClientId");
      });
    services.AddAuthorization();

    // OpenTelemetry traces + metrics. Only wired when an OTLP endpoint is configured,
    // so the API runs unchanged when the collector is absent (CI, prod, other devs).
    string? otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];
    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
      Uri endpoint = new(otlpEndpoint);

      services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(
          serviceName: "argon-webapi",
          serviceVersion: typeof(ServiceCollectionExtensions).Assembly.GetName().Version?.ToString() ?? "1.7.0"))
        .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()  // inbound HTTP server spans
          .AddHttpClientInstrumentation()  // outbound HTTP client spans
          .AddSource("Npgsql")             // PostgreSQL command spans (EF Core → Npgsql ActivitySource)
          .AddOtlpExporter(otlp => otlp.Endpoint = endpoint))
        .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation()     // GC, JIT, thread pool, ...
          .AddOtlpExporter(otlp => otlp.Endpoint = endpoint));
    }
  }
}
