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

    // OpenTelemetry traces + metrics. Only wired when an OTLP endpoint is resolved (the standard
    // OTEL_EXPORTER_OTLP_ENDPOINT env var, or the OpenTelemetry:OtlpEndpoint key as a fallback), so
    // the API runs unchanged when the collector is absent (CI, prod, other devs). A malformed
    // endpoint is logged and skipped rather than crashing host startup.
    string? otlpEndpoint = OpenTelemetryDefaults.ResolveOtlpEndpoint(configuration);
    if (otlpEndpoint is not null)
    {
      // When the endpoint comes from the standard env var, let the SDK exporter read the full set
      // of OTEL_EXPORTER_OTLP_* vars (protocol, headers, per-signal endpoints) itself; only apply
      // the custom config value when the env var is absent.
      bool endpointFromEnv = OpenTelemetryDefaults.IsEndpointFromEnvironment(configuration);

      void ConfigureExporter(OtlpExporterOptions otlp)
      {
        if (!endpointFromEnv)
          otlp.Endpoint = new Uri(otlpEndpoint);
      }

      services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
          .AddService(
            serviceName: OpenTelemetryDefaults.ResolveServiceName(configuration),
            serviceVersion: OpenTelemetryDefaults.ServiceVersion)
          .AddContainerDetector()          // container.id when running in Docker (beta)
          .AddHostDetector())              // host.name / host.id (beta)
        .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation(options =>
            // health probes are excluded from the logs too; keep them out of traces as well
            options.Filter = context => !context.Request.Path.StartsWithSegments("/healthz"))  // inbound HTTP server spans
          .AddHttpClientInstrumentation()  // outbound HTTP client spans
          // PostgreSQL command spans. Subscribe to the "Npgsql" ActivitySource that ships in the
          // core Npgsql assembly (already referenced transitively via the EF Core provider). This is
          // exactly what Npgsql.OpenTelemetry's AddNpgsql() does internally; that package isn't used
          // because its TracerProviderBuilderExtensions class collides with the SDK's same-named one,
          // which shadows AddNpgsql() and makes it unreachable without an extern alias.
          .AddSource("Npgsql")
          .AddOtlpExporter(ConfigureExporter))
        .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation()     // GC, JIT, thread pool, ...
          .AddProcessInstrumentation()     // process CPU / memory / thread count (beta)
          .AddMeter("Npgsql")              // Npgsql connection-pool & command metrics
          .AddOtlpExporter(ConfigureExporter));
    }
  }
}
