using System.Net;
using System.Net.Http;
using Argon.Cli.Auth;
using Argon.Cli.Tests.Infrastructure;

namespace Argon.Cli.Tests.Auth;

[NonParallelizable]
public class AuthenticatedHttpHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _authFake = new FakeHttpMessageHandler();
    _authClient = new HttpClient(_authFake);
    AuthOptions options = new()
    {
      Authority = "http://auth.test/app/o/argon/",
      ClientId = "argon-cli",
      Scope = "openid profile offline_access",
    };
    _flow = new DeviceCodeFlow(_authClient, options);

    _credentialsDirectory = Path.Combine(Path.GetTempPath(), "argon-test-" + Guid.NewGuid().ToString("N"));
    _store = new TokenStore(Path.Combine(_credentialsDirectory, "credentials.json"));

    _apiFake = new FakeHttpMessageHandler();
    _sut = new AuthenticatedHttpHandler(_store, _flow) { InnerHandler = _apiFake };
    _apiClient = new HttpClient(_sut) { BaseAddress = new Uri("http://api.test/") };
  }

  [TearDown]
  public void TearDown()
  {
    _apiClient.Dispose();
    _sut.Dispose();
    _authClient.Dispose();
    _apiFake.Dispose();
    _authFake.Dispose();
    try
    {
      if (Directory.Exists(_credentialsDirectory))
      {
        Directory.Delete(_credentialsDirectory, recursive: true);
      }
    }
    catch
    {
      // best-effort cleanup
    }
  }

  private FakeHttpMessageHandler _authFake = null!;
  private HttpClient _authClient = null!;
  private DeviceCodeFlow _flow = null!;
  private FakeHttpMessageHandler _apiFake = null!;
  private AuthenticatedHttpHandler _sut = null!;
  private HttpClient _apiClient = null!;
  private TokenStore _store = null!;
  private string _credentialsDirectory = null!;

  [Test]
  public async Task SendAsync_ShouldThrow_WhenNoCredentialsAreStored()
  {
    // act
    Func<Task> act = () => _apiClient.GetAsync("things");

    // assert
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("*Not signed in*");
    _apiFake.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task SendAsync_ShouldThrow_WhenStoredAccessTokenIsEmpty()
  {
    // arrange
    _store.Save(new TokenSet
    {
      AccessToken = "",
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });

    // act
    Func<Task> act = () => _apiClient.GetAsync("things");

    // assert
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("*Not signed in*");
  }

  [Test]
  public async Task SendAsync_ShouldAttachBearerHeader_WhenStoredTokenIsValid()
  {
    // arrange
    _store.Save(new TokenSet
    {
      AccessToken = "my-token",
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });
    _apiFake.EnqueueEmpty();

    // act
    HttpResponseMessage response = await _apiClient.GetAsync("things");

    // assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    _apiFake.Requests.Should().ContainSingle();
    _apiFake.Requests[0].AuthorizationHeader.Should().Be("Bearer my-token");
  }

  [Test]
  public async Task SendAsync_ShouldProactivelyRefresh_WhenTokenIsExpiringSoonAndRefreshTokenIsPresent()
  {
    // arrange
    _store.Save(new TokenSet
    {
      AccessToken = "old-token",
      RefreshToken = "rt-1",
      ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(10),
    });
    _authFake.EnqueueRaw(HttpStatusCode.OK,
      """{"access_token":"fresh-token","refresh_token":"rt-2","token_type":"Bearer","expires_in":3600}""");
    _apiFake.EnqueueEmpty();

    // act
    HttpResponseMessage response = await _apiClient.GetAsync("things");

    // assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    _apiFake.Requests.Should().ContainSingle("the refresh happens before the API call so only one inner call");
    _apiFake.Requests[0].AuthorizationHeader.Should().Be("Bearer fresh-token");
    _authFake.Requests.Should().ContainSingle();
    _authFake.Requests[0].Body.Should().Contain("grant_type=refresh_token");
    _authFake.Requests[0].Body.Should().Contain("refresh_token=rt-1");
  }

  [Test]
  public async Task SendAsync_ShouldSkipProactiveRefresh_WhenNoRefreshTokenIsAvailable()
  {
    // arrange — expiring soon but no refresh token means we just use the stale token
    _store.Save(new TokenSet
    {
      AccessToken = "almost-expired",
      RefreshToken = null,
      ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(10),
    });
    _apiFake.EnqueueEmpty();

    // act
    HttpResponseMessage response = await _apiClient.GetAsync("things");

    // assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    _apiFake.Requests[0].AuthorizationHeader.Should().Be("Bearer almost-expired");
    _authFake.Requests.Should().BeEmpty("no refresh token = no proactive refresh attempt");
  }

  [Test]
  public async Task SendAsync_ShouldThrow_WhenProactiveRefreshFails()
  {
    // arrange
    _store.Save(new TokenSet
    {
      AccessToken = "old-token",
      RefreshToken = "rt-1",
      ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(10),
    });
    _authFake.EnqueueRaw(HttpStatusCode.BadRequest, """{"error":"invalid_grant"}""");

    // act
    Func<Task> act = () => _apiClient.GetAsync("things");

    // assert
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("*refresh token is no longer valid*");
    _apiFake.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task SendAsync_ShouldRefreshAndRetryOnce_WhenInitialResponseIs401AndRefreshTokenIsPresent()
  {
    // arrange — token looks valid by expiry, but the API decides otherwise
    _store.Save(new TokenSet
    {
      AccessToken = "stale-token",
      RefreshToken = "rt-1",
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });
    _apiFake.EnqueueEmpty(HttpStatusCode.Unauthorized);
    _authFake.EnqueueRaw(HttpStatusCode.OK,
      """{"access_token":"refreshed","refresh_token":"rt-2","token_type":"Bearer","expires_in":3600}""");
    _apiFake.EnqueueEmpty(HttpStatusCode.OK);

    // act
    HttpResponseMessage response = await _apiClient.GetAsync("things");

    // assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    _apiFake.Requests.Should().HaveCount(2,
      "the handler retries the same request once with the refreshed bearer");
    _apiFake.Requests[0].AuthorizationHeader.Should().Be("Bearer stale-token");
    _apiFake.Requests[1].AuthorizationHeader.Should().Be("Bearer refreshed");
  }

  [Test]
  public async Task SendAsync_ShouldReturnTheUnauthorizedResponse_WhenNoRefreshTokenIsAvailable()
  {
    // arrange
    _store.Save(new TokenSet
    {
      AccessToken = "stale-token",
      RefreshToken = null,
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });
    _apiFake.EnqueueEmpty(HttpStatusCode.Unauthorized);

    // act
    HttpResponseMessage response = await _apiClient.GetAsync("things");

    // assert
    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized,
      "without a refresh token the handler cannot recover and the caller sees the 401");
    _apiFake.Requests.Should().ContainSingle();
  }

  [Test]
  public async Task SendAsync_ShouldThrow_WhenReactiveRefreshFails()
  {
    // arrange
    _store.Save(new TokenSet
    {
      AccessToken = "stale-token",
      RefreshToken = "rt-1",
      ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
    });
    _apiFake.EnqueueEmpty(HttpStatusCode.Unauthorized);
    _authFake.EnqueueRaw(HttpStatusCode.BadRequest, """{"error":"invalid_grant"}""");

    // act
    Func<Task> act = () => _apiClient.GetAsync("things");

    // assert
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("*refresh token is no longer valid*");
  }

  [Test]
  public async Task SendAsync_ShouldPersistRefreshedTokens_ViaTheTokenStore()
  {
    // arrange
    _store.Save(new TokenSet
    {
      AccessToken = "old-token",
      RefreshToken = "rt-1",
      ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(10),
    });
    _authFake.EnqueueRaw(HttpStatusCode.OK,
      """{"access_token":"fresh-token","refresh_token":"rt-2","token_type":"Bearer","expires_in":3600}""");
    _apiFake.EnqueueEmpty();

    // act
    await _apiClient.GetAsync("things");

    // assert
    TokenSet? persisted = _store.Load();
    persisted.Should().NotBeNull();
    persisted!.AccessToken.Should().Be("fresh-token");
    persisted.RefreshToken.Should().Be("rt-2");
  }
}
