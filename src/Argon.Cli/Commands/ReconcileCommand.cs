using System.CommandLine;
using System.Globalization;
using System.Text.Json;
using Argon.Cli.Generated;
using Argon.Cli.Output;

namespace Argon.Cli.Commands;

internal static class ReconcileCommand
{
  private const decimal DefaultTolerance = 0.005m;

  public static Command Build(CliContextFactory factory)
  {
    Option<string> accountRef = new("--account", "Cash account to reconcile (name or id)") { IsRequired = true };
    Option<decimal?> expected = new("--expected", parseArgument: r =>
        r.Tokens.Count == 0 ? null : decimal.Parse(r.Tokens[0].Value, CultureInfo.InvariantCulture),
      description: "The bank statement's closing balance to compare the ledger against (saldo contabile finale).");
    Option<DateTimeOffset?> from = new("--from", "Only check transactions from this date (inclusive)");
    Option<DateTimeOffset?> to = new("--to", "Only check transactions up to this date (inclusive)");
    Option<string?> month = new("--month", "Restrict to a month: yyyy-MM, 'current', or 'last'. Cannot be combined with --from/--to.");
    Option<decimal> tolerance = new("--tolerance", () => DefaultTolerance, "Amount tolerance when comparing cash legs (default 0.005).");

    Command cmd = new("reconcile", "Check that an account's ledger balance and parsed cash legs agree with the bank")
      { accountRef, expected, from, to, month, tolerance };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();

      Guid accountId = await app.Resolver.ResolveAccountAsync(ctx.ParseResult.GetValueForOption(accountRef)!, ct);

      DateTimeOffset? dateFrom = ctx.ParseResult.GetValueForOption(from);
      DateTimeOffset? dateTo = ctx.ParseResult.GetValueForOption(to);
      string? monthInput = ctx.ParseResult.GetValueForOption(month);
      if (monthInput is not null)
      {
        if (dateFrom is not null || dateTo is not null)
        {
          throw new ArgumentException("--month cannot be combined with --from/--to.");
        }

        (dateFrom, dateTo) = TransactionsCommand.MonthToRange(monthInput);
      }

      decimal toleranceValue = ctx.ParseResult.GetValueForOption(tolerance);
      decimal? expectedValue = ctx.ParseResult.GetValueForOption(expected);

      ICollection<AccountsGetListResponse> accounts = await app.Accounts.GetListAsync(null, dateTo, ct);
      AccountsGetListResponse? account = accounts.FirstOrDefault(a => a.Id == accountId);
      if (account is null)
      {
        throw new ArgumentException($"Account {accountId} not found.");
      }

      PaginatedListOfTransactionsGetListResponse transactions = await app.Transactions.GetListAsync(
        accountIds: new[] { accountId },
        counterpartyIds: null,
        dateFrom: dateFrom,
        dateTo: dateTo,
        status: null,
        linked: null,
        rowAmount: null,
        rowAmountTolerance: null,
        pageNumber: null,
        pageSize: -1,
        cancellationToken: ct);

      int checkedCount = 0;
      List<CashLegMismatch> mismatches = new();
      foreach (TransactionsGetListResponse transaction in transactions.Items)
      {
        if (!TryReadRawAmount(transaction.RawImportData, out decimal rawAmount))
        {
          continue;
        }

        checkedCount++;
        decimal cashLeg = transaction.TransactionRows
          .Where(row => row.AccountId == accountId)
          .Sum(row => (row.Debit ?? 0m) - (row.Credit ?? 0m));

        decimal delta = cashLeg - rawAmount;
        if (Math.Abs(delta) > toleranceValue)
        {
          mismatches.Add(new CashLegMismatch(
            transaction.Id,
            transaction.Date,
            string.IsNullOrEmpty(transaction.CounterpartyName) ? "(none)" : transaction.CounterpartyName,
            rawAmount,
            cashLeg,
            delta));
        }
      }

      decimal? difference = expectedValue is { } exp ? account.TotalAmount - exp : null;
      bool balanceOk = difference is null || Math.Abs(difference.Value) <= toleranceValue;
      bool ok = balanceOk && mismatches.Count == 0;

      if (app.Output == OutputFormat.Table)
      {
        WriteTableReport(account, expectedValue, difference, mismatches, checkedCount, balanceOk);
      }
      else
      {
        OutputFormatter.Write(
          new ReconcileReport(
            account.Id, account.Name, account.TotalAmount, expectedValue, difference, ok, checkedCount, mismatches),
          app.Output);
      }

      ctx.ExitCode = ok ? 0 : 1;
    });
    return cmd;
  }

  private static void WriteTableReport(
    AccountsGetListResponse account,
    decimal? expected,
    decimal? difference,
    List<CashLegMismatch> mismatches,
    int checkedCount,
    bool balanceOk)
  {
    Console.WriteLine($"Account : {account.Name}");
    Console.WriteLine($"Ledger  : {Money(account.TotalAmount)}");
    if (expected is { } exp)
    {
      Console.WriteLine($"Expected: {Money(exp)}");
      Console.WriteLine($"Delta   : {Money(difference!.Value)} ({(balanceOk ? "OK" : "MISMATCH")})");
    }

    Console.WriteLine();
    if (mismatches.Count == 0)
    {
      Console.WriteLine($"All {checkedCount} parsed cash legs match their bank amount.");
      return;
    }

    Console.WriteLine($"{mismatches.Count} cash-leg mismatch(es) out of {checkedCount} parsed transactions:");
    Console.WriteLine();
    OutputFormatter.Write(mismatches, OutputFormat.Table);
  }

  private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

  /// <summary>
  ///   Reads the bank amount out of a transaction's raw import data JSON. The parser
  ///   serialises the amount under an "Amount" property; it is matched case-insensitively
  ///   and accepts either a JSON number or a numeric string.
  /// </summary>
  private static bool TryReadRawAmount(string? rawImportData, out decimal amount)
  {
    amount = 0m;
    if (string.IsNullOrWhiteSpace(rawImportData))
    {
      return false;
    }

    try
    {
      using JsonDocument document = JsonDocument.Parse(rawImportData);
      if (document.RootElement.ValueKind != JsonValueKind.Object)
      {
        return false;
      }

      foreach (JsonProperty property in document.RootElement.EnumerateObject())
      {
        if (!string.Equals(property.Name, "amount", StringComparison.OrdinalIgnoreCase))
        {
          continue;
        }

        return property.Value.ValueKind switch
        {
          JsonValueKind.Number => property.Value.TryGetDecimal(out amount),
          JsonValueKind.String => decimal.TryParse(
            property.Value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out amount),
          _ => false,
        };
      }
    }
    catch (JsonException)
    {
      return false;
    }

    return false;
  }

  private sealed record CashLegMismatch(
    Guid Id,
    DateOnly Date,
    string Counterparty,
    decimal RawAmount,
    decimal CashLeg,
    decimal Delta);

  private sealed record ReconcileReport(
    Guid AccountId,
    string AccountName,
    decimal LedgerBalance,
    decimal? Expected,
    decimal? Difference,
    bool Ok,
    int CheckedCount,
    List<CashLegMismatch> Mismatches);
}
