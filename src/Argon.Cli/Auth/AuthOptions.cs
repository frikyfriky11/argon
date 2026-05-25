namespace Argon.Cli.Auth;

public sealed class AuthOptions
{
  public string Authority { get; set; } = "";
  public string ClientId { get; set; } = "";
  public string Scope { get; set; } = "openid profile offline_access";
}
