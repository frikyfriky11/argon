using System.Net;

namespace Argon.Cli.Auth;

public sealed class AuthenticatedHttpHandler : DelegatingHandler
{
  private readonly TokenStore _store;
  private readonly DeviceCodeFlow _flow;

  public AuthenticatedHttpHandler(TokenStore store, DeviceCodeFlow flow)
  {
    _store = store;
    _flow = flow;
  }

  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, CancellationToken cancellationToken)
  {
    TokenSet tokens = LoadOrThrow();

    if (tokens.IsExpiringSoon && !string.IsNullOrEmpty(tokens.RefreshToken))
    {
      TokenSet? proactive = await TryRefresh(tokens.RefreshToken, cancellationToken);
      if (proactive is null)
      {
        throw new InvalidOperationException(
          "Authentication failed and refresh token is no longer valid. Run `argon login`.");
      }

      tokens = proactive;
    }

    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
    HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

    if (response.StatusCode != HttpStatusCode.Unauthorized || string.IsNullOrEmpty(tokens.RefreshToken))
    {
      return response;
    }

    response.Dispose();
    TokenSet? refreshed = await TryRefresh(tokens.RefreshToken, cancellationToken);
    if (refreshed is null)
    {
      throw new InvalidOperationException(
        "Authentication failed and refresh token is no longer valid. Run `argon login`.");
    }

    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
    return await base.SendAsync(request, cancellationToken);
  }

  private TokenSet LoadOrThrow()
  {
    TokenSet? tokens = _store.Load();
    if (tokens is null || string.IsNullOrEmpty(tokens.AccessToken))
    {
      throw new InvalidOperationException("Not signed in. Run `argon login` first.");
    }

    return tokens;
  }

  private async Task<TokenSet?> TryRefresh(string refreshToken, CancellationToken ct)
  {
    try
    {
      TokenSet refreshed = await _flow.RefreshAsync(refreshToken, ct);
      _store.Save(refreshed);
      return refreshed;
    }
    catch
    {
      return null;
    }
  }
}
