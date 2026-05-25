using System.CommandLine;
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
    accounts.AddCommand(CreateCommand(factory));
    accounts.AddCommand(UpdateCommand(factory));
    accounts.AddCommand(DeleteCommand(factory));
    accounts.AddCommand(FavouriteCommand(factory));
    return accounts;
  }

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
