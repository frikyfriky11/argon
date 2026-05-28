using System.Net;
using System.Net.Http;
using System.Text.Json;
using Argon.Cli.Generated;
using Argon.Cli.Tests.Infrastructure;

namespace Argon.Cli.Tests.Commands;

[NonParallelizable]
public class CounterpartyIdentifiersCommandTests
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

  [Test]
  public async Task List_ShouldCallGetCounterpartyIdentifiers_WithoutFiltersByDefault()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyIdentifierPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("counterparty-identifiers list");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests.Should().ContainSingle();
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be("/CounterpartyIdentifiers");
    _harness.Handler.Requests[0].Uri.Query.Should().BeEmpty();
  }

  [Test]
  public async Task List_ShouldPassResolvedCounterpartyIdAsQueryParameter_WhenCounterpartyIsAGuid()
  {
    // arrange
    Guid cpId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    _harness.Handler.EnqueueJson(EmptyIdentifierPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"counterparty-identifiers list --counterparty {cpId}");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests.Should().ContainSingle(
      "passing a GUID short-circuits ReferenceResolver and no lookup call is needed");
    _harness.Handler.Requests[0].Uri.Query.Should().Contain($"CounterpartyId={cpId}");
  }

  [Test]
  public async Task List_ShouldLookUpCounterpartyByName_BeforeIssuingTheListCall()
  {
    // arrange
    Guid cpId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    _harness.Handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[] { new CounterpartiesGetListResponse { Id = cpId, Name = "Amazon" } },
      PageNumber = 1, TotalPages = 1, TotalCount = 1,
    });
    _harness.Handler.EnqueueJson(EmptyIdentifierPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("counterparty-identifiers list --counterparty Amazon");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests.Should().HaveCount(2,
      "ReferenceResolver fetches the counterparties list once before resolving the name");
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be("/Counterparties");
    _harness.Handler.Requests[1].Uri.AbsolutePath.Should().Be("/CounterpartyIdentifiers");
    _harness.Handler.Requests[1].Uri.Query.Should().Contain($"CounterpartyId={cpId}");
  }

  [Test]
  public async Task List_ShouldPassTextFilterAndPagination_WhenFlagsAreSupplied()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyIdentifierPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      "counterparty-identifiers list --text IT60 --page 3 --page-size 100");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain("IdentifierText=IT60");
    query.Should().Contain("PageNumber=3");
    query.Should().Contain("PageSize=100");
  }

  [Test]
  public async Task Get_ShouldCallGetCounterpartyIdentifiersById()
  {
    // arrange
    Guid id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    _harness.Handler.EnqueueJson(new CounterpartyIdentifiersGetResponse
    {
      Id = id, CounterpartyId = Guid.NewGuid(), IdentifierText = "IT60X...",
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"counterparty-identifiers get {id}");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be($"/CounterpartyIdentifiers/{id}");
  }

  [Test]
  public async Task Create_ShouldResolveCounterpartyByName_ThenPostIdentifier()
  {
    // arrange
    Guid cpId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    Guid created = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    _harness.Handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[] { new CounterpartiesGetListResponse { Id = cpId, Name = "Amazon" } },
      PageNumber = 1, TotalPages = 1, TotalCount = 1,
    });
    _harness.Handler.EnqueueJson(new CounterpartyIdentifiersCreateResponse { Id = created });

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      "counterparty-identifiers create --counterparty Amazon --text AMAZON.EU");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests.Should().HaveCount(2);
    CapturedRequest postRequest = _harness.Handler.Requests[1];
    postRequest.Method.Should().Be(HttpMethod.Post);
    postRequest.Uri.AbsolutePath.Should().Be("/CounterpartyIdentifiers");
    JsonDocument body = JsonDocument.Parse(postRequest.Body!);
    body.RootElement.GetProperty("counterpartyId").GetGuid().Should().Be(cpId);
    body.RootElement.GetProperty("identifierText").GetString().Should().Be("AMAZON.EU");
  }

  [Test]
  public async Task Create_ShouldFail_WhenCounterpartyNameDoesNotResolve()
  {
    // arrange
    _harness.Handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = Array.Empty<CounterpartiesGetListResponse>(),
      PageNumber = 1, TotalPages = 0, TotalCount = 0,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      "counterparty-identifiers create --counterparty UnknownCp --text foo");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("UnknownCp");
  }

  [Test]
  public async Task Update_ShouldPutCounterpartyAndText_ToIdentifiersId()
  {
    // arrange
    Guid id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    Guid cpId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"counterparty-identifiers update {id} --counterparty {cpId} --text NEW");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("updated.");
    CapturedRequest req = _harness.Handler.Requests[0];
    req.Method.Should().Be(HttpMethod.Put);
    req.Uri.AbsolutePath.Should().Be($"/CounterpartyIdentifiers/{id}");
    JsonDocument body = JsonDocument.Parse(req.Body!);
    body.RootElement.GetProperty("counterpartyId").GetGuid().Should().Be(cpId);
    body.RootElement.GetProperty("identifierText").GetString().Should().Be("NEW");
  }

  [Test]
  public async Task Resolve_ShouldPostRawTextAsBody_AndReturnMatches()
  {
    // arrange
    Guid cpId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    _harness.Handler.EnqueueJson(new[]
    {
      new CounterpartyIdentifiersResolveResponse
      {
        CounterpartyId = cpId, CounterpartyName = "Amazon",
        MatchedByIdentifier = true, MatchedByName = false,
      },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      "-o json counterparty-identifiers resolve \"AMAZON EU SARL\"");

    // assert
    result.ExitCode.Should().Be(0);
    CapturedRequest req = _harness.Handler.Requests[0];
    req.Method.Should().Be(HttpMethod.Post);
    req.Uri.AbsolutePath.Should().Be("/CounterpartyIdentifiers/resolve");
    JsonDocument body = JsonDocument.Parse(req.Body!);
    body.RootElement.GetProperty("rawText").GetString().Should().Be("AMAZON EU SARL");
    JsonDocument outDoc = JsonDocument.Parse(result.StdOut);
    outDoc.RootElement[0].GetProperty("counterpartyName").GetString().Should().Be("Amazon");
  }

  [Test]
  public async Task Delete_ShouldCallDeleteEndpoint_AndPrintConfirmation()
  {
    // arrange
    Guid id = Guid.Parse("55555555-5555-5555-5555-555555555555");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.OK);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"counterparty-identifiers delete {id}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("deleted.");
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be($"/CounterpartyIdentifiers/{id}");
  }

  [Test]
  public async Task CpiAlias_ShouldBeAcceptedInPlaceOfCounterpartyIdentifiers()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyIdentifierPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("cpi list");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be("/CounterpartyIdentifiers");
  }

  private static PaginatedListOfCounterpartyIdentifiersGetListResponse EmptyIdentifierPage() => new()
  {
    Items = Array.Empty<CounterpartyIdentifiersGetListResponse>(),
    PageNumber = 1,
    TotalPages = 0,
    TotalCount = 0,
  };
}
