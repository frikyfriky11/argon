using System.Net;
using System.Net.Http;
using System.Text.Json;
using Argon.Cli.Generated;
using Argon.Cli.Tests.Infrastructure;

namespace Argon.Cli.Tests.Commands;

[NonParallelizable]
public class AccountsCommandTests
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
  public async Task List_ShouldCallGetAccountsWithoutDateFilters_WhenNoOptionsArePassed()
  {
    // arrange
    _harness.Handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = Guid.NewGuid(), Name = "Cash", Type = AccountType.Cash, IsFavourite = true, TotalAmount = 100m },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("accounts list");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdErr.Should().BeEmpty();
    _harness.Handler.Requests.Should().ContainSingle();
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be("/Accounts");
    _harness.Handler.Requests[0].Uri.Query.Should().BeEmpty();
  }

  [Test]
  public async Task List_ShouldPassFromAndToAsQueryParameters_WhenSupplied()
  {
    // arrange
    _harness.Handler.EnqueueJson(Array.Empty<AccountsGetListResponse>());

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      "accounts list --from 2024-01-01 --to 2024-12-31");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain("TotalAmountsFrom=2024-01-01T00%3A00%3A00");
    query.Should().Contain("TotalAmountsTo=2024-12-31T00%3A00%3A00");
  }

  [Test]
  public async Task List_ShouldRespectJsonOutputFormat_WhenOFlagIsPassed()
  {
    // arrange
    Guid id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    _harness.Handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = id, Name = "Cash", Type = AccountType.Cash, IsFavourite = false, TotalAmount = 0m },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("-o json accounts list");

    // assert
    result.ExitCode.Should().Be(0);
    JsonDocument doc = JsonDocument.Parse(result.StdOut);
    doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
    doc.RootElement[0].GetProperty("id").GetGuid().Should().Be(id);
    doc.RootElement[0].GetProperty("name").GetString().Should().Be("Cash");
    doc.RootElement[0].GetProperty("type").GetInt32().Should().Be((int)AccountType.Cash);
  }

  [Test]
  public async Task Get_ShouldCallGetAccountsById_WithTheGuidArgument()
  {
    // arrange
    Guid id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    _harness.Handler.EnqueueJson(new AccountsGetResponse
    {
      Id = id,
      Name = "Sparkasse",
      Type = AccountType.Cash,
      IsFavourite = true,
      TotalAmount = 0m,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"accounts get {id}");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be($"/Accounts/{id}");
  }

  [Test]
  public async Task Get_ShouldFail_WhenArgumentIsNotAGuid()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("accounts get not-a-guid");

    // assert
    result.ExitCode.Should().NotBe(0);
    _harness.Handler.Requests.Should().BeEmpty(
      "argument parsing failed before any HTTP call could be made");
  }

  [Test]
  public async Task Create_ShouldPostNameAndTypeAsJsonBody_WhenAllRequiredFlagsArePassed()
  {
    // arrange
    Guid created = Guid.Parse("33333333-3333-3333-3333-333333333333");
    _harness.Handler.EnqueueJson(new AccountsCreateResponse { Id = created });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("accounts create --name Groceries --type Expense");

    // assert
    result.ExitCode.Should().Be(0);
    CapturedRequest req = _harness.Handler.Requests[0];
    req.Method.Should().Be(HttpMethod.Post);
    req.Uri.AbsolutePath.Should().Be("/Accounts");
    req.Body.Should().NotBeNull();
    JsonDocument body = JsonDocument.Parse(req.Body!);
    body.RootElement.GetProperty("name").GetString().Should().Be("Groceries");
    body.RootElement.GetProperty("type").GetInt32().Should().Be((int)AccountType.Expense);
  }

  [Test]
  public async Task Create_ShouldFail_WhenNameFlagIsMissing()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("accounts create --type Cash");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("--name");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task Create_ShouldFail_WhenTypeFlagIsMissing()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("accounts create --name X");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("--type");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task Create_ShouldFail_WhenTypeIsNotAValidEnumValue()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("accounts create --name X --type Bogus");

    // assert
    result.ExitCode.Should().NotBe(0);
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task Update_ShouldPutNewNameAndType_ToAccountsId()
  {
    // arrange
    Guid id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"accounts update {id} --name Renamed --type Revenue");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("updated.");
    CapturedRequest req = _harness.Handler.Requests[0];
    req.Method.Should().Be(HttpMethod.Put);
    req.Uri.AbsolutePath.Should().Be($"/Accounts/{id}");
    JsonDocument body = JsonDocument.Parse(req.Body!);
    body.RootElement.GetProperty("name").GetString().Should().Be("Renamed");
    body.RootElement.GetProperty("type").GetInt32().Should().Be((int)AccountType.Revenue);
  }

  [Test]
  public async Task Delete_ShouldCallDeleteEndpoint_AndPrintConfirmation()
  {
    // arrange
    Guid id = Guid.Parse("55555555-5555-5555-5555-555555555555");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.OK);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"accounts delete {id}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("deleted.");
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be($"/Accounts/{id}");
  }

  [Test]
  public async Task Favourite_ShouldDefaultToTrue_WhenNoExplicitValueIsGiven()
  {
    // arrange
    Guid id = Guid.Parse("66666666-6666-6666-6666-666666666666");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"accounts favourite {id}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("updated.");
    CapturedRequest req = _harness.Handler.Requests[0];
    req.Method.Should().Be(HttpMethod.Put);
    req.Uri.AbsolutePath.Should().Be($"/Accounts/{id}/Favourite");
    JsonDocument body = JsonDocument.Parse(req.Body!);
    body.RootElement.GetProperty("isFavourite").GetBoolean().Should().BeTrue();
  }

  [Test]
  public async Task Favourite_ShouldSendFalse_WhenExplicitlyDisabled()
  {
    // arrange
    Guid id = Guid.Parse("77777777-7777-7777-7777-777777777777");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"accounts favourite {id} --is-favourite false");

    // assert
    result.ExitCode.Should().Be(0);
    JsonDocument body = JsonDocument.Parse(_harness.Handler.Requests[0].Body!);
    body.RootElement.GetProperty("isFavourite").GetBoolean().Should().BeFalse();
  }

  [Test]
  public async Task Favourite_ShouldAlsoAcceptTheFavouriteAlias()
  {
    // arrange
    Guid id = Guid.Parse("88888888-8888-8888-8888-888888888888");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"accounts favourite {id} --favourite false");

    // assert
    result.ExitCode.Should().Be(0);
    JsonDocument body = JsonDocument.Parse(_harness.Handler.Requests[0].Body!);
    body.RootElement.GetProperty("isFavourite").GetBoolean().Should().BeFalse();
  }
}
