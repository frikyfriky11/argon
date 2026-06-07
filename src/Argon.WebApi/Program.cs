namespace Argon.WebApi;

public static class Program
{
  public static int Main(string[] args)
  {
    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
      .Enrich.FromLogContext()
      .WriteTo.Console()
      .CreateBootstrapLogger();

    try
    {
      Log.Information("Starting web host");
      CreateHostBuilder(args).Build().Run();
      return 0;
    }
    catch (Exception ex)
    {
      Log.Fatal(ex, "Host terminated unexpectedly");
      return 1;
    }
    finally
    {
      Log.CloseAndFlush();
    }
  }

  private static IHostBuilder CreateHostBuilder(string[] args)
  {
    return Host.CreateDefaultBuilder(args)
      .UseSerilog((context, services, configuration) =>
      {
        configuration
          .ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services)
          .Enrich.FromLogContext();

        // Ship logs to the OTLP collector when configured. The sink attaches the active
        // TraceId/SpanId, giving log↔trace correlation. Endpoint/name/protocol are resolved with
        // the standard OTEL_* env vars taking precedence; no-op when no endpoint is resolved.
        string? otlpEndpoint = OpenTelemetryDefaults.ResolveOtlpEndpoint(context.Configuration);
        if (otlpEndpoint is not null)
        {
          configuration.WriteTo.OpenTelemetry(options =>
          {
            options.Endpoint = otlpEndpoint;
            options.Protocol = OpenTelemetryDefaults.ResolveOtlpProtocol(context.Configuration);
            options.ResourceAttributes = new Dictionary<string, object>
            {
              ["service.name"] = OpenTelemetryDefaults.ResolveServiceName(context.Configuration),
              ["service.version"] = OpenTelemetryDefaults.ServiceVersion,
            };
          });
        }
      })
      .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());
  }
}
