using System.Text;
using System.Text.Json;
using Argon.Cli.Auth;
using Argon.Cli.Tests.Infrastructure;

namespace Argon.Cli.Tests.Commands;

[NonParallelizable]
public class LoginCommandTests
{
  [SetUp]
  public void SetUp()
  {
    _harness = new CliTestHarness();
  }

  [TearDown]
  public void TearDown()
  {
    _harness.Dispose();
  }

  private CliTestHarness _harness = null!;

  // ----- logout -----

  [Test]
  public async Task Logout_ShouldDeleteTheCredentialsFile_WhenItExists()
  {
    // arrange
    _harness.TokenStore.Save(new TokenSet
    {
      AccessToken = "at", RefreshToken = "rt", IdToken = MakeIdToken(new { sub = "u" }),
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });
    File.Exists(_harness.TokenStore.CredentialsPath).Should().BeTrue();

    // act
    CliInvocationResult result = await _harness.InvokeAsync("logout");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("Signed out.");
    File.Exists(_harness.TokenStore.CredentialsPath).Should().BeFalse();
  }

  [Test]
  public async Task Logout_ShouldBeANoOp_WhenNoCredentialsAreStored()
  {
    // arrange
    File.Exists(_harness.TokenStore.CredentialsPath).Should().BeFalse();

    // act
    CliInvocationResult result = await _harness.InvokeAsync("logout");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("Signed out.");
  }

  // ----- whoami -----

  [Test]
  public async Task Whoami_ShouldExitOneAndPrintToStderr_WhenNotSignedIn()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("whoami");

    // assert
    result.ExitCode.Should().Be(1);
    result.StdErr.Should().Contain("Not signed in");
    result.StdOut.Should().BeEmpty();
  }

  [Test]
  public async Task Whoami_ShouldPreferThePreferredUsernameClaim_WhenAllAreAvailable()
  {
    // arrange
    DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddHours(1);
    _harness.TokenStore.Save(new TokenSet
    {
      AccessToken = "at",
      IdToken = MakeIdToken(new
      {
        preferred_username = "stefano",
        email = "stefano@example.com",
        sub = "abc-123",
      }),
      ExpiresAt = expiresAt,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("whoami");

    // assert — match the whole "signed in as" line so we cannot pass on
    // "signed in as: stefano@example.com" via substring.
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().MatchRegex(@"(?m)^signed in as: stefano\s*$",
      "preferred_username wins over email when both are present");
  }

  [Test]
  public async Task Whoami_ShouldFallBackToEmail_WhenPreferredUsernameIsAbsent()
  {
    // arrange
    _harness.TokenStore.Save(new TokenSet
    {
      AccessToken = "at",
      IdToken = MakeIdToken(new { email = "stefano@example.com", sub = "abc-123" }),
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("whoami");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("signed in as: stefano@example.com");
  }

  [Test]
  public async Task Whoami_ShouldFallBackToSub_WhenNeitherUsernameNorEmailIsPresent()
  {
    // arrange
    _harness.TokenStore.Save(new TokenSet
    {
      AccessToken = "at",
      IdToken = MakeIdToken(new { sub = "abc-123" }),
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("whoami");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("signed in as: abc-123");
  }

  [Test]
  public async Task Whoami_ShouldPrintUnknown_WhenNoIdTokenIsStored()
  {
    // arrange
    _harness.TokenStore.Save(new TokenSet
    {
      AccessToken = "at",
      IdToken = null,
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("whoami");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("signed in as: (unknown)");
  }

  [Test]
  public async Task Whoami_ShouldPrintUnknown_WhenIdTokenIsMalformed()
  {
    // arrange
    _harness.TokenStore.Save(new TokenSet
    {
      AccessToken = "at",
      IdToken = "not-a-real-jwt",
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("whoami");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("signed in as: (unknown)");
  }

  [Test]
  public async Task Whoami_ShouldPrintTokenExpiryInUniversalSortableFormat()
  {
    // arrange
    DateTimeOffset expiresAt = new(2026, 6, 1, 12, 34, 56, TimeSpan.Zero);
    _harness.TokenStore.Save(new TokenSet
    {
      AccessToken = "at",
      IdToken = MakeIdToken(new { sub = "x" }),
      ExpiresAt = expiresAt,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("whoami");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("token expires: 2026-06-01 12:34:56Z");
  }

  /// <summary>
  ///   Builds a minimally-valid JWT shape: `header.payload.signature` where only the payload
  ///   is meaningful — TryReadClaim only base64-decodes the middle segment, no signature check.
  /// </summary>
  private static string MakeIdToken(object payload)
  {
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(payload);
    string b64 = Convert.ToBase64String(bytes)
      .Replace('+', '-')
      .Replace('/', '_')
      .TrimEnd('=');
    return $"header.{b64}.signature";
  }
}
