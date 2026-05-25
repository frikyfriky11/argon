using System.CommandLine;
using Argon.Cli.Generated;
using Argon.Cli.Output;

namespace Argon.Cli.Commands;

internal static class CounterpartiesCommand
{
  public static Command Build(CliContextFactory factory)
  {
    Command counterparties = new("counterparties", "Manage counterparties");
    counterparties.AddAlias("cp");
    counterparties.AddCommand(ListCommand(factory));
    counterparties.AddCommand(GetCommand(factory));
    counterparties.AddCommand(CreateCommand(factory));
    counterparties.AddCommand(UpdateCommand(factory));
    counterparties.AddCommand(DeleteCommand(factory));
    return counterparties;
  }

  private static Command ListCommand(CliContextFactory factory)
  {
    Option<string?> name = new("--name", "Filter by name");
    Option<int?> page = new("--page", "Page number (default 1)");
    Option<int?> pageSize = new("--page-size", "Page size (default 25, -1 for all)");
    Command cmd = new("list", "List counterparties") { name, page, pageSize };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      PaginatedListOfCounterpartiesGetListResponse result = await app.Counterparties.GetListAsync(
        ctx.ParseResult.GetValueForOption(name),
        ctx.ParseResult.GetValueForOption(page),
        ctx.ParseResult.GetValueForOption(pageSize),
        ctx.GetCancellationToken());
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
    Argument<Guid> id = new("id", "Counterparty id");
    Command cmd = new("get", "Get a counterparty") { id };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CounterpartiesGetResponse result = await app.Counterparties.GetAsync(
        ctx.ParseResult.GetValueForArgument(id),
        ctx.GetCancellationToken());
      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command CreateCommand(CliContextFactory factory)
  {
    Option<string> name = new("--name", "Counterparty name") { IsRequired = true };
    Command cmd = new("create", "Create a counterparty") { name };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CounterpartiesCreateRequest request = new()
      {
        Name = ctx.ParseResult.GetValueForOption(name)!,
      };
      CounterpartiesCreateResponse result = await app.Counterparties.CreateAsync(request, ctx.GetCancellationToken());
      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command UpdateCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Counterparty id");
    Option<string> name = new("--name", "Counterparty name") { IsRequired = true };
    Command cmd = new("update", "Update a counterparty") { id, name };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CounterpartiesUpdateRequest request = new()
      {
        Name = ctx.ParseResult.GetValueForOption(name)!,
      };
      await app.Counterparties.UpdateAsync(
        ctx.ParseResult.GetValueForArgument(id),
        request,
        ctx.GetCancellationToken());
      Console.WriteLine("updated.");
    });
    return cmd;
  }

  private static Command DeleteCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Counterparty id");
    Command cmd = new("delete", "Delete a counterparty") { id };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      using FileResponse _ = await app.Counterparties.DeleteAsync(
        ctx.ParseResult.GetValueForArgument(id),
        ctx.GetCancellationToken());
      Console.WriteLine("deleted.");
    });
    return cmd;
  }
}
