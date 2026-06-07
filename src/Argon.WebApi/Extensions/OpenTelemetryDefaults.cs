namespace Argon.WebApi.Extensions;

/// <summary>
///   Shared OpenTelemetry configuration helpers used by both the traces/metrics SDK wiring and the
///   Serilog OTLP log sink. Resolves the OTLP endpoint, service name and protocol from configuration,
///   giving the standard <c>OTEL_*</c> environment variables precedence over the project-specific
///   <c>OpenTelemetry:*</c> keys.
/// </summary>
[ExcludeFromCodeCoverage]
public static class OpenTelemetryDefaults
{
  /// <summary>The default logical service name reported to the collector.</summary>
  public const string ServiceName = "argon-webapi";

  /// <summary>The project-specific configuration key holding the OTLP endpoint.</summary>
  public const string OtlpEndpointConfigKey = "OpenTelemetry:OtlpEndpoint";

  /// <summary>The standard OpenTelemetry environment variable for the OTLP endpoint.</summary>
  public const string OtlpEndpointEnvVar = "OTEL_EXPORTER_OTLP_ENDPOINT";

  /// <summary>The standard OpenTelemetry environment variable for the service name.</summary>
  public const string ServiceNameEnvVar = "OTEL_SERVICE_NAME";

  /// <summary>The standard OpenTelemetry environment variable for the OTLP protocol.</summary>
  public const string OtlpProtocolEnvVar = "OTEL_EXPORTER_OTLP_PROTOCOL";

  /// <summary>The service version, derived from the running assembly (no hardcoded fallback).</summary>
  public static string ServiceVersion =>
    typeof(OpenTelemetryDefaults).Assembly.GetName().Version?.ToString() ?? "unknown";

  /// <summary>
  ///   <c>true</c> when the OTLP endpoint comes from the standard <c>OTEL_EXPORTER_OTLP_ENDPOINT</c>
  ///   environment variable. When so, the OpenTelemetry SDK exporter is left to read the endpoint
  ///   (and its sibling protocol / header / per-signal env vars) itself.
  /// </summary>
  public static bool IsEndpointFromEnvironment(IConfiguration configuration)
  {
    return !string.IsNullOrWhiteSpace(configuration[OtlpEndpointEnvVar]);
  }

  /// <summary>
  ///   Resolves the OTLP endpoint, preferring the standard <c>OTEL_EXPORTER_OTLP_ENDPOINT</c>
  ///   environment variable over the project-specific <c>OpenTelemetry:OtlpEndpoint</c> key.
  ///   Returns <c>null</c> when neither is set, or when the value is not a valid absolute URI — in
  ///   which case a warning is logged and telemetry export stays disabled rather than crashing
  ///   host startup.
  /// </summary>
  public static string? ResolveOtlpEndpoint(IConfiguration configuration)
  {
    string? endpoint = configuration[OtlpEndpointEnvVar];
    if (string.IsNullOrWhiteSpace(endpoint))
      endpoint = configuration[OtlpEndpointConfigKey];

    if (string.IsNullOrWhiteSpace(endpoint))
      return null;

    if (!Uri.TryCreate(endpoint, UriKind.Absolute, out _))
    {
      Log.Warning("OpenTelemetry OTLP endpoint {Endpoint} is not a valid absolute URI; telemetry export disabled", endpoint);
      return null;
    }

    return endpoint;
  }

  /// <summary>
  ///   Resolves the service name, preferring the standard <c>OTEL_SERVICE_NAME</c> environment
  ///   variable over the <see cref="ServiceName" /> default.
  /// </summary>
  public static string ResolveServiceName(IConfiguration configuration)
  {
    string? serviceName = configuration[ServiceNameEnvVar];
    return string.IsNullOrWhiteSpace(serviceName) ? ServiceName : serviceName;
  }

  /// <summary>
  ///   Resolves the OTLP protocol for the Serilog sink from the standard
  ///   <c>OTEL_EXPORTER_OTLP_PROTOCOL</c> environment variable, defaulting to gRPC. (The
  ///   OpenTelemetry SDK exporter reads this variable on its own, so it only needs handling here.)
  /// </summary>
  public static OtlpProtocol ResolveOtlpProtocol(IConfiguration configuration)
  {
    return configuration[OtlpProtocolEnvVar]?.Trim().ToLowerInvariant() switch
    {
      "http/protobuf" => OtlpProtocol.HttpProtobuf,
      "grpc" => OtlpProtocol.Grpc,
      _ => OtlpProtocol.Grpc,
    };
  }
}
