using System.Diagnostics;

namespace Argon.Cli.Auth;

public sealed class DeviceCodeFlow
{
  private readonly HttpClient _http;
  private readonly AuthOptions _options;
  private readonly Action<string> _browserLauncher;
  private readonly Func<TimeSpan, CancellationToken, Task> _delay;

  public DeviceCodeFlow(HttpClient http, AuthOptions options)
    : this(http, options, browserLauncher: null, delay: null)
  {
  }

  // Test seam: tests inject a no-op browser launcher and a zero-delay function so the
  // polling loop runs synchronously without spawning a browser process.
  internal DeviceCodeFlow(
    HttpClient http,
    AuthOptions options,
    Action<string>? browserLauncher,
    Func<TimeSpan, CancellationToken, Task>? delay)
  {
    _http = http;
    _options = options;
    _browserLauncher = browserLauncher ?? TryOpenBrowser;
    _delay = delay ?? Task.Delay;
  }

  public async Task<TokenSet> LoginAsync(CancellationToken ct)
  {
    DeviceAuthorizationResponse device = await RequestDeviceCodeAsync(ct);

    Console.WriteLine();
    Console.WriteLine("To finish signing in, open this URL in a browser:");
    Console.WriteLine();
    Console.WriteLine($"    {device.VerificationUriComplete ?? device.VerificationUri}");
    Console.WriteLine();
    if (device.VerificationUriComplete is null)
    {
      Console.WriteLine($"Then enter the code: {device.UserCode}");
      Console.WriteLine();
    }

    _browserLauncher(device.VerificationUriComplete ?? device.VerificationUri);

    Console.WriteLine("Waiting for authorization...");
    return await PollForTokenAsync(device, ct);
  }

  public async Task<TokenSet> RefreshAsync(string refreshToken, CancellationToken ct)
  {
    Dictionary<string, string> form = new()
    {
      ["grant_type"] = "refresh_token",
      ["refresh_token"] = refreshToken,
      ["client_id"] = _options.ClientId,
    };

    using HttpResponseMessage response = await _http.PostAsync(
      BuildUrl("token/"), new FormUrlEncodedContent(form), ct);

    if (!response.IsSuccessStatusCode)
    {
      string body = await response.Content.ReadAsStringAsync(ct);
      throw new InvalidOperationException($"Token refresh failed ({(int)response.StatusCode}): {body}");
    }

    TokenResponse tokenResponse = (await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct))!;
    return ToTokenSet(tokenResponse, fallbackRefreshToken: refreshToken);
  }

  private async Task<DeviceAuthorizationResponse> RequestDeviceCodeAsync(CancellationToken ct)
  {
    Dictionary<string, string> form = new()
    {
      ["client_id"] = _options.ClientId,
      ["scope"] = _options.Scope,
    };

    using HttpResponseMessage response = await _http.PostAsync(
      BuildUrl("device/"), new FormUrlEncodedContent(form), ct);

    if (!response.IsSuccessStatusCode)
    {
      string body = await response.Content.ReadAsStringAsync(ct);
      throw new InvalidOperationException(
        $"Device authorization request failed ({(int)response.StatusCode}): {body}");
    }

    return (await response.Content.ReadFromJsonAsync<DeviceAuthorizationResponse>(cancellationToken: ct))!;
  }

  private async Task<TokenSet> PollForTokenAsync(DeviceAuthorizationResponse device, CancellationToken ct)
  {
    TimeSpan interval = TimeSpan.FromSeconds(Math.Max(1, device.Interval ?? 5));
    DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(device.ExpiresIn ?? 600);

    while (DateTimeOffset.UtcNow < deadline)
    {
      await _delay(interval, ct);

      Dictionary<string, string> form = new()
      {
        ["grant_type"] = "urn:ietf:params:oauth:grant-type:device_code",
        ["device_code"] = device.DeviceCode,
        ["client_id"] = _options.ClientId,
      };

      using HttpResponseMessage response = await _http.PostAsync(
        BuildUrl("token/"), new FormUrlEncodedContent(form), ct);

      string body = await response.Content.ReadAsStringAsync(ct);

      if (response.IsSuccessStatusCode)
      {
        TokenResponse tokenResponse = JsonSerializer.Deserialize<TokenResponse>(body)!;
        return ToTokenSet(tokenResponse);
      }

      TokenErrorResponse? error = TryDeserialize<TokenErrorResponse>(body);
      switch (error?.Error)
      {
        case "authorization_pending":
          continue;
        case "slow_down":
          interval += TimeSpan.FromSeconds(5);
          continue;
        case "expired_token":
          throw new InvalidOperationException("The device code has expired. Run `argon login` again.");
        case "access_denied":
          throw new InvalidOperationException("Authorization was denied.");
        default:
          throw new InvalidOperationException(
            $"Token endpoint returned {(int)response.StatusCode}: {body}");
      }
    }

    throw new InvalidOperationException("Timed out waiting for authorization.");
  }

  private static TokenSet ToTokenSet(TokenResponse response, string? fallbackRefreshToken = null)
  {
    return new TokenSet
    {
      AccessToken = response.AccessToken,
      RefreshToken = response.RefreshToken ?? fallbackRefreshToken,
      IdToken = response.IdToken,
      TokenType = response.TokenType ?? "Bearer",
      ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn ?? 3600),
    };
  }

  private string BuildUrl(string relative)
  {
    // Authority is the OIDC issuer (e.g. https://auth.example.com/application/o/argon-dev/),
    // but Authentik exposes token/device/authorize/userinfo at the un-slugged base
    // (.../application/o/), so strip the trailing application slug before appending.
    Uri authority = new(_options.Authority);
    string path = authority.AbsolutePath.TrimEnd('/');
    int lastSlash = path.LastIndexOf('/');
    string basePath = lastSlash > 0 ? path[..lastSlash] : string.Empty;
    return $"{authority.Scheme}://{authority.Authority}{basePath}/{relative}";
  }

  private static void TryOpenBrowser(string url)
  {
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SSH_CONNECTION"))
        || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SSH_TTY")))
    {
      return;
    }

    try
    {
      Process.Start(new ProcessStartInfo
      {
        FileName = url,
        UseShellExecute = true,
      });
    }
    catch
    {
      // not all environments can launch a browser; the URL is already printed
    }
  }

  private static T? TryDeserialize<T>(string json)
  {
    try
    {
      return JsonSerializer.Deserialize<T>(json);
    }
    catch
    {
      return default;
    }
  }

  private sealed record DeviceAuthorizationResponse
  {
    [JsonPropertyName("device_code")] public string DeviceCode { get; init; } = "";
    [JsonPropertyName("user_code")] public string UserCode { get; init; } = "";
    [JsonPropertyName("verification_uri")] public string VerificationUri { get; init; } = "";
    [JsonPropertyName("verification_uri_complete")] public string? VerificationUriComplete { get; init; }
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; init; }
    [JsonPropertyName("interval")] public int? Interval { get; init; }
  }

  private sealed record TokenResponse
  {
    [JsonPropertyName("access_token")] public string AccessToken { get; init; } = "";
    [JsonPropertyName("refresh_token")] public string? RefreshToken { get; init; }
    [JsonPropertyName("id_token")] public string? IdToken { get; init; }
    [JsonPropertyName("token_type")] public string? TokenType { get; init; }
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; init; }
  }

  private sealed record TokenErrorResponse
  {
    [JsonPropertyName("error")] public string Error { get; init; } = "";
    [JsonPropertyName("error_description")] public string? ErrorDescription { get; init; }
  }
}
