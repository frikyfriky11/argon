using System.Net.Http;
using Argon.Cli.Generated;
using Argon.Cli.Tests.Infrastructure;

namespace Argon.Cli.Tests;

public class ReferenceResolverTests
{
  [SetUp]
  public void SetUp()
  {
    _handler = new FakeHttpMessageHandler();
    _httpClient = new HttpClient(_handler) { BaseAddress = new Uri("http://test.local/") };
    _accounts = new AccountsClient(_httpClient);
    _counterparties = new CounterpartiesClient(_httpClient);
    _sut = new ReferenceResolver(_accounts, _counterparties);
  }

  [TearDown]
  public void TearDown()
  {
    _httpClient.Dispose();
    _handler.Dispose();
  }

  private FakeHttpMessageHandler _handler = null!;
  private HttpClient _httpClient = null!;
  private AccountsClient _accounts = null!;
  private CounterpartiesClient _counterparties = null!;
  private ReferenceResolver _sut = null!;

  [Test]
  public async Task ResolveAccountAsync_ShouldReturnTheParsedGuid_WhenInputIsAGuid_WithoutAnyHttpCall()
  {
    // arrange
    Guid id = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // act
    Guid result = await _sut.ResolveAccountAsync(id.ToString(), CancellationToken.None);

    // assert
    result.Should().Be(id);
    _handler.Requests.Should().BeEmpty(
      "a literal GUID short-circuits the resolver and never touches the API");
  }

  [Test]
  public async Task ResolveAccountAsync_ShouldFetchAccountListAndMatchByName_WhenInputIsNotAGuid()
  {
    // arrange
    Guid expected = Guid.Parse("22222222-2222-2222-2222-222222222222");
    _handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = expected, Name = "Sparkasse", Type = AccountType.Cash },
      new AccountsGetListResponse { Id = Guid.NewGuid(), Name = "Cash", Type = AccountType.Cash },
    });

    // act
    Guid result = await _sut.ResolveAccountAsync("Sparkasse", CancellationToken.None);

    // assert
    result.Should().Be(expected);
    _handler.Requests.Should().ContainSingle();
    _handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _handler.Requests[0].Uri.AbsolutePath.Should().Be("/Accounts");
  }

  [Test]
  public async Task ResolveAccountAsync_ShouldMatchCaseInsensitively()
  {
    // arrange
    Guid expected = Guid.Parse("33333333-3333-3333-3333-333333333333");
    _handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = expected, Name = "Sparkasse", Type = AccountType.Cash },
    });

    // act
    Guid result = await _sut.ResolveAccountAsync("SPARKASSE", CancellationToken.None);

    // assert
    result.Should().Be(expected);
  }

  [Test]
  public async Task ResolveAccountAsync_ShouldCacheTheAccountList_AcrossCallsOnTheSameInstance()
  {
    // arrange
    Guid id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    _handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = id, Name = "Sparkasse", Type = AccountType.Cash },
    });

    // act
    await _sut.ResolveAccountAsync("Sparkasse", CancellationToken.None);
    await _sut.ResolveAccountAsync("Sparkasse", CancellationToken.None);
    await _sut.ResolveAccountAsync("Sparkasse", CancellationToken.None);

    // assert
    _handler.Requests.Should().ContainSingle(
      "subsequent resolutions are served from the in-memory cache");
  }

  [Test]
  public async Task ResolveAccountAsync_ShouldThrow_WhenNoAccountMatchesByName()
  {
    // arrange
    _handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = Guid.NewGuid(), Name = "Cash", Type = AccountType.Cash },
    });

    // act
    Func<Task> act = () => _sut.ResolveAccountAsync("DoesNotExist", CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*No account matching 'DoesNotExist'*");
  }

  [Test]
  public async Task ResolveAccountAsync_ShouldThrow_WhenMultipleAccountsMatchTheSameName()
  {
    // arrange
    Guid first = Guid.Parse("55555555-5555-5555-5555-555555555555");
    Guid second = Guid.Parse("66666666-6666-6666-6666-666666666666");
    _handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = first, Name = "Savings", Type = AccountType.Cash },
      new AccountsGetListResponse { Id = second, Name = "Savings", Type = AccountType.Cash },
    });

    // act
    Func<Task> act = () => _sut.ResolveAccountAsync("Savings", CancellationToken.None);

    // assert
    (await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*Multiple account entries match 'Savings'*"))
      .Which.Message.Should().ContainAll(first.ToString(), second.ToString());
  }

  [Test]
  public async Task ResolveCounterpartyAsync_ShouldReturnTheParsedGuid_WhenInputIsAGuid()
  {
    // arrange
    Guid id = Guid.Parse("77777777-7777-7777-7777-777777777777");

    // act
    Guid result = await _sut.ResolveCounterpartyAsync(id.ToString(), CancellationToken.None);

    // assert
    result.Should().Be(id);
    _handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task ResolveCounterpartyAsync_ShouldFetchAllCounterparties_WithPageSizeNegativeOne()
  {
    // arrange
    Guid expected = Guid.Parse("88888888-8888-8888-8888-888888888888");
    _handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[] { new CounterpartiesGetListResponse { Id = expected, Name = "Amazon" } },
      PageNumber = 1, TotalPages = 1, TotalCount = 1,
    });

    // act
    Guid result = await _sut.ResolveCounterpartyAsync("Amazon", CancellationToken.None);

    // assert
    result.Should().Be(expected);
    _handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _handler.Requests[0].Uri.AbsolutePath.Should().Be("/Counterparties");
    _handler.Requests[0].Uri.Query.Should().Contain("PageSize=-1",
      "the resolver bypasses pagination so it sees every counterparty");
  }

  [Test]
  public async Task ResolveCounterpartyAsync_ShouldThrow_WhenNoMatch()
  {
    // arrange
    _handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = Array.Empty<CounterpartiesGetListResponse>(),
      PageNumber = 1, TotalPages = 0, TotalCount = 0,
    });

    // act
    Func<Task> act = () => _sut.ResolveCounterpartyAsync("Ghost", CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*No counterparty matching 'Ghost'*");
  }

  [Test]
  public async Task ResolveCounterpartyAsync_ShouldMatchOnSubstring_WhenNoExactMatchAndExactlyOneContains()
  {
    // arrange — `Athesia` should resolve `Athesia Buch` without a separate cp list|grep
    Guid expected = Guid.Parse("99999999-9999-9999-9999-999999999999");
    _handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[]
      {
        new CounterpartiesGetListResponse { Id = expected, Name = "Athesia Buch" },
        new CounterpartiesGetListResponse { Id = Guid.NewGuid(), Name = "Eurospar" },
      },
      PageNumber = 1, TotalPages = 1, TotalCount = 2,
    });

    // act
    Guid result = await _sut.ResolveCounterpartyAsync("Athesia", CancellationToken.None);

    // assert
    result.Should().Be(expected);
  }

  [Test]
  public async Task ResolveCounterpartyAsync_ShouldPreferExactMatch_OverSubstringCandidates()
  {
    // arrange — an exact "Amazon" wins even though "Amazon Web Services" also contains it
    Guid exact = Guid.Parse("aaaaaaaa-0000-0000-0000-aaaaaaaaaaaa");
    _handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[]
      {
        new CounterpartiesGetListResponse { Id = exact, Name = "Amazon" },
        new CounterpartiesGetListResponse { Id = Guid.NewGuid(), Name = "Amazon Web Services" },
      },
      PageNumber = 1, TotalPages = 1, TotalCount = 2,
    });

    // act
    Guid result = await _sut.ResolveCounterpartyAsync("Amazon", CancellationToken.None);

    // assert
    result.Should().Be(exact);
  }

  [Test]
  public async Task ResolveCounterpartyAsync_ShouldThrowDisambiguation_WhenMultipleSubstringMatches()
  {
    // arrange
    Guid first = Guid.Parse("11111111-aaaa-aaaa-aaaa-111111111111");
    Guid second = Guid.Parse("22222222-aaaa-aaaa-aaaa-222222222222");
    _handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[]
      {
        new CounterpartiesGetListResponse { Id = first, Name = "Athesia Buch" },
        new CounterpartiesGetListResponse { Id = second, Name = "Athesia Druck" },
      },
      PageNumber = 1, TotalPages = 1, TotalCount = 2,
    });

    // act
    Func<Task> act = () => _sut.ResolveCounterpartyAsync("Athesia", CancellationToken.None);

    // assert
    (await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*Multiple counterparty entries match 'Athesia'*"))
      .Which.Message.Should().ContainAll("Athesia Buch", "Athesia Druck");
  }

  [Test]
  public async Task ResolveCounterpartyAsync_ShouldNotFallBackToSubstring_WhenExactIsRequested()
  {
    // arrange
    _handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[] { new CounterpartiesGetListResponse { Id = Guid.NewGuid(), Name = "Athesia Buch" } },
      PageNumber = 1, TotalPages = 1, TotalCount = 1,
    });

    // act
    Func<Task> act = () => _sut.ResolveCounterpartyAsync("Athesia", CancellationToken.None, exact: true);

    // assert
    await act.Should().ThrowAsync<ArgumentException>()
      .WithMessage("*No counterparty matching 'Athesia'*");
  }
}
