using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Argon.Cli.Auth;

public sealed class TokenStore
{
  private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

  public string CredentialsPath { get; }

  public TokenStore()
  {
    string baseDir = OperatingSystem.IsWindows()
      ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
      : Path.Combine(
        Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
          ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config"));

    string dir = Path.Combine(baseDir, "argon-cli");
    Directory.CreateDirectory(dir);
    CredentialsPath = Path.Combine(dir, "credentials.json");
  }

  public TokenSet? Load()
  {
    if (!File.Exists(CredentialsPath))
    {
      return null;
    }

    byte[] raw = File.ReadAllBytes(CredentialsPath);
    byte[] plain = Unprotect(raw);
    return JsonSerializer.Deserialize<TokenSet>(plain);
  }

  public void Save(TokenSet tokens)
  {
    byte[] plain = JsonSerializer.SerializeToUtf8Bytes(tokens, JsonOptions);
    byte[] protectedBytes = Protect(plain);
    File.WriteAllBytes(CredentialsPath, protectedBytes);

    if (!OperatingSystem.IsWindows())
    {
      try
      {
        File.SetUnixFileMode(CredentialsPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
      }
      catch
      {
        // best-effort: not all filesystems support unix modes
      }
    }
  }

  public void Clear()
  {
    if (File.Exists(CredentialsPath))
    {
      File.Delete(CredentialsPath);
    }
  }

  private static byte[] Protect(byte[] plain)
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      return ProtectedData.Protect(plain, optionalEntropy: null, DataProtectionScope.CurrentUser);
    }

    return plain;
  }

  private static byte[] Unprotect(byte[] data)
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
      return ProtectedData.Unprotect(data, optionalEntropy: null, DataProtectionScope.CurrentUser);
    }

    return data;
  }
}
