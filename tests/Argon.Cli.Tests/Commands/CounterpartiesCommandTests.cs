using System.Net;
using System.Net.Http;
using System.Text.Json;
using Argon.Cli.Generated;
using Argon.Cli.Tests.Infrastructure;

namespace Argon.Cli.Tests.Commands;

[NonParallelizable]
public class CounterpartiesCommandTests
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
  public async Task List_ShouldCallGetCounterpartiesWithoutFilters_WhenNoOptionsArePassed()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("counterparties list");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be("/Counterparties");
    _harness.Handler.Requests[0].Uri.Query.Should().BeEmpty();
  }

  [Test]
  public async Task List_ShouldPassNameFilterAndPagination_WhenFlagsAreSupplied()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      "counterparties list --name Amazon --page 2 --page-size 50");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain("Name=Amazon");
    query.Should().Contain("PageNumber=2");
    query.Should().Contain("PageSize=50");
  }

  [Test]
  public async Task List_ShouldPrintTablePaginationFooter_WhenOutputIsTable()
  {
    // arrange
    _harness.Handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[]
      {
        new CounterpartiesGetListResponse { Id = Guid.NewGuid(), Name = "Amazon" },
      },
      PageNumber = 1,
      TotalPages = 3,
      TotalCount = 42,
      HasPreviousPage = false,
      HasNextPage = true,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("counterparties list");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("page 1/3");
    result.StdOut.Should().Contain("(42 total)");
  }

  [Test]
  public async Task List_ShouldNotPrintPaginationFooter_WhenOutputIsJson()
  {
    // arrange
    _harness.Handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[] { new CounterpartiesGetListResponse { Id = Guid.NewGuid(), Name = "X" } },
      PageNumber = 1, TotalPages = 1, TotalCount = 1,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("-o json counterparties list");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().NotContain("page ");
    result.StdOut.Should().NotContain("total");
  }

  [Test]
  public async Task Get_ShouldCallGetCounterpartiesById_WithTheGuidArgument()
  {
    // arrange
    Guid id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    _harness.Handler.EnqueueJson(new CounterpartiesGetResponse { Id = id, Name = "Amazon" });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"counterparties get {id}");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be($"/Counterparties/{id}");
  }

  [Test]
  public async Task Create_ShouldPostNameAsJsonBody_WhenNameFlagIsSupplied()
  {
    // arrange
    Guid created = Guid.Parse("22222222-2222-2222-2222-222222222222");
    _harness.Handler.EnqueueJson(new CounterpartiesCreateResponse { Id = created });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("counterparties create --name \"Mein Beck\"");

    // assert
    result.ExitCode.Should().Be(0);
    CapturedRequest req = _harness.Handler.Requests[0];
    req.Method.Should().Be(HttpMethod.Post);
    req.Uri.AbsolutePath.Should().Be("/Counterparties");
    JsonDocument body = JsonDocument.Parse(req.Body!);
    body.RootElement.GetProperty("name").GetString().Should().Be("Mein Beck");
  }

  [Test]
  public async Task Create_ShouldFail_WhenNameFlagIsMissing()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("counterparties create");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("--name");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task Update_ShouldPutNewName_ToCounterpartiesId()
  {
    // arrange
    Guid id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"counterparties update {id} --name NewName");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("updated.");
    CapturedRequest req = _harness.Handler.Requests[0];
    req.Method.Should().Be(HttpMethod.Put);
    req.Uri.AbsolutePath.Should().Be($"/Counterparties/{id}");
    JsonDocument body = JsonDocument.Parse(req.Body!);
    body.RootElement.GetProperty("name").GetString().Should().Be("NewName");
  }

  [Test]
  public async Task Delete_ShouldCallDeleteEndpoint_AndPrintConfirmation()
  {
    // arrange
    Guid id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.OK);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"counterparties delete {id}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("deleted.");
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be($"/Counterparties/{id}");
  }

  [Test]
  public async Task CpAlias_ShouldBeAcceptedInPlaceOfCounterparties()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("cp list");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be("/Counterparties");
  }

  private static PaginatedListOfCounterpartiesGetListResponse EmptyPage() => new()
  {
    Items = Array.Empty<CounterpartiesGetListResponse>(),
    PageNumber = 1,
    TotalPages = 0,
    TotalCount = 0,
  };
}
