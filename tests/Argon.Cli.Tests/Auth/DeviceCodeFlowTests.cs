using System.Net;
using System.Net.Http;
using Argon.Cli.Auth;
using Argon.Cli.Tests.Infrastructure;

namespace Argon.Cli.Tests.Auth;

[NonParallelizable]
public class DeviceCodeFlowTests
{
  [SetUp]
  public void SetUp()
  {
    _handler = new FakeHttpMessageHandler();
    _http = new HttpClient(_handler);
    _options = new AuthOptions
    {
      Authority = "http://auth.test/app/o/argon/",
      ClientId = "argon-cli",
      Scope = "openid profile offline_access",
    };
    _browserUrl = null;
    _sut = new DeviceCodeFlow(
      _http, _options,
      browserLauncher: url => _browserUrl = url,
      delay: (_, _) => Task.CompletedTask);
  }

  [TearDown]
  public void TearDown()
  {
    _http.Dispose();
    _handler.Dispose();
  }

  private FakeHttpMessageHandler _handler = null!;
  private HttpClient _http = null!;
  private AuthOptions _options = null!;
  private DeviceCodeFlow _sut = null!;
  private string? _browserUrl;

  // ----- LoginAsync -----

  [Test]
  public async Task LoginAsync_ShouldHitDeviceAndTokenEndpoints_AndReturnTokensOnFirstPoll()
  {
    // arrange
    EnqueueDeviceCode(verificationUriComplete: "http://auth.test/verify?code=ABC-DEF");
    EnqueueTokenSuccess(accessToken: "the-access", refreshToken: "the-refresh", idToken: "the-id", expiresIn: 3600);

    // act
    TokenSet tokens = await _sut.LoginAsync(CancellationToken.None);

    // assert
    tokens.AccessToken.Should().Be("the-access");
    tokens.RefreshToken.Should().Be("the-refresh");
    tokens.IdToken.Should().Be("the-id");
    tokens.TokenType.Should().Be("Bearer");
    tokens.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddSeconds(3600), TimeSpan.FromSeconds(5));

    _handler.Requests.Should().HaveCount(2);
    _handler.Requests[0].Uri.AbsolutePath.Should().Be("/app/o/device/");
    _handler.Requests[1].Uri.AbsolutePath.Should().Be("/app/o/token/");
    _browserUrl.Should().Be("http://auth.test/verify?code=ABC-DEF");
  }

  [Test]
  public async Task LoginAsync_ShouldRetryWithIncreasedInterval_WhenTokenEndpointReturnsSlowDown()
  {
    // arrange — server asks us to back off once, then accepts
    List<TimeSpan> requestedDelays = new();
    DeviceCodeFlow sut = new(
      _http, _options,
      browserLauncher: _ => { },
      delay: (interval, _) =>
      {
        requestedDelays.Add(interval);
        return Task.CompletedTask;
      });
    EnqueueDeviceCode(intervalSeconds: 1);
    EnqueueTokenError("slow_down", HttpStatusCode.BadRequest);
    EnqueueTokenSuccess(accessToken: "ok");

    // act
    TokenSet tokens = await sut.LoginAsync(CancellationToken.None);

    // assert
    tokens.AccessToken.Should().Be("ok");
    requestedDelays.Should().HaveCount(2,
      "the polling loop delayed twice: before the slow_down response and before the success");
    requestedDelays[1].Should().BeGreaterThan(requestedDelays[0],
      "slow_down bumps the polling interval up by 5 seconds before the next attempt");
  }

  [Test]
  public async Task LoginAsync_ShouldKeepPolling_WhileTokenEndpointReturnsAuthorizationPending()
  {
    // arrange
    EnqueueDeviceCode();
    EnqueueTokenError("authorization_pending", HttpStatusCode.BadRequest);
    EnqueueTokenError("authorization_pending", HttpStatusCode.BadRequest);
    EnqueueTokenSuccess(accessToken: "ok");

    // act
    TokenSet tokens = await _sut.LoginAsync(CancellationToken.None);

    // assert
    tokens.AccessToken.Should().Be("ok");
    _handler.Requests.Should().HaveCount(4,
      "device code + 3 polls (two pending then success)");
  }

  [Test]
  public async Task LoginAsync_ShouldThrow_OnExpiredToken()
  {
    // arrange
    EnqueueDeviceCode();
    EnqueueTokenError("expired_token", HttpStatusCode.BadRequest);

    // act
    Func<Task> act = () => _sut.LoginAsync(CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("*device code has expired*");
  }

  [Test]
  public async Task LoginAsync_ShouldThrow_OnAccessDenied()
  {
    // arrange
    EnqueueDeviceCode();
    EnqueueTokenError("access_denied", HttpStatusCode.BadRequest);

    // act
    Func<Task> act = () => _sut.LoginAsync(CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("*Authorization was denied*");
  }

  [Test]
  public async Task LoginAsync_ShouldThrow_OnUnexpectedTokenError()
  {
    // arrange
    EnqueueDeviceCode();
    EnqueueTokenError("nuclear_meltdown", HttpStatusCode.InternalServerError);

    // act
    Func<Task> act = () => _sut.LoginAsync(CancellationToken.None);

    // assert
    (await act.Should().ThrowAsync<InvalidOperationException>())
      .Which.Message.Should().ContainAll("Token endpoint returned", "500", "nuclear_meltdown");
  }

  [Test]
  public async Task LoginAsync_ShouldThrow_WhenDeviceCodeRequestItselfFails()
  {
    // arrange
    _handler.EnqueueRaw(HttpStatusCode.BadGateway, "upstream is down", "text/plain");

    // act
    Func<Task> act = () => _sut.LoginAsync(CancellationToken.None);

    // assert
    (await act.Should().ThrowAsync<InvalidOperationException>())
      .Which.Message.Should().ContainAll("Device authorization request failed", "502", "upstream is down");
  }

  [Test]
  public async Task LoginAsync_ShouldPrintTheUserCodeBlock_WhenServerOmitsVerificationUriComplete()
  {
    // arrange
    EnqueueDeviceCode(verificationUriComplete: null, userCode: "FOO-BAR", verificationUri: "http://auth.test/verify");
    EnqueueTokenSuccess();

    // act
    StringWriter capture = new();
    TextWriter previous = Console.Out;
    Console.SetOut(capture);
    try
    {
      await _sut.LoginAsync(CancellationToken.None);
    }
    finally
    {
      Console.SetOut(previous);
    }

    // assert
    string output = capture.ToString();
    output.Should().Contain("http://auth.test/verify");
    output.Should().Contain("Then enter the code: FOO-BAR",
      "when the combined URL is missing, the user has to type the code into the bare verify page");
    _browserUrl.Should().Be("http://auth.test/verify");
  }

  // ----- RefreshAsync -----

  [Test]
  public async Task RefreshAsync_ShouldPostRefreshGrantToTokenEndpoint_AndReturnRefreshedTokens()
  {
    // arrange
    EnqueueTokenSuccess(accessToken: "new-at", refreshToken: "new-rt", idToken: "new-id", expiresIn: 1200);

    // act
    TokenSet tokens = await _sut.RefreshAsync("old-rt", CancellationToken.None);

    // assert
    tokens.AccessToken.Should().Be("new-at");
    tokens.RefreshToken.Should().Be("new-rt");
    tokens.IdToken.Should().Be("new-id");
    tokens.ExpiresAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddSeconds(1200), TimeSpan.FromSeconds(5));

    _handler.Requests.Should().ContainSingle();
    _handler.Requests[0].Method.Should().Be(HttpMethod.Post);
    _handler.Requests[0].Uri.AbsolutePath.Should().Be("/app/o/token/");
    _handler.Requests[0].Body.Should().Contain("grant_type=refresh_token");
    _handler.Requests[0].Body.Should().Contain("refresh_token=old-rt");
    _handler.Requests[0].Body.Should().Contain("client_id=argon-cli");
  }

  [Test]
  public async Task RefreshAsync_ShouldFallBackToTheSuppliedRefreshToken_WhenResponseOmitsIt()
  {
    // arrange — Authentik often omits refresh_token when it rotates only the access token
    _handler.EnqueueRaw(
      HttpStatusCode.OK,
      """{"access_token":"new-at","token_type":"Bearer","expires_in":600}""");

    // act
    TokenSet tokens = await _sut.RefreshAsync("keep-me", CancellationToken.None);

    // assert
    tokens.AccessToken.Should().Be("new-at");
    tokens.RefreshToken.Should().Be("keep-me",
      "callers expect the original refresh token to survive a partial-rotation response");
  }

  [Test]
  public async Task RefreshAsync_ShouldThrow_WhenTokenEndpointReturnsAnError()
  {
    // arrange
    _handler.EnqueueRaw(HttpStatusCode.BadRequest, """{"error":"invalid_grant"}""");

    // act
    Func<Task> act = () => _sut.RefreshAsync("stale-rt", CancellationToken.None);

    // assert
    (await act.Should().ThrowAsync<InvalidOperationException>())
      .Which.Message.Should().ContainAll("Token refresh failed", "400", "invalid_grant");
  }

  // ----- helpers -----

  private void EnqueueDeviceCode(
    string verificationUri = "http://auth.test/verify",
    string? verificationUriComplete = "http://auth.test/verify?code=ABC-DEF",
    string userCode = "ABC-DEF",
    int? expiresInSeconds = 600,
    int? intervalSeconds = 1)
  {
    string vuc = verificationUriComplete is null ? "null" : $"\"{verificationUriComplete}\"";
    _handler.EnqueueRaw(HttpStatusCode.OK,
      $$"""
        {
          "device_code": "DEVCODE-1",
          "user_code": "{{userCode}}",
          "verification_uri": "{{verificationUri}}",
          "verification_uri_complete": {{vuc}},
          "expires_in": {{expiresInSeconds}},
          "interval": {{intervalSeconds}}
        }
        """);
  }

  private void EnqueueTokenSuccess(
    string accessToken = "at",
    string? refreshToken = "rt",
    string? idToken = "id",
    int expiresIn = 3600)
  {
    string rt = refreshToken is null ? "null" : $"\"{refreshToken}\"";
    string id = idToken is null ? "null" : $"\"{idToken}\"";
    _handler.EnqueueRaw(HttpStatusCode.OK,
      $$"""
        {
          "access_token": "{{accessToken}}",
          "refresh_token": {{rt}},
          "id_token": {{id}},
          "token_type": "Bearer",
          "expires_in": {{expiresIn}}
        }
        """);
  }

  private void EnqueueTokenError(string error, HttpStatusCode status)
  {
    _handler.EnqueueRaw(status, $$"""{"error":"{{error}}"}""");
  }
}
