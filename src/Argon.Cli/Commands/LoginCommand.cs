using System.CommandLine;
using Argon.Cli.Auth;

namespace Argon.Cli.Commands;

internal static class LoginCommand
{
  public static IEnumerable<Command> Build(CliContextFactory factory)
  {
    Command login = new("login", "Sign in to Argon via the browser");
    login.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      TokenSet tokens = await app.DeviceCodeFlow.LoginAsync(ctx.GetCancellationToken());
      app.TokenStore.Save(tokens);
      Console.WriteLine("Signed in.");
    });

    Command logout = new("logout", "Remove locally stored credentials");
    logout.SetHandler(_ =>
    {
      new TokenStore().Clear();
      Console.WriteLine("Signed out.");
    });

    Command whoami = new("whoami", "Show the identity of the currently signed-in user");
    whoami.SetHandler(ctx =>
    {
      TokenSet? tokens = new TokenStore().Load();
      if (tokens is null)
      {
        Console.Error.WriteLine("Not signed in. Run `argon login`.");
        ctx.ExitCode = 1;
        return;
      }

      string? user = TryReadClaim(tokens.IdToken, "preferred_username")
                     ?? TryReadClaim(tokens.IdToken, "email")
                     ?? TryReadClaim(tokens.IdToken, "sub");

      Console.WriteLine($"signed in as: {user ?? "(unknown)"}");
      Console.WriteLine($"token expires: {tokens.ExpiresAt:u}");
    });

    return new[] { login, logout, whoami };
  }

  private static string? TryReadClaim(string? idToken, string claim)
  {
    if (string.IsNullOrEmpty(idToken))
    {
      return null;
    }

    string[] parts = idToken.Split('.');
    if (parts.Length < 2)
    {
      return null;
    }

    try
    {
      string payload = parts[1];
      int padding = (4 - payload.Length % 4) % 4;
      payload = payload.Replace('-', '+').Replace('_', '/') + new string('=', padding);
      byte[] bytes = Convert.FromBase64String(payload);
      using JsonDocument doc = JsonDocument.Parse(bytes);
      if (doc.RootElement.TryGetProperty(claim, out JsonElement value))
      {
        return value.GetString();
      }
    }
    catch
    {
      // ignore — invalid id_token shape
    }

    return null;
  }
}
