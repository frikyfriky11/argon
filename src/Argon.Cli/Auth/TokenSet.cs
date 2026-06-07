namespace Argon.Cli.Auth;

public sealed class TokenSet
{
  [JsonPropertyName("access_token")]
  public string AccessToken { get; set; } = "";

  [JsonPropertyName("refresh_token")]
  public string? RefreshToken { get; set; }

  [JsonPropertyName("id_token")]
  public string? IdToken { get; set; }

  [JsonPropertyName("token_type")]
  public string TokenType { get; set; } = "Bearer";

  [JsonPropertyName("expires_at")]
  public DateTimeOffset ExpiresAt { get; set; }

  [JsonIgnore]
  public bool IsExpiringSoon => ExpiresAt - DateTimeOffset.UtcNow < TimeSpan.FromSeconds(60);
}
