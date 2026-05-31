using System.CommandLine;
using System.Globalization;
using Argon.Cli.Generated;
using Argon.Cli.Output;

namespace Argon.Cli.Commands;

internal static class TransactionsCommand
{
  public static Command Build(CliContextFactory factory)
  {
    Command tx = new("transactions", "Manage transactions");
    tx.AddAlias("tx");
    tx.AddCommand(ListCommand(factory));
    tx.AddCommand(GetCommand(factory));
    tx.AddCommand(CreateCommand(factory));
    tx.AddCommand(UpdateCommand(factory));
    tx.AddCommand(CategorizeCommand(factory));
    tx.AddCommand(SetCounterpartyCommand(factory));
    tx.AddCommand(HistoryCommand(factory));
    tx.AddCommand(DeleteCommand(factory));
    return tx;
  }

  private static Command ListCommand(CliContextFactory factory)
  {
    Option<string[]> accountRefs = new("--account", "Filter by account name or id (repeatable)") { AllowMultipleArgumentsPerToken = true };
    Option<string[]> counterpartyRefs = new("--counterparty", "Filter by counterparty name or id (repeatable)") { AllowMultipleArgumentsPerToken = true };
    Option<DateTimeOffset?> from = new("--from", "Date from (inclusive)");
    Option<DateTimeOffset?> to = new("--to", "Date to (inclusive)");
    Option<string?> month = new("--month", "Filter by month: yyyy-MM, 'current', or 'last'. Expands to --from/--to (cannot be combined with them).");
    Option<TransactionStatus?> status = new(
      "--status",
      parseArgument: r =>
      {
        if (TryParseStatus(r.Tokens[0].Value, out TransactionStatus parsed))
        {
          return parsed;
        }

        r.ErrorMessage = $"Unknown status '{r.Tokens[0].Value}'. Expected: pending, confirmed, duplicate.";
        return default;
      },
      description: "Filter by status: pending, confirmed, duplicate");
    Option<int?> page = new("--page", "Page number");
    Option<int?> pageSize = new("--page-size", "Page size (default 25, -1 for all)");

    Command cmd = new("list", "List transactions")
      { accountRefs, counterpartyRefs, from, to, month, status, page, pageSize };

    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();
      string[]? accountInputs = ctx.ParseResult.GetValueForOption(accountRefs);
      string[]? counterpartyInputs = ctx.ParseResult.GetValueForOption(counterpartyRefs);

      List<Guid>? accountIds = await ResolveAllAsync(accountInputs, app.Resolver.ResolveAccountAsync, ct);
      List<Guid>? counterpartyIds = await ResolveAllAsync(counterpartyInputs, app.Resolver.ResolveCounterpartyAsync, ct);

      DateTimeOffset? dateFrom = ctx.ParseResult.GetValueForOption(from);
      DateTimeOffset? dateTo = ctx.ParseResult.GetValueForOption(to);
      string? monthInput = ctx.ParseResult.GetValueForOption(month);
      if (monthInput is not null)
      {
        if (dateFrom is not null || dateTo is not null)
        {
          throw new ArgumentException("--month cannot be combined with --from/--to.");
        }

        (dateFrom, dateTo) = MonthToRange(monthInput);
      }

      PaginatedListOfTransactionsGetListResponse result = await app.Transactions.GetListAsync(
        accountIds: accountIds,
        counterpartyIds: counterpartyIds,
        dateFrom: dateFrom,
        dateTo: dateTo,
        status: ctx.ParseResult.GetValueForOption(status),
        pageNumber: ctx.ParseResult.GetValueForOption(page),
        pageSize: ctx.ParseResult.GetValueForOption(pageSize),
        cancellationToken: ct);

      OutputFormatter.Write(result.Items, app.Output);
      PaginationFooter.Write(
        app.Output, result.PageNumber, result.TotalPages, result.TotalCount,
        result.Items?.Count ?? 0, result.HasNextPage);
    });
    return cmd;
  }

  private static Command GetCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Transaction id");
    Command cmd = new("get", "Get a transaction") { id };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      TransactionsGetResponse result = await app.Transactions.GetAsync(
        ctx.ParseResult.GetValueForArgument(id),
        ctx.GetCancellationToken());

      if (app.Output == OutputFormat.Table)
      {
        Console.WriteLine($"Id            : {result.Id}");
        Console.WriteLine($"Date          : {result.Date:yyyy-MM-dd}");
        Console.WriteLine($"Counterparty  : {result.CounterpartyName} ({result.CounterpartyId})");
        Console.WriteLine();
        OutputFormatter.Write(result.TransactionRows, app.Output);
      }
      else
      {
        OutputFormatter.Write(result, app.Output);
      }
    });
    return cmd;
  }

  private static Command CreateCommand(CliContextFactory factory)
  {
    Option<DateOnly> date = new("--date", parseArgument: r =>
        DateOnly.Parse(r.Tokens[0].Value, CultureInfo.InvariantCulture),
      description: "Transaction date (yyyy-MM-dd)") { IsRequired = true };
    Option<string> counterpartyRef = new("--counterparty", "Counterparty name or id") { IsRequired = true };
    Option<string[]> rows = new(
      new[] { "--row", "-r" },
      "Row in the form <account>:<debit>:<credit>[:<description>] (account name or id). Repeatable.")
    {
      IsRequired = true,
      AllowMultipleArgumentsPerToken = false,
    };

    Command cmd = new("create", "Create a transaction") { date, counterpartyRef, rows };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();

      string[] rowValues = ctx.ParseResult.GetValueForOption(rows) ?? Array.Empty<string>();
      List<TransactionRowsCreateRequest> parsed = new();
      int counter = 1;
      foreach (string raw in rowValues)
      {
        parsed.Add(await ParseCreateRowAsync(raw, counter++, app.Resolver, ct));
      }

      TransactionsCreateRequest request = new()
      {
        Date = ctx.ParseResult.GetValueForOption(date),
        CounterpartyId = await app.Resolver.ResolveCounterpartyAsync(ctx.ParseResult.GetValueForOption(counterpartyRef)!, ct),
        TransactionRows = parsed,
      };

      TransactionsCreateResponse result = await app.Transactions.CreateAsync(request, ct);
      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command UpdateCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Transaction id");
    Option<DateOnly> date = new("--date", parseArgument: r =>
        DateOnly.Parse(r.Tokens[0].Value, CultureInfo.InvariantCulture),
      description: "Transaction date (yyyy-MM-dd)") { IsRequired = true };
    Option<string> counterpartyRef = new("--counterparty", "Counterparty name or id") { IsRequired = true };
    Option<string[]> rows = new(
      new[] { "--row", "-r" },
      "Row in the form [rowId]:<account>:<debit>:<credit>[:<description>] (account name or id). Empty rowId means new row.")
    {
      IsRequired = true,
      AllowMultipleArgumentsPerToken = false,
    };

    Command cmd = new("update", "Update a transaction") { id, date, counterpartyRef, rows };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();

      string[] rowValues = ctx.ParseResult.GetValueForOption(rows) ?? Array.Empty<string>();
      List<TransactionRowsUpdateRequest> parsed = new();
      int counter = 1;
      foreach (string raw in rowValues)
      {
        parsed.Add(await ParseUpdateRowAsync(raw, counter++, app.Resolver, ct));
      }

      TransactionsUpdateRequest request = new()
      {
        Date = ctx.ParseResult.GetValueForOption(date),
        CounterpartyId = await app.Resolver.ResolveCounterpartyAsync(ctx.ParseResult.GetValueForOption(counterpartyRef)!, ct),
        TransactionRows = parsed,
      };

      await app.Transactions.UpdateAsync(
        ctx.ParseResult.GetValueForArgument(id),
        request,
        ct);
      Console.WriteLine("updated.");
    });
    return cmd;
  }

  private static Command CategorizeCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Transaction id");
    Option<int?> row = new("--row", "Row counter (1-based). If omitted, the unique row without an assigned account is used.");
    Option<string> accountRef = new("--account", "Account name or id to assign to the row") { IsRequired = true };

    Command cmd = new("categorize", "Assign an account to a single row of a transaction") { id, row, accountRef };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();

      Guid transactionId = ctx.ParseResult.GetValueForArgument(id);
      int? rowCounter = ctx.ParseResult.GetValueForOption(row);

      TransactionsGetResponse transaction = await app.Transactions.GetAsync(transactionId, ct);
      Guid rowId = ResolveRowToCategorize(transaction, rowCounter);

      TransactionsCategorizeRowRequest request = new()
      {
        AccountId = await app.Resolver.ResolveAccountAsync(ctx.ParseResult.GetValueForOption(accountRef)!, ct),
      };

      await app.Transactions.CategorizeRowAsync(transactionId, rowId, request, ct);
      Console.WriteLine("categorized.");
    });
    return cmd;
  }

  private static Command SetCounterpartyCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Transaction id");
    Argument<string> counterpartyRef = new("counterparty", "Counterparty name or id");

    Command cmd = new("set-counterparty", "Reassign the counterparty of a transaction") { id, counterpartyRef };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();

      TransactionsSetCounterpartyRequest request = new()
      {
        CounterpartyId = await app.Resolver.ResolveCounterpartyAsync(
          ctx.ParseResult.GetValueForArgument(counterpartyRef),
          ct),
      };

      await app.Transactions.SetCounterpartyAsync(
        ctx.ParseResult.GetValueForArgument(id),
        request,
        ct);
      Console.WriteLine("counterparty updated.");
    });
    return cmd;
  }

  private static Guid ResolveRowToCategorize(TransactionsGetResponse transaction, int? rowCounter)
  {
    if (rowCounter is { } counter)
    {
      List<TransactionRowsGetResponse> byCounter = transaction.TransactionRows
        .Where(r => r.RowCounter == counter)
        .ToList();

      if (byCounter.Count == 0)
      {
        throw new ArgumentException($"No row with counter {counter} on transaction {transaction.Id}.");
      }

      if (byCounter.Count > 1)
      {
        throw new ArgumentException($"Multiple rows with counter {counter} on transaction {transaction.Id} — ambiguous.");
      }

      return byCounter[0].Id;
    }

    List<TransactionRowsGetResponse> pending = transaction.TransactionRows
      .Where(r => r.AccountId is null)
      .ToList();

    if (pending.Count == 0)
    {
      throw new ArgumentException(
        $"Transaction {transaction.Id} has no row awaiting categorization. Pass --row to pick one explicitly.");
    }

    if (pending.Count > 1)
    {
      throw new ArgumentException(
        $"Transaction {transaction.Id} has {pending.Count} rows without an account. Pass --row <counter> to disambiguate.");
    }

    return pending[0].Id;
  }

  private static Command HistoryCommand(CliContextFactory factory)
  {
    Option<string> counterpartyRef = new("--counterparty", "Counterparty name or id") { IsRequired = true };

    Command cmd = new("history", "Show the frequency table of accounts a counterparty has been posted against")
      { counterpartyRef };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();

      Guid counterpartyId = await app.Resolver.ResolveCounterpartyAsync(
        ctx.ParseResult.GetValueForOption(counterpartyRef)!,
        ct);

      ICollection<CounterpartiesAccountHistoryResponse> result =
        await app.Counterparties.AccountHistoryAsync(counterpartyId, ct);

      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command DeleteCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Transaction id");
    Command cmd = new("delete", "Delete a transaction") { id };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      using FileResponse _ = await app.Transactions.DeleteAsync(
        ctx.ParseResult.GetValueForArgument(id),
        ctx.GetCancellationToken());
      Console.WriteLine("deleted.");
    });
    return cmd;
  }

  private static async Task<TransactionRowsCreateRequest> ParseCreateRowAsync(
    string raw, int counter, ReferenceResolver resolver, CancellationToken cancellationToken)
  {
    string[] parts = raw.Split(':', 4);
    if (parts.Length < 3)
    {
      throw new ArgumentException(
        $"Invalid row '{raw}'. Expected <account>:<debit>:<credit>[:<description>].");
    }

    Guid accountId = await resolver.ResolveAccountAsync(parts[0], cancellationToken);
    decimal? debit = ParseAmount(parts[1]);
    decimal? credit = ParseAmount(parts[2]);
    string? description = parts.Length == 4 ? parts[3] : null;

    return new TransactionRowsCreateRequest
    {
      RowCounter = counter,
      AccountId = accountId,
      Debit = debit,
      Credit = credit,
      Description = description,
    };
  }

  private static async Task<TransactionRowsUpdateRequest> ParseUpdateRowAsync(
    string raw, int counter, ReferenceResolver resolver, CancellationToken cancellationToken)
  {
    string[] parts = raw.Split(':', 5);
    if (parts.Length < 4)
    {
      throw new ArgumentException(
        $"Invalid row '{raw}'. Expected [rowId]:<account>:<debit>:<credit>[:<description>].");
    }

    Guid? rowId = string.IsNullOrWhiteSpace(parts[0]) ? null : Guid.Parse(parts[0]);
    Guid accountId = await resolver.ResolveAccountAsync(parts[1], cancellationToken);
    decimal? debit = ParseAmount(parts[2]);
    decimal? credit = ParseAmount(parts[3]);
    string? description = parts.Length == 5 ? parts[4] : null;

    return new TransactionRowsUpdateRequest
    {
      Id = rowId,
      RowCounter = counter,
      AccountId = accountId,
      Debit = debit,
      Credit = credit,
      Description = description,
    };
  }

  private static async Task<List<Guid>?> ResolveAllAsync(
    string[]? inputs,
    Func<string, CancellationToken, Task<Guid>> resolve,
    CancellationToken cancellationToken)
  {
    if (inputs is not { Length: > 0 })
    {
      return null;
    }

    List<Guid> resolved = new(inputs.Length);
    foreach (string input in inputs)
    {
      resolved.Add(await resolve(input, cancellationToken));
    }

    return resolved;
  }

  /// <summary>
  ///   Expands a --month token (yyyy-MM, 'current', or 'last') into an inclusive
  ///   [first-day, last-day] range, end-of-month aware. Returned as DateTimeOffset
  ///   at midnight so it serialises to the same yyyy-MM-dd shape as --from/--to.
  /// </summary>
  internal static (DateTimeOffset From, DateTimeOffset To) MonthToRange(string raw)
  {
    string trimmed = raw.Trim().ToLowerInvariant();
    int year;
    int month;

    switch (trimmed)
    {
      case "current":
        year = DateTime.Today.Year;
        month = DateTime.Today.Month;
        break;
      case "last":
        DateTime firstOfLast = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
        year = firstOfLast.Year;
        month = firstOfLast.Month;
        break;
      default:
        string[] parts = trimmed.Split('-');
        if (parts.Length != 2
            || parts[0].Length != 4
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out year)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out month)
            || month is < 1 or > 12)
        {
          throw new ArgumentException($"Invalid --month '{raw}'. Expected yyyy-MM, 'current', or 'last'.");
        }

        break;
    }

    DateTimeOffset from = new(new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Unspecified), TimeSpan.Zero);
    return (from, from.AddMonths(1).AddDays(-1));
  }

  private static decimal? ParseAmount(string raw)
  {
    if (string.IsNullOrWhiteSpace(raw) || raw == "0")
    {
      return null;
    }

    return decimal.Parse(raw, CultureInfo.InvariantCulture);
  }

  private static bool TryParseStatus(string raw, out TransactionStatus status)
  {
    switch (raw.ToLowerInvariant())
    {
      case "pending":
      case "pendingimportreview":
      case "pending-import-review":
        status = TransactionStatus.PendingImportReview;
        return true;
      case "confirmed":
        status = TransactionStatus.Confirmed;
        return true;
      case "duplicate":
      case "potentialduplicate":
      case "potential-duplicate":
        status = TransactionStatus.PotentialDuplicate;
        return true;
      default:
        status = default;
        return false;
    }
  }
}
