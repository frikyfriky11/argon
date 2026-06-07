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
  public async Task List_ShouldSendLinkedTrue_WhenLinkedFlagIsPassed()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --linked");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Uri.Query.Should().Contain("Linked=true");
  }

  [Test]
  public async Task List_ShouldSendLinkedFalse_WhenUnlinkedFlagIsPassed()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --unlinked");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Uri.Query.Should().Contain("Linked=false");
  }

  [Test]
  public async Task List_ShouldNotSendLinked_WhenNeitherFlagIsPassed()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Uri.Query.Should().NotContain("Linked");
  }

  [Test]
  public async Task List_ShouldFail_WhenLinkedAndUnlinkedAreBothPassed()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --linked --unlinked");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("--linked and --unlinked cannot be combined");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task List_ShouldDefaultToAccountingDateField()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --month 2025-11");

    // assert — DateField=1 is AccountingDate, the default
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Uri.Query.Should().Contain($"DateField={(int)TransactionDateField.AccountingDate}");
  }

  [Test]
  public async Task List_ShouldUseCurrencyDateField_WhenDateFieldDateIsRequested()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --month 2025-11 --date-field date");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Uri.Query.Should().Contain($"DateField={(int)TransactionDateField.Date}");
  }

  [Test]
  public async Task List_ShouldFail_WhenDateFieldIsUnknown()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx list --date-field bogus");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("Unknown date field");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task List_ShouldExposeAccountingDate_InJsonOutput()
  {
    // arrange
    _harness.Handler.EnqueueJson(new PaginatedListOfTransactionsGetListResponse
    {
      Items = new[]
      {
        new TransactionsGetListResponse
        {
          Id = Guid.NewGuid(), Date = new DateOnly(2025, 10, 30), AccountingDate = new DateOnly(2025, 11, 1),
          CounterpartyName = "Amazon", TransactionRows = Array.Empty<TransactionRowsGetListResponse>(),
        },
      },
      PageNumber = 1, TotalPages = 1, TotalCount = 1,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync("-o json tx list");

    // assert
    result.ExitCode.Should().Be(0);
    JsonDocument doc = JsonDocument.Parse(result.StdOut);
    doc.RootElement[0].GetProperty("accountingDate").GetString().Should().Be("2025-11-01");
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

  // ----- find -----

  [Test]
  public async Task Find_ShouldSendRowAmountAndTolerance_AndDefaultToAllMatches()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx find --amount 93.78 --tolerance 0.50");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain("RowAmount=93.78");
    query.Should().Contain("RowAmountTolerance=0.5");
    query.Should().Contain("PageSize=-1");
  }

  [Test]
  public async Task Find_ShouldRequireAmount()
  {
    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx find --tolerance 1");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("--amount");
    _harness.Handler.Requests.Should().BeEmpty();
  }

  [Test]
  public async Task Find_ShouldExpandMonth_IntoDateRange()
  {
    // arrange
    _harness.Handler.EnqueueJson(EmptyTransactionPage());

    // act
    CliInvocationResult result = await _harness.InvokeAsync("tx find --amount 10 --month 2025-11");

    // assert
    result.ExitCode.Should().Be(0);
    string query = _harness.Handler.Requests[0].Uri.Query;
    query.Should().Contain("DateFrom=2025-11-01");
    query.Should().Contain("DateTo=2025-11-30");
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

  [Test]
  public async Task Update_ShouldDefaultDateAndCounterparty_FromExistingTransaction_WhenOmitted()
  {
    // arrange
    Guid txId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    Guid rowId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    Guid cpId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    Guid accId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2025, 10, 9), CounterpartyId = cpId, CounterpartyName = "Amazon",
      TransactionRows = Array.Empty<TransactionRowsGetResponse>(),
    });
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act — no --date / --counterparty supplied
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx update {txId} -r {rowId}:{accId}:5:0:edited -r :{accId}:0:5:new");

    // assert
    result.ExitCode.Should().Be(0);
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
    CapturedRequest put = _harness.Handler.Requests[1];
    put.Method.Should().Be(HttpMethod.Put);
    JsonDocument body = JsonDocument.Parse(put.Body!);
    body.RootElement.GetProperty("date").GetString().Should().Be("2025-10-09");
    body.RootElement.GetProperty("counterpartyId").GetGuid().Should().Be(cpId);
  }

  // ----- patch -----

  [Test]
  public async Task Patch_ShouldUpdateOnlyTheNamedRow_AndPassOthersThroughVerbatim()
  {
    // arrange — a pending tx: cash leg filled, offsetting row blank
    Guid txId = Guid.Parse("aaaa1111-1111-1111-1111-111111111111");
    Guid cashRow = Guid.Parse("aaaa2222-2222-2222-2222-222222222222");
    Guid blankRow = Guid.Parse("aaaa3333-3333-3333-3333-333333333333");
    Guid cashAcc = Guid.Parse("aaaa4444-4444-4444-4444-444444444444");
    Guid expenseAcc = Guid.Parse("aaaa5555-5555-5555-5555-555555555555");
    Guid cpId = Guid.Parse("aaaa6666-6666-6666-6666-666666666666");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2025, 10, 9), CounterpartyId = cpId, CounterpartyName = "Amazon",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = cashRow, RowCounter = 1, AccountId = cashAcc, AccountType = AccountType.Cash, Credit = 1.16m },
        new TransactionRowsGetResponse { Id = blankRow, RowCounter = 2, AccountId = null, AccountType = null, Debit = 1.16m },
      },
    });
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act — fill the blank row + label it; never re-type the cash leg
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx patch {txId} -r \"{blankRow}:{expenseAcc}:1.16:0:Sale lavastoviglie\"");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Trim().Should().Be("patched.");
    CapturedRequest put = _harness.Handler.Requests[1];
    put.Method.Should().Be(HttpMethod.Put);
    JsonDocument body = JsonDocument.Parse(put.Body!);
    body.RootElement.GetProperty("date").GetString().Should().Be("2025-10-09");
    body.RootElement.GetProperty("counterpartyId").GetGuid().Should().Be(cpId);

    JsonElement rows = body.RootElement.GetProperty("transactionRows");
    rows.GetArrayLength().Should().Be(2);
    JsonElement cash = rows.EnumerateArray().Single(r => r.GetProperty("id").GetGuid() == cashRow);
    cash.GetProperty("accountId").GetGuid().Should().Be(cashAcc);
    cash.GetProperty("credit").GetDecimal().Should().Be(1.16m);
    JsonElement filled = rows.EnumerateArray().Single(r => r.GetProperty("id").GetGuid() == blankRow);
    filled.GetProperty("accountId").GetGuid().Should().Be(expenseAcc);
    filled.GetProperty("debit").GetDecimal().Should().Be(1.16m);
    filled.GetProperty("description").GetString().Should().Be("Sale lavastoviglie");
  }

  [Test]
  public async Task Patch_ShouldRefuseToChangeACashLeg_WithoutForce()
  {
    // arrange
    Guid txId = Guid.Parse("bbbb1111-1111-1111-1111-111111111111");
    Guid cashRow = Guid.Parse("bbbb2222-2222-2222-2222-222222222222");
    Guid cashAcc = Guid.Parse("bbbb4444-4444-4444-4444-444444444444");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2025, 10, 9), CounterpartyId = Guid.NewGuid(), CounterpartyName = "Amazon",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = cashRow, RowCounter = 1, AccountId = cashAcc, AccountType = AccountType.Cash, Credit = 1.16m },
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 2, AccountId = Guid.NewGuid(), AccountType = AccountType.Expense, Debit = 1.16m },
      },
    });

    // act — try to shrink the cash leg from 1.16 to 9.99
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx patch {txId} -r {cashRow}:{cashAcc}:0:9.99");

    // assert — rejected before any PUT
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("Cash (bank) leg");
    _harness.Handler.Requests.Should().ContainSingle();
    _harness.Handler.Requests[0].Method.Should().Be(HttpMethod.Get);
  }

  [Test]
  public async Task Patch_ShouldAllowChangingACashLeg_WithForce()
  {
    // arrange
    Guid txId = Guid.Parse("cccc1111-1111-1111-1111-111111111111");
    Guid cashRow = Guid.Parse("cccc2222-2222-2222-2222-222222222222");
    Guid cashAcc = Guid.Parse("cccc4444-4444-4444-4444-444444444444");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2025, 10, 9), CounterpartyId = Guid.NewGuid(), CounterpartyName = "Amazon",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = cashRow, RowCounter = 1, AccountId = cashAcc, AccountType = AccountType.Cash, Credit = 1.16m },
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 2, AccountId = Guid.NewGuid(), AccountType = AccountType.Expense, Debit = 1.16m },
      },
    });
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx patch {txId} --force -r {cashRow}:{cashAcc}:0:9.99");

    // assert
    result.ExitCode.Should().Be(0);
    JsonDocument body = JsonDocument.Parse(_harness.Handler.Requests[1].Body!);
    JsonElement cash = body.RootElement.GetProperty("transactionRows").EnumerateArray()
      .Single(r => r.GetProperty("id").GetGuid() == cashRow);
    cash.GetProperty("credit").GetDecimal().Should().Be(9.99m);
  }

  [Test]
  public async Task Patch_ShouldAppendNewRows_WhenRowIdIsEmpty()
  {
    // arrange — split out a fee into a fresh row (Mooney pattern)
    Guid txId = Guid.Parse("dddd1111-1111-1111-1111-111111111111");
    Guid cashRow = Guid.Parse("dddd2222-2222-2222-2222-222222222222");
    Guid blankRow = Guid.Parse("dddd3333-3333-3333-3333-333333333333");
    Guid cashAcc = Guid.Parse("dddd4444-4444-4444-4444-444444444444");
    Guid expenseAcc = Guid.Parse("dddd5555-5555-5555-5555-555555555555");
    Guid feeAcc = Guid.Parse("dddd6666-6666-6666-6666-666666666666");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2025, 10, 9), CounterpartyId = Guid.NewGuid(), CounterpartyName = "Mooney",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = cashRow, RowCounter = 1, AccountId = cashAcc, AccountType = AccountType.Cash, Credit = 10m },
        new TransactionRowsGetResponse { Id = blankRow, RowCounter = 2, AccountId = null, AccountType = null, Debit = 10m },
      },
    });
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act — fill the blank row with 8.50 and add a 1.50 fee row
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx patch {txId} -r {blankRow}:{expenseAcc}:8.50:0 -r :{feeAcc}:1.50:0:Commissione");

    // assert
    result.ExitCode.Should().Be(0);
    JsonElement rows = JsonDocument.Parse(_harness.Handler.Requests[1].Body!).RootElement.GetProperty("transactionRows");
    rows.GetArrayLength().Should().Be(3);
    JsonElement fee = rows.EnumerateArray().Single(r => r.GetProperty("description").GetString() == "Commissione");
    fee.GetProperty("accountId").GetGuid().Should().Be(feeAcc);
    fee.GetProperty("rowCounter").GetInt32().Should().Be(3);
    AssertNullOrAbsent(fee, "id", "a new row carries no id");
  }

  [Test]
  public async Task Patch_ShouldSendNullCounterparty_OnAnUnlinkedTransaction()
  {
    // arrange — Delticom case: set a label before a counterparty exists
    Guid txId = Guid.Parse("eeee1111-1111-1111-1111-111111111111");
    Guid cashRow = Guid.Parse("eeee2222-2222-2222-2222-222222222222");
    Guid blankRow = Guid.Parse("eeee3333-3333-3333-3333-333333333333");
    Guid cashAcc = Guid.Parse("eeee4444-4444-4444-4444-444444444444");
    Guid expenseAcc = Guid.Parse("eeee5555-5555-5555-5555-555555555555");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2025, 10, 9), CounterpartyId = null, CounterpartyName = "",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = cashRow, RowCounter = 1, AccountId = cashAcc, AccountType = AccountType.Cash, Credit = 80m },
        new TransactionRowsGetResponse { Id = blankRow, RowCounter = 2, AccountId = null, AccountType = null, Debit = 80m },
      },
    });
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx patch {txId} -r \"{blankRow}:{expenseAcc}:80:0:Pneumatici invernali\"");

    // assert
    result.ExitCode.Should().Be(0);
    JsonDocument body = JsonDocument.Parse(_harness.Handler.Requests[1].Body!);
    AssertNullOrAbsent(body.RootElement, "counterpartyId", "the transaction has no linked counterparty");
  }

  [Test]
  public async Task Patch_ShouldFail_WhenNamedRowDoesNotExist()
  {
    // arrange
    Guid txId = Guid.Parse("ffff1111-1111-1111-1111-111111111111");
    Guid accId = Guid.Parse("ffff5555-5555-5555-5555-555555555555");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2025, 10, 9), CounterpartyId = Guid.NewGuid(), CounterpartyName = "Amazon",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = Guid.NewGuid(), AccountType = AccountType.Cash, Credit = 5m },
      },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx patch {txId} -r 99999999-9999-9999-9999-999999999999:{accId}:5:0");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("no row with id");
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
  public async Task Categorize_ShouldSendDescription_WhenDescriptionFlagIsPassed()
  {
    // arrange
    Guid txId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    Guid pendingRowId = Guid.Parse("12121212-1212-1212-1212-121212121212");
    Guid accId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2024, 3, 14), CounterpartyId = Guid.NewGuid(), CounterpartyName = "Amazon",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = Guid.NewGuid(), Credit = 1.16m },
        new TransactionRowsGetResponse { Id = pendingRowId, RowCounter = 2, AccountId = null, Debit = 1.16m },
      },
    });
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"tx categorize {txId} --account {accId} --description \"Sale lavastoviglie\"");

    // assert
    result.ExitCode.Should().Be(0);
    CapturedRequest patch = _harness.Handler.Requests[1];
    JsonDocument body = JsonDocument.Parse(patch.Body!);
    body.RootElement.GetProperty("accountId").GetGuid().Should().Be(accId);
    body.RootElement.GetProperty("description").GetString().Should().Be("Sale lavastoviglie");
  }

  [Test]
  public async Task Categorize_ShouldOmitDescription_WhenFlagNotPassed()
  {
    // arrange
    Guid txId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    Guid pendingRowId = Guid.Parse("12121212-1212-1212-1212-121212121212");
    Guid accId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = txId, Date = new DateOnly(2024, 3, 14), CounterpartyId = Guid.NewGuid(), CounterpartyName = "Amazon",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = Guid.NewGuid(), Credit = 1.16m },
        new TransactionRowsGetResponse { Id = pendingRowId, RowCounter = 2, AccountId = null, Debit = 1.16m },
      },
    });
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx categorize {txId} --account {accId}");

    // assert — null description is dropped by WhenWritingNull, so the row keeps its existing text
    result.ExitCode.Should().Be(0);
    JsonDocument body = JsonDocument.Parse(_harness.Handler.Requests[1].Body!);
    AssertNullOrAbsent(body.RootElement, "description", "an omitted --description must not be serialised");
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

  // ----- duplicate -----

  [Test]
  public async Task Duplicate_ShouldClonethRowsWithNewDate_WhenNoAmountGiven()
  {
    // arrange
    Guid sourceId = Guid.Parse("a1a1a1a1-1111-1111-1111-111111111111");
    Guid cpId = Guid.Parse("a1a1a1a1-2222-2222-2222-222222222222");
    Guid walletAcc = Guid.Parse("a1a1a1a1-3333-3333-3333-333333333333");
    Guid bankAcc = Guid.Parse("a1a1a1a1-4444-4444-4444-444444444444");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = sourceId, Date = new DateOnly(2025, 10, 1), CounterpartyId = cpId, CounterpartyName = "Sparkasse",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = walletAcc, AccountType = AccountType.Cash, Debit = 30m, Description = "Prelievo contanti" },
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 2, AccountId = bankAcc, AccountType = AccountType.Cash, Credit = 30m },
      },
    });
    _harness.Handler.EnqueueJson(new TransactionsCreateResponse { Id = Guid.NewGuid() });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx duplicate {sourceId} --date 2025-11-08");

    // assert
    result.ExitCode.Should().Be(0);
    CapturedRequest post = _harness.Handler.Requests[1];
    post.Method.Should().Be(HttpMethod.Post);
    post.Uri.AbsolutePath.Should().Be("/Transactions");
    JsonDocument body = JsonDocument.Parse(post.Body!);
    body.RootElement.GetProperty("date").GetString().Should().Be("2025-11-08");
    body.RootElement.GetProperty("counterpartyId").GetGuid().Should().Be(cpId);
    JsonElement rows = body.RootElement.GetProperty("transactionRows");
    rows.GetArrayLength().Should().Be(2);
    rows[0].GetProperty("debit").GetDecimal().Should().Be(30m);
    rows[0].GetProperty("description").GetString().Should().Be("Prelievo contanti");
    rows[1].GetProperty("credit").GetDecimal().Should().Be(30m);
  }

  [Test]
  public async Task Duplicate_ShouldScaleRowsProportionally_WhenAmountGiven()
  {
    // arrange
    Guid sourceId = Guid.Parse("b1b1b1b1-1111-1111-1111-111111111111");
    Guid cpId = Guid.Parse("b1b1b1b1-2222-2222-2222-222222222222");
    Guid walletAcc = Guid.Parse("b1b1b1b1-3333-3333-3333-333333333333");
    Guid bankAcc = Guid.Parse("b1b1b1b1-4444-4444-4444-444444444444");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = sourceId, Date = new DateOnly(2025, 10, 1), CounterpartyId = cpId, CounterpartyName = "Sparkasse",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = walletAcc, AccountType = AccountType.Cash, Debit = 30m },
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 2, AccountId = bankAcc, AccountType = AccountType.Cash, Credit = 30m },
      },
    });
    _harness.Handler.EnqueueJson(new TransactionsCreateResponse { Id = Guid.NewGuid() });

    // act — scale 30 -> 50
    CliInvocationResult result = await _harness.InvokeAsync($"tx duplicate {sourceId} --date 2025-11-08 --amount 50");

    // assert
    result.ExitCode.Should().Be(0);
    JsonElement rows = JsonDocument.Parse(_harness.Handler.Requests[1].Body!).RootElement.GetProperty("transactionRows");
    rows[0].GetProperty("debit").GetDecimal().Should().Be(50m);
    rows[1].GetProperty("credit").GetDecimal().Should().Be(50m);
  }

  [Test]
  public async Task Duplicate_ShouldFail_WhenSourceHasNoCounterpartyAndNoneProvided()
  {
    // arrange
    Guid sourceId = Guid.Parse("c1c1c1c1-1111-1111-1111-111111111111");
    _harness.Handler.EnqueueJson(new TransactionsGetResponse
    {
      Id = sourceId, Date = new DateOnly(2025, 10, 1), CounterpartyId = null, CounterpartyName = "",
      TransactionRows = new[]
      {
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = Guid.NewGuid(), AccountType = AccountType.Cash, Debit = 30m },
        new TransactionRowsGetResponse { Id = Guid.NewGuid(), RowCounter = 2, AccountId = Guid.NewGuid(), AccountType = AccountType.Cash, Credit = 30m },
      },
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"tx duplicate {sourceId} --date 2025-11-08");

    // assert
    result.ExitCode.Should().NotBe(0);
    result.StdErr.Should().Contain("no counterparty");
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
    _harness.Handler.EnqueueEmpty(HttpStatusCode.NoContent);

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
