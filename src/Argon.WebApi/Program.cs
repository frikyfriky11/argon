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
        // TraceId/SpanId, giving log↔trace correlation. No-op when the endpoint is unset.
        string? otlpEndpoint = context.Configuration["OpenTelemetry:OtlpEndpoint"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
          configuration.WriteTo.OpenTelemetry(options =>
          {
            options.Endpoint = otlpEndpoint;
            options.Protocol = OtlpProtocol.Grpc;
            options.ResourceAttributes = new Dictionary<string, object>
            {
              ["service.name"] = "argon-webapi",
              ["service.version"] = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.7.0",
            };
          });
        }
      })
      .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());
  }
}
