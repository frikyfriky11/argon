using System.Text.Json;
using Argon.Cli.Generated;
using Argon.Cli.Tests.Infrastructure;

namespace Argon.Cli.Tests.Commands;

[NonParallelizable]
public class ReconcileCommandTests
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

  private static readonly Guid AccountId = Guid.Parse("5f4f119d-1111-1111-1111-111111111111");
  private static readonly Guid OtherAccount = Guid.Parse("5f4f119d-2222-2222-2222-222222222222");

  private void EnqueueAccount(decimal totalAmount) =>
    _harness.Handler.EnqueueJson(new[]
    {
      new AccountsGetListResponse { Id = AccountId, Name = "Sparkasse famiglia", Type = AccountType.Cash, TotalAmount = totalAmount },
    });

  private static TransactionsGetListResponse ParsedTransaction(decimal rawAmount, decimal cashDebit, decimal cashCredit) =>
    new()
    {
      Id = Guid.NewGuid(),
      Date = new DateOnly(2025, 12, 1),
      CounterpartyName = "Amazon",
      RawImportData = $"{{\"Amount\":{rawAmount.ToString(System.Globalization.CultureInfo.InvariantCulture)},\"RawDescription\":\"x\"}}",
      TransactionRows = new[]
      {
        new TransactionRowsGetListResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = AccountId, Debit = cashDebit == 0 ? null : cashDebit, Credit = cashCredit == 0 ? null : cashCredit },
        new TransactionRowsGetListResponse { Id = Guid.NewGuid(), RowCounter = 2, AccountId = OtherAccount, Debit = cashCredit == 0 ? null : cashCredit, Credit = cashDebit == 0 ? null : cashDebit },
      },
    };

  private void EnqueueTransactions(params TransactionsGetListResponse[] items) =>
    _harness.Handler.EnqueueJson(new PaginatedListOfTransactionsGetListResponse
    {
      Items = items, PageNumber = 1, TotalPages = 1, TotalCount = items.Length,
    });

  [Test]
  public async Task Reconcile_ShouldReportOk_WhenBalanceMatchesAndCashLegsAgree()
  {
    // arrange — cash leg credit 1.16 => debit-credit = -1.16, matching the raw amount
    EnqueueAccount(6442.09m);
    EnqueueTransactions(ParsedTransaction(rawAmount: -1.16m, cashDebit: 0m, cashCredit: 1.16m));

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"reconcile --account {AccountId} --expected 6442.09");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("OK");
    result.StdOut.Should().Contain("cash legs match");
  }

  [Test]
  public async Task Reconcile_ShouldFlagCashLegMismatch_AndExitNonZero()
  {
    // arrange — raw amount -1.16 but the cash leg is -9.96 (a transposition)
    EnqueueAccount(6442.09m);
    EnqueueTransactions(ParsedTransaction(rawAmount: -1.16m, cashDebit: 0m, cashCredit: 9.96m));

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"reconcile --account {AccountId}");

    // assert
    result.ExitCode.Should().Be(1);
    result.StdOut.Should().Contain("mismatch");
  }

  [Test]
  public async Task Reconcile_ShouldReportBalanceMismatch_WhenLedgerDiffersFromExpected()
  {
    // arrange — cash legs are fine, but the ledger total is €7.80 below the statement
    EnqueueAccount(6434.29m);
    EnqueueTransactions(ParsedTransaction(rawAmount: -1.16m, cashDebit: 0m, cashCredit: 1.16m));

    // act
    CliInvocationResult result = await _harness.InvokeAsync(
      $"reconcile --account {AccountId} --expected 6442.09");

    // assert
    result.ExitCode.Should().Be(1);
    result.StdOut.Should().Contain("MISMATCH");
    result.StdOut.Should().Contain("-7.80");
  }

  [Test]
  public async Task Reconcile_ShouldEmitJsonReport_WithMismatches()
  {
    // arrange
    EnqueueAccount(100m);
    EnqueueTransactions(ParsedTransaction(rawAmount: -1.16m, cashDebit: 0m, cashCredit: 9.96m));

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"-o json reconcile --account {AccountId}");

    // assert
    result.ExitCode.Should().Be(1);
    JsonDocument doc = JsonDocument.Parse(result.StdOut);
    doc.RootElement.GetProperty("ok").GetBoolean().Should().BeFalse();
    doc.RootElement.GetProperty("mismatches").GetArrayLength().Should().Be(1);
  }

  [Test]
  public async Task Reconcile_ShouldIgnoreTransactionsWithoutRawImportData()
  {
    // arrange — a manual entry (no rawImportData) must not be flagged
    EnqueueAccount(50m);
    _harness.Handler.EnqueueJson(new PaginatedListOfTransactionsGetListResponse
    {
      Items = new[]
      {
        new TransactionsGetListResponse
        {
          Id = Guid.NewGuid(), Date = new DateOnly(2025, 12, 1), CounterpartyName = "Manual",
          RawImportData = null,
          TransactionRows = new[]
          {
            new TransactionRowsGetListResponse { Id = Guid.NewGuid(), RowCounter = 1, AccountId = AccountId, Debit = 50m },
            new TransactionRowsGetListResponse { Id = Guid.NewGuid(), RowCounter = 2, AccountId = OtherAccount, Credit = 50m },
          },
        },
      },
      PageNumber = 1, TotalPages = 1, TotalCount = 1,
    });

    // act
    CliInvocationResult result = await _harness.InvokeAsync($"reconcile --account {AccountId}");

    // assert
    result.ExitCode.Should().Be(0);
    result.StdOut.Should().Contain("All 0 parsed cash legs match");
  }
}
