using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Argon.Cli.Commands;
using Argon.Cli.Generated;
using Argon.Cli.Tests.Infrastructure;

namespace Argon.Cli.Tests.Commands;

[NonParallelizable]
public class TransactionsCommandTests
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

  // ----- list -----

  [Test]
  public async Task List_ShouldCallGetTransactionsWithoutFilters_WhenNoOptionsArePassed()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be("/Transactions");
  }

  [Test]
  public async Task List_ShouldPassAccountIdsAsRepeatedQueryParams_WhenAccountIsAGuid()
  {
    // arrange
    Guid a1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa11");
    Guid a2 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa22");
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx list --account {a1} --account {a2}");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain($"AccountIds={a1}");
    query.Should().Contain($"AccountIds={a2}");
  }

  [Test]
  public async Task List_ShouldLookUpAccountByName_BeforeIssuingTheListCall()
  {
    // arrange
    Guid accountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    _harness.Handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = accountId, Name = "Sparkasse", Type = AccountType.Cash },
    });
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --account Sparkasse");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests.Should().HaveCount(2);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be("/Accounts");
    _harness.Handler.Requests[1].Uri.Query.Should().Contain($"AccountIds={accountId}");
  }

  [TestCase("pending", TransactionStatus.PendingImportReview)]
  [TestCase("pendingimportreview", TransactionStatus.PendingImportReview)]
  [TestCase("pending-import-review", TransactionStatus.PendingImportReview)]
  [TestCase("confirmed", TransactionStatus.Confirmed)]
  [TestCase("duplicate", TransactionStatus.PotentialDuplicate)]
  [TestCase("potentialduplicate", TransactionStatus.PotentialDuplicate)]
  [TestCase("potential-duplicate", TransactionStatus.PotentialDuplicate)]
  public async Task List_ShouldMapStatusSynonyms_ToTheGeneratedEnum(string synonym, TransactionStatus expected)
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx list --status {synonym}");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Uri.Query.Should().Contain($"Status={(int)expected}");
  }

  [Test]
  public async Task List_ShouldFail_WhenStatusIsAnUnknownSynonym()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --status bogus");

    // assert
    result.ExitCode.Should().NotBe(0);
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task List_ShouldPassDateFiltersAndPagination_WhenAllFlagsAreSupplied()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      "tx list --from 2024-01-01 --to 2024-12-31 --page 2 --page-size 50");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain("DateFrom=2024-01-01");
    query.Should().Contain("DateTo=2024-12-31");
    query.Should().Contain("PageNumber=2");
    query.Should().Contain("PageSize=50");
  }

  [Test]
  public async Task List_ShouldPrintTablePaginationFooter_WhenOutputIsTable()
  {
    // arrange
    _harness.Handler.EnqueueJson(new PaginatedListOfTransactionsGetListResponse
    {
      Items = Array.Empty<TransactionsGetListResponse>(),
      PageNumber = 1, TotalPages = 5, TotalCount = 123,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("page 1/5");
    result.StdOut.Should().Contain("(123 total)");
  }

  [Test]
  public async Task List_ShouldExpandMonthToFirstAndLastDay_WhenMonthIsSupplied()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --month 2025-02");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain("DateFrom=2025-02-01");
    query.Should().Contain("DateTo=2025-02-28");
  }

  [Test]
  public async Task List_ShouldUseEndOfMonthAwareRange_ForMonthsOfDifferingLength()
  {
    // arrange — January has 31 days; the range must not spill into February
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --month 2025-01");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain("DateFrom=2025-01-01");
    query.Should().Contain("DateTo=2025-01-31");
  }

  [Test]
  public async Task List_ShouldRejectMonth_WhenCombinedWithExplicitFromOrTo()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --month 2025-02 --from 2025-01-01");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("--month cannot be combined with --from/--to");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task List_ShouldFail_WhenMonthIsMalformed()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --month 2025-13");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("Invalid --month");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [TestCase("current")]
  [TestCase("last")]
  public async Task List_ShouldAcceptRelativeMonthKeywords(string keyword)
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());
    (DateTimeOffset expectedFrom, DateTimeOffset expectedTo) = TransactionsCommand.MonthToRange(keyword);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx list --month {keyword}");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain($"DateFrom={expectedFrom:yyyy-MM-dd}");
    query.Should().Contain($"DateTo={expectedTo:yyyy-MM-dd}");
  }

  [Test]
  public async Task GlobalOutputOption_ShouldBeAccepted_AfterTheSubcommand()
  {
    // arrange — the trailing position used to error with "Unrecognized command or argument '-o'"
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list -o json");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().StartWith("[");
  }

  [Test]
  public async Task GlobalOutputOption_ShouldStillBeAccepted_BeforeTheSubcommand()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("-o json tx list");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().StartWith("[");
  }

  // ----- get -----

  [Test]
  public async Task Get_ShouldPrintHeaderLinesThenRowsTable_WhenOutputIsTable()
  {
    // arrange
    Guid id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    Guid cpId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = id,
      Date = new DateOnly(2024, 3, 14),
      CounterpartyId = cpId,
      CounterpartyName = "Amazon",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse
        {
          Id = Guid.NewGuid(), RowCounter = 1,
          AccountId = Guid.NewGuid(), AccountName = "Sparkasse",
          AccountType = AccountType.Cash, Credit = 9.99m,
        },
      },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx get {id}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain($"Id            : {id}");
    result.StdOut.Should().Contain("Date          : 2024-03-14");
    result.StdOut.Should().Contain($"Counterparty  : Amazon ({cpId})");
    result.StdOut.Should().Contain("Sparkasse");
  }

  [Test]
  public async Task Get_ShouldEmitFullResponseAsJson_WhenOutputIsJson()
  {
    // arrange
    Guid id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = id, Date = new DateOnly(2024, 3, 14),
      CounterpartyId = Guid.NewGuid(), CounterpartyName = "Amazon",
      TransactionRows = Array.Empty<TransactionRowsGetResponse>(),
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"-o json tx get {id}");

    // assert
    result.ExitCode.Should().Be(0);
    JsonDocument doc = JsonDocument.Parse(result.StdOut);
    doc.RootElement.GetProperty("id").GetGuid().Should().Be(id);
    doc.RootElement.GetProperty("counterpartyName").GetString().Should().Be("Amazon");
  }

  [Test]
  public async Task Get_ShouldExposeRawImportDataStatusAndDuplicateId_InJson_LikeList()
  {
    // arrange
    Guid id = Guid.Parse("4a4a4a4a-4a4a-4a4a-4a4a-4a4a4a4a4a4a");
    Guid dupId = Guid.Parse("5b5b5b5b-5b5b-5b5b-5b5b-5b5b5b5b5b5b");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = id, Date = new DateOnly(2024, 3, 14),
      CounterpartyId = Guid.NewGuid(), CounterpartyName = "Amazon",
      TransactionRows = Array.Empty<TransactionRowsGetResponse>(),
      RawImportData = "{\"Amount\":12.34,\"RawDescription\":\"amazon.it\"}",
      Status = TransactionStatus.PotentialDuplicate,
      PotentialDuplicateOfTransactionId = dupId,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"-o json tx get {id}");

    // assert
    result.ExitCode.Should().Be(0);
    JsonDocument doc = JsonDocument.Parse(result.StdOut);
    // rawImportData inner fields are lifted to the top level, exactly like tx list
    doc.RootElement.GetProperty("amount").GetDecimal().Should().Be(12.34m);
    doc.RootElement.GetProperty("rawDescription").GetString().Should().Be("amazon.it");
    doc.RootElement.GetProperty("status").GetInt32().Should().Be((int)TransactionStatus.PotentialDuplicate);
    doc.RootElement.GetProperty("potentialDuplicateOfTransactionId").GetGuid().Should().Be(dupId);
  }

  [Test]
  public async Task Get_ShouldPrintStatusLine_InTableMode()
  {
    // arrange
    Guid id = Guid.Parse("6c6c6c6c-6c6c-6c6c-6c6c-6c6c6c6c6c6c");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = id, Date = new DateOnly(2024, 3, 14),
      CounterpartyId = Guid.NewGuid(), CounterpartyName = "Amazon",
      TransactionRows = Array.Empty<TransactionRowsGetResponse>(),
      Status = TransactionStatus.PendingImportReview,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx get {id}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("Status        : PendingImportReview");
  }

  // ----- create -----

  [Test]
  public async Task Create_ShouldBuildJsonBodyWithResolvedRefsAndParsedRows()
  {
    // arrange
    Guid cpId = Guid.Parse("aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa");
    Guid groceries = Guid.Parse("bbbbbbbb-2222-2222-2222-bbbbbbbbbbbb");
    Guid cash = Guid.Parse("cccccccc-3333-3333-3333-cccccccccccc");
    Guid created = Guid.Parse("dddddddd-4444-4444-4444-dddddddddddd");
    // The command resolves account names while parsing rows first, then the counterparty,
    // then issues the create — queue responses in that order.
    _harness.Handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = groceries, Name = "Groceries", Type = AccountType.Expense },
      new AccountsGetListResponse { Id = cash, Name = "Cash", Type = AccountType.Cash },
    });
    _harness.Handler.EnqueueJson(new PaginatedListOfCounterpartiesGetListResponse
    {
      Items = new[] { new CounterpartiesGetListResponse { Id = cpId, Name = "Eurospar" } },
      PageNumber = 1, TotalPages = 1, TotalCount = 1,
    });
    _harness.Handler.EnqueueJson(new TransactionsCreateResponse { Id = created });

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      "tx create --date 2024-03-14 --counterparty Eurospar -r Groceries:12.34:0:Bread -r Cash:0:12.34");

    // assert
    result.ExitCode.Should().Be(0);
    CapturedRequest post = _harness.Handler.Requests.Last();
    post.Method.Should().Be(HttpMethod.Post);
    post.Uri.AbsolutePath.Should().Be("/Transactions");

    JsonDocument body = JsonDocument.Parse(post.Body!);
    body.RootElement.GetProperty("date").GetString().Should().Be("2024-03-14");
    body.RootElement.GetProperty("counterpartyId").GetGuid().Should().Be(cpId);

    JsonElement rows = body.RootElement.GetProperty("transactionRows");
    rows.GetArrayLength().Should().Be(2);
    rows[0].GetProperty("rowCounter").GetInt32().Should().Be(1);
    rows[0].GetProperty("accountId").GetGuid().Should().Be(groceries);
    rows[0].GetProperty("debit").GetDecimal().Should().Be(12.34m);
    AssertNullOrAbsent(rows[0], "credit", "0 is normalised to null by ParseAmount");
    rows[0].GetProperty("description").GetString().Should().Be("Bread");
    rows[1].GetProperty("rowCounter").GetInt32().Should().Be(2);
    rows[1].GetProperty("accountId").GetGuid().Should().Be(cash);
    rows[1].GetProperty("credit").GetDecimal().Should().Be(12.34m);
    AssertNullOrAbsent(rows[1], "description",
      "the description is optional and absent when no fourth segment is supplied");
  }

  [Test]
  public async Task Create_ShouldParseDecimalsUsingInvariantCulture_RegardlessOfHostLocale()
  {
    // arrange — pin the host to it-IT (comma decimal separator) so that a regression
    // to decimal.Parse(raw) without an explicit culture would FormatException on the
    // dotted input below.
    CultureInfo previous = CultureInfo.CurrentCulture;
    CultureInfo.CurrentCulture = new CultureInfo("it-IT");
    try
    {
      Guid cpId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
      Guid accId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
      _harness.Handler.EnqueueJson(new TransactionsCreateResponse { Id = Guid.NewGuid() });

      // act
      CliInvocationResult result = await _harness.InvokeAsync(
        $"tx create --date 2024-03-14 --counterparty {cpId} -r {accId}:1234.56:0");

      // assert
      result.ExitCode.Should().Be(0);
      JsonDocument body = JsonDocument.Parse(_harness.Handler.Requests[0].Body!);
      body.RootElement.GetProperty("transactionRows")[0]
        .GetProperty("debit").GetDecimal().Should().Be(1234.56m);
    }
    finally
    {
      CultureInfo.CurrentCulture = previous;
    }
  }

  [Test]
  public async Task Create_ShouldFail_WhenRowHasFewerThanThreeSegments()
  {
    // arrange
    Guid accId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    Guid cpId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx create --date 2024-03-14 --counterparty {cpId} -r {accId}:10");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("Invalid row");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task Create_ShouldFail_WhenDateIsMissing()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      "tx create --counterparty cp -r acc:1:0");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("--date");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task Create_ShouldFail_WhenNoRowsArePassed()
  {
    // arrange
    Guid cpId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx create --date 2024-03-14 --counterparty {cpId}");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("--row");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  // ----- update -----

  [Test]
  public async Task Update_ShouldAcceptExistingAndNewRows_DistinguishedByThePrefix()
  {
    // arrange
    Guid txId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    Guid existingRow = Guid.Parse("66666666-6666-6666-6666-666666666666");
    Guid cpId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    Guid accId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx update {txId} --date 2024-03-14 --counterparty {cpId} " +
      $"-r {existingRow}:{accId}:5:0:edited -r :{accId}:0:5:new");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("updated.");
    CapturedRequest req = _harness.Handler.Requests[0];
    req.Method.Should().Be(HttpMethod.Put);
    req.Uri.AbsolutePath.Should().Be($"/Transactions/{txId}");

    JsonDocument body = JsonDocument.Parse(req.Body!);
    JsonElement rows = body.RootElement.GetProperty("transactionRows");
    rows[0].GetProperty("id").GetGuid().Should().Be(existingRow);
    rows[0].GetProperty("description").GetString().Should().Be("edited");
    AssertNullOrAbsent(rows[1], "id",
      "an empty rowId segment means a brand-new row and serialises as null");
    rows[1].GetProperty("description").GetString().Should().Be("new");
  }

  [Test]
  public async Task Update_ShouldFail_WhenRowHasFewerThanFourSegments()
  {
    // arrange
    Guid txId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    Guid cpId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    Guid accId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx update {txId} --date 2024-03-14 --counterparty {cpId} -r {accId}:5:0");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("Invalid row");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  // ----- categorize -----

  [Test]
  public async Task Categorize_ShouldAutoPickTheUniqueRowWithoutAnAccount_WhenRowFlagIsOmitted()
  {
    // arrange
    Guid txId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    Guid pendingRowId = Guid.Parse("12121212-1212-1212-1212-121212121212");
    Guid accId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2024, 3, 14), CounterpartyId = Guid.NewGuid(), CounterpartyName = "X",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = Guid.NewGuid(), Credit = 10m },
        new TransactionRowsGetResponse { Id = pendingRowId, RowCounter = 2, AccountId = null, Debit = 10m },
      },
    });
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx categorize {txId} --account {accId}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("categorized.");
    CapturedRequest patch = _harness.Handler.Requests[1];
    patch.Method.Should().Be(HttpMethod.Patch);
    patch.Uri.AbsolutePath.Should().Be($"/Transactions/{txId}/rows/{pendingRowId}");
    JsonDocument body = JsonDocument.Parse(patch.Body!);
    body.RootElement.GetProperty("accountId").GetGuid().Should().Be(accId);
  }

  [Test]
  public async Task Categorize_ShouldUseTheRowMatchingTheGivenCounter_WhenRowFlagIsPassed()
  {
    // arrange
    Guid txId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    Guid targetRowId = Guid.Parse("13131313-1313-1313-1313-131313131313");
    Guid accId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2024, 3, 14), CounterpartyId = Guid.NewGuid(), CounterpartyName = "X",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = Guid.NewGuid() },
        new TransactionRowsGetResponse { Id = targetRowId, RowCounter = 2, AccountId = Guid.NewGuid() },
      },
    });
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx categorize {txId} --row 2 --account {accId}");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[1].Uri.AbsolutePath.Should().Be($"/Transactions/{txId}/rows/{targetRowId}");
  }

  [Test]
  public async Task Categorize_ShouldFail_WhenTransactionHasNoRowsWithoutAnAccount()
  {
    // arrange
    Guid txId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    Guid accId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2024, 3, 14), CounterpartyId = Guid.NewGuid(), CounterpartyName = "X",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = Guid.NewGuid() },
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 2, AccountId = Guid.NewGuid() },
      },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx categorize {txId} --account {accId}");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("no row awaiting categorization");
  }

  [Test]
  public async Task Categorize_ShouldFail_WhenMultipleRowsAreWithoutAnAccount()
  {
    // arrange
    Guid txId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    Guid accId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2024, 3, 14), CounterpartyId = Guid.NewGuid(), CounterpartyName = "X",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = null },
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 2, AccountId = null },
      },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx categorize {txId} --account {accId}");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("Pass --row");
  }

  [Test]
  public async Task Categorize_ShouldFail_WhenRowCounterDoesNotMatchAnyRow()
  {
    // arrange
    Guid txId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    Guid accId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2024, 3, 14), CounterpartyId = Guid.NewGuid(), CounterpartyName = "X",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = null },
      },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx categorize {txId} --row 9 --account {accId}");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("No row with counter 9");
  }

  // ----- set-counterparty -----

  [Test]
  public async Task SetCounterparty_ShouldResolveAndPut_ToTransactionsIdCounterparty()
  {
    // arrange
    Guid txId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    Guid cpId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx set-counterparty {txId} {cpId}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("counterparty updated.");
    CapturedRequest req = _harness.Handler.Requests[0];
    req.Method.Should().Be(HttpMethod.Patch);
    req.Uri.AbsolutePath.Should().Be($"/Transactions/{txId}/counterparty");
    JsonDocument body = JsonDocument.Parse(req.Body!);
    body.RootElement.GetProperty("counterpartyId").GetGuid().Should().Be(cpId);
  }

  // ----- history -----

  [Test]
  public async Task History_ShouldHitCounterpartiesAccountHistoryEndpoint_WithResolvedId()
  {
    // arrange
    Guid cpId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    _harness.Handler.EnqueueJson(new[]
    {
      new CounterpartiesAccountHistoryResponse { AccountId = Guid.NewGuid(), AccountName = "Groceries", AccountType = AccountType.Expense, Count = 7 },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx history --counterparty {cpId}");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be($"/Counterparties/{cpId}/account-history");
    result.StdOut.Should().Contain("Groceries");
  }

  // ----- delete -----

  [Test]
  public async Task Delete_ShouldCallDeleteEndpoint_AndPrintConfirmation()
  {
    // arrange
    Guid id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    _harness.Handler.EnqueueEmpty(HttpStatusCode.OK);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx delete {id}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("deleted.");
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Delete);
    _harness.Handler.Requests[0].Uri.AbsolutePath.Should().Be($"/Transactions/{id}");
  }

  // ----- alias -----

  [Test]
  public async Task TransactionsAndTxAliases_ShouldBothBeAccepted()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult longForm = await _harness.InvokeAsync("transactions list");
    CliInvocationResult shortForm = await _harness.InvokeAsync("tx list");

    // assert
    longForm.ExitCode.Should().Be(0);
    shortForm.ExitCode.Should().Be(0);
    _harness.Handler.Requests.Should().AllSatisfy(r =>
      r.Uri.AbsolutePath.Should().Be("/Transactions"));
  }

  private static PaginatedListOfTransactionsGetListResponse EmptyTransactionPage() => new()
  {
    Items = Array.Empty<TransactionsGetListResponse>(),
    PageNumber = 1,
    TotalPages = 0,
    TotalCount = 0,
  };

  private static void AssertNullOrAbsent(JsonElement element, string property, string reason)
  {
    bool isNull = !element.TryGetProperty(property, out JsonElement value)
                  || value.ValueKind == JsonValueKind.Null;
    isNull.Should().BeTrue(reason);
  }
}
