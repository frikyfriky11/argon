using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Argon.Cli.Tests.Infrastructure;

/// <summary>
///   Test double for HttpMessageHandler. Tests enqueue canned responses in the order
///   that the generated NSwag client is expected to make calls; the handler records
///   every outgoing request so assertions can inspect URLs, methods, and bodies.
/// </summary>
internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
  private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responders = new();

  public List<CapturedRequest> Requests { get; } = new();

  public void EnqueueJson<T>(T payload, HttpStatusCode status = HttpStatusCode.OK)
  {
    string body = JsonSerializer.Serialize(payload);
    _responders.Enqueue(_ => new HttpResponseMessage(status)
    {
      Content = new StringContent(body, Encoding.UTF8, "application/json"),
    });
  }

  public void EnqueueEmpty(HttpStatusCode status = HttpStatusCode.OK)
  {
    _responders.Enqueue(_ => new HttpResponseMessage(status));
  }

  public void EnqueueRaw(HttpStatusCode status, string body, string contentType = "application/json")
  {
    _responders.Enqueue(_ => new HttpResponseMessage(status)
    {
      Content = new StringContent(body, Encoding.UTF8, contentType),
    });
  }

  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request, CancellationToken cancellationToken)
  {
    string? body = request.Content is null
      ? null
      : await request.Content.ReadAsStringAsync(cancellationToken);

    Requests.Add(new CapturedRequest(
      request.Method,
      request.RequestUri ?? new Uri("about:blank"),
      body,
      request.Headers.Authorization?.ToString()));

    if (_responders.Count == 0)
    {
      throw new InvalidOperationException(
        $"FakeHttpMessageHandler received an unexpected request: {request.Method} {request.RequestUri}");
    }

    return _responders.Dequeue()(request);
  }
}

internal sealed record CapturedRequest(
  HttpMethod Method,
  Uri Uri,
  string? Body,
  string? AuthorizationHeader);
