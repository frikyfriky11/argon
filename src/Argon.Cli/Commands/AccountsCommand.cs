using System.CommandLine;
using System.Globalization;
using Argon.Cli.Generated;
using Argon.Cli.Output;

namespace Argon.Cli.Commands;

internal static class AccountsCommand
{
  public static Command Build(CliContextFactory factory)
  {
    Command accounts = new("accounts", "Manage accounts");
    accounts.AddCommand(ListCommand(factory));
    accounts.AddCommand(GetCommand(factory));
    accounts.AddCommand(BalanceCommand(factory));
    accounts.AddCommand(CreateCommand(factory));
    accounts.AddCommand(UpdateCommand(factory));
    accounts.AddCommand(DeleteCommand(factory));
    accounts.AddCommand(FavouriteCommand(factory));
    return accounts;
  }

  private static Command BalanceCommand(CliContextFactory factory)
  {
    Argument<string> nameOrId = new("account", "Account name or id");
    Option<DateOnly?> asOf = new("--as-of", parseArgument: r =>
        r.Tokens.Count == 0 ? null : DateOnly.Parse(r.Tokens[0].Value, CultureInfo.InvariantCulture),
      description: "Balance and contributions as of this date (yyyy-MM-dd). Defaults to all-time.");

    Command cmd = new("balance", "Show an account's running balance and the transactions that make it up")
      { nameOrId, asOf };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();

      Guid accountId = await app.Resolver.ResolveAccountAsync(ctx.ParseResult.GetValueForArgument(nameOrId), ct);
      DateOnly? asOfDate = ctx.ParseResult.GetValueForOption(asOf);
      DateTimeOffset? to = asOfDate is { } d
        ? new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
        : null;

      ICollection<AccountsGetListResponse> accounts = await app.Accounts.GetListAsync(null, to, ct);
      AccountsGetListResponse? account = accounts.FirstOrDefault(a => a.Id == accountId);
      if (account is null)
      {
        throw new ArgumentException($"Account {accountId} not found.");
      }

      PaginatedListOfTransactionsGetListResponse transactions = await app.Transactions.GetListAsync(
        accountIds: new[] { accountId },
        counterpartyIds: null,
        dateFrom: null,
        dateTo: to,
        status: null,
        linked: null,
        rowAmount: null,
        rowAmountTolerance: null,
        pageNumber: null,
        pageSize: -1,
        cancellationToken: ct);

      List<BalanceContribution> contributions = transactions.Items
        .SelectMany(t => t.TransactionRows
          .Where(row => row.AccountId == accountId)
          .Select(row => new BalanceContribution(
            t.Date,
            string.IsNullOrEmpty(t.CounterpartyName) ? "(none)" : t.CounterpartyName,
            row.Debit,
            row.Credit,
            row.Description)))
        .OrderBy(c => c.Date)
        .ToList();

      if (app.Output == OutputFormat.Table)
      {
        Console.WriteLine($"Account : {account.Name} ({account.Type})");
        string balance = account.TotalAmount.ToString("0.00", CultureInfo.InvariantCulture);
        Console.WriteLine(asOfDate is { } day
          ? $"Balance : {balance} (as of {day:yyyy-MM-dd})"
          : $"Balance : {balance}");
        Console.WriteLine();
        OutputFormatter.Write(contributions, app.Output);
      }
      else
      {
        OutputFormatter.Write(
          new AccountBalanceView(account.Id, account.Name, account.Type, account.TotalAmount, asOfDate, contributions),
          app.Output);
      }
    });
    return cmd;
  }

  private sealed record BalanceContribution(
    DateOnly Date,
    string Counterparty,
    decimal? Debit,
    decimal? Credit,
    string? Description);

  private sealed record AccountBalanceView(
    Guid AccountId,
    string Name,
    AccountType Type,
    decimal Balance,
    DateOnly? AsOf,
    List<BalanceContribution> Contributions);

  private static Command ListCommand(CliContextFactory factory)
  {
    Option<DateTimeOffset?> from = new("--from", "Compute total amounts from this date");
    Option<DateTimeOffset?> to = new("--to", "Compute total amounts up to this date");
    Command cmd = new("list", "List accounts") { from, to };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      ICollection<AccountsGetListResponse> result = await app.Accounts.GetListAsync(
        ctx.ParseResult.GetValueForOption(from),
        ctx.ParseResult.GetValueForOption(to),
        ctx.GetCancellationToken());
      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command GetCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Account id");
    Command cmd = new("get", "Get an account") { id };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      AccountsGetResponse result = await app.Accounts.GetAsync(
        ctx.ParseResult.GetValueForArgument(id),
        ctx.GetCancellationToken());
      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command CreateCommand(CliContextFactory factory)
  {
    Option<string> name = new("--name", "Account name") { IsRequired = true };
    Option<AccountType> type = new("--type", "Account type") { IsRequired = true };
    Command cmd = new("create", "Create an account") { name, type };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      AccountsCreateRequest request = new()
      {
        Name = ctx.ParseResult.GetValueForOption(name)!,
        Type = ctx.ParseResult.GetValueForOption(type),
      };
      AccountsCreateResponse result = await app.Accounts.CreateAsync(request, ctx.GetCancellationToken());
      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command UpdateCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Account id");
    Option<string> name = new("--name", "Account name") { IsRequired = true };
    Option<AccountType> type = new("--type", "Account type") { IsRequired = true };
    Command cmd = new("update", "Update an account") { id, name, type };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      AccountsUpdateRequest request = new()
      {
        Name = ctx.ParseResult.GetValueForOption(name)!,
        Type = ctx.ParseResult.GetValueForOption(type),
      };
      await app.Accounts.UpdateAsync(
        ctx.ParseResult.GetValueForArgument(id),
        request,
        ctx.GetCancellationToken());
      Console.WriteLine("updated.");
    });
    return cmd;
  }

  private static Command DeleteCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Account id");
    Command cmd = new("delete", "Delete an account") { id };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      using FileResponse _ = await app.Accounts.DeleteAsync(
        ctx.ParseResult.GetValueForArgument(id),
        ctx.GetCancellationToken());
      Console.WriteLine("deleted.");
    });
    return cmd;
  }

  private static Command FavouriteCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Account id");
    Option<bool> isFavourite = new(
      new[] { "--is-favourite", "--favourite" },
      () => true,
      "Set favourite flag (default: true)");
    Command cmd = new("favourite", "Toggle the favourite flag on an account") { id, isFavourite };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      AccountsFavouriteRequest request = new()
      {
        IsFavourite = ctx.ParseResult.GetValueForOption(isFavourite),
      };
      await app.Accounts.FavouriteAsync(
        ctx.ParseResult.GetValueForArgument(id),
        request,
        ctx.GetCancellationToken());
      Console.WriteLine("updated.");
    });
    return cmd;
  }
}
