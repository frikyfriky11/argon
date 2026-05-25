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
    tx.AddCommand(DeleteCommand(factory));
    return tx;
  }

  private static Command ListCommand(CliContextFactory factory)
  {
    Option<Guid[]> accountIds = new("--account", "Filter by account id (repeatable)") { AllowMultipleArgumentsPerToken = true };
    Option<Guid[]> counterpartyIds = new("--counterparty", "Filter by counterparty id (repeatable)") { AllowMultipleArgumentsPerToken = true };
    Option<DateTimeOffset?> from = new("--from", "Date from (inclusive)");
    Option<DateTimeOffset?> to = new("--to", "Date to (inclusive)");
    Option<TransactionStatus?> status = new(
      "--status",
      parseArgument: r => ParseStatus(r.Tokens[0].Value),
      description: "Filter by status: pending, confirmed, duplicate");
    Option<int?> page = new("--page", "Page number");
    Option<int?> pageSize = new("--page-size", "Page size (default 25, -1 for all)");

    Command cmd = new("list", "List transactions")
      { accountIds, counterpartyIds, from, to, status, page, pageSize };

    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      Guid[]? accounts = ctx.ParseResult.GetValueForOption(accountIds);
      Guid[]? counterparties = ctx.ParseResult.GetValueForOption(counterpartyIds);

      PaginatedListOfTransactionsGetListResponse result = await app.Transactions.GetListAsync(
        accountIds: accounts is { Length: > 0 } ? accounts : null,
        counterpartyIds: counterparties is { Length: > 0 } ? counterparties : null,
        dateFrom: ctx.ParseResult.GetValueForOption(from),
        dateTo: ctx.ParseResult.GetValueForOption(to),
        status: ctx.ParseResult.GetValueForOption(status),
        pageNumber: ctx.ParseResult.GetValueForOption(page),
        pageSize: ctx.ParseResult.GetValueForOption(pageSize),
        cancellationToken: ctx.GetCancellationToken());

      OutputFormatter.Write(result.Items, app.Output);
      if (app.Output == OutputFormat.Table)
      {
        Console.WriteLine();
        Console.WriteLine($"page {result.PageNumber}/{result.TotalPages}  ({result.TotalCount} total)");
      }
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
    Option<Guid> counterpartyId = new("--counterparty", "Counterparty id") { IsRequired = true };
    Option<string[]> rows = new(
      new[] { "--row", "-r" },
      "Row in the form <accountId>:<debit>:<credit>[:<description>]. Repeatable.")
    {
      IsRequired = true,
      AllowMultipleArgumentsPerToken = false,
    };

    Command cmd = new("create", "Create a transaction") { date, counterpartyId, rows };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);

      string[] rowValues = ctx.ParseResult.GetValueForOption(rows) ?? Array.Empty<string>();
      List<TransactionRowsCreateRequest> parsed = new();
      int counter = 1;
      foreach (string raw in rowValues)
      {
        parsed.Add(ParseCreateRow(raw, counter++));
      }

      TransactionsCreateRequest request = new()
      {
        Date = ctx.ParseResult.GetValueForOption(date),
        CounterpartyId = ctx.ParseResult.GetValueForOption(counterpartyId),
        TransactionRows = parsed,
      };

      TransactionsCreateResponse result = await app.Transactions.CreateAsync(request, ctx.GetCancellationToken());
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
    Option<Guid> counterpartyId = new("--counterparty", "Counterparty id") { IsRequired = true };
    Option<string[]> rows = new(
      new[] { "--row", "-r" },
      "Row in the form [rowId]:<accountId>:<debit>:<credit>[:<description>]. Empty rowId means new row.")
    {
      IsRequired = true,
      AllowMultipleArgumentsPerToken = false,
    };

    Command cmd = new("update", "Update a transaction") { id, date, counterpartyId, rows };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);

      string[] rowValues = ctx.ParseResult.GetValueForOption(rows) ?? Array.Empty<string>();
      List<TransactionRowsUpdateRequest> parsed = new();
      int counter = 1;
      foreach (string raw in rowValues)
      {
        parsed.Add(ParseUpdateRow(raw, counter++));
      }

      TransactionsUpdateRequest request = new()
      {
        Date = ctx.ParseResult.GetValueForOption(date),
        CounterpartyId = ctx.ParseResult.GetValueForOption(counterpartyId),
        TransactionRows = parsed,
      };

      await app.Transactions.UpdateAsync(
        ctx.ParseResult.GetValueForArgument(id),
        request,
        ctx.GetCancellationToken());
      Console.WriteLine("updated.");
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

  private static TransactionRowsCreateRequest ParseCreateRow(string raw, int counter)
  {
    string[] parts = raw.Split(':', 4);
    if (parts.Length < 3)
    {
      throw new ArgumentException(
        $"Invalid row '{raw}'. Expected <accountId>:<debit>:<credit>[:<description>].");
    }

    Guid accountId = Guid.Parse(parts[0]);
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

  private static TransactionRowsUpdateRequest ParseUpdateRow(string raw, int counter)
  {
    string[] parts = raw.Split(':', 5);
    if (parts.Length < 4)
    {
      throw new ArgumentException(
        $"Invalid row '{raw}'. Expected [rowId]:<accountId>:<debit>:<credit>[:<description>].");
    }

    Guid? rowId = string.IsNullOrWhiteSpace(parts[0]) ? null : Guid.Parse(parts[0]);
    Guid accountId = Guid.Parse(parts[1]);
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

  private static decimal? ParseAmount(string raw)
  {
    if (string.IsNullOrWhiteSpace(raw) || raw == "0")
    {
      return null;
    }

    return decimal.Parse(raw, CultureInfo.InvariantCulture);
  }

  private static TransactionStatus ParseStatus(string raw) => raw.ToLowerInvariant() switch
  {
    "pending" or "pendingimportreview" or "pending-import-review" => TransactionStatus.PendingImportReview,
    "confirmed" => TransactionStatus.Confirmed,
    "duplicate" or "potentialduplicate" or "potential-duplicate" => TransactionStatus.PotentialDuplicate,
    _ => throw new ArgumentException($"Unknown status '{raw}'. Expected: pending, confirmed, duplicate."),
  };
}
