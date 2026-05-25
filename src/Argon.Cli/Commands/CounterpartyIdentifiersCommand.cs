using System.CommandLine;
using Argon.Cli.Generated;
using Argon.Cli.Output;

namespace Argon.Cli.Commands;

internal static class CounterpartyIdentifiersCommand
{
  public static Command Build(CliContextFactory factory)
  {
    Command identifiers = new("counterparty-identifiers", "Manage counterparty identifiers (IBAN, account numbers, etc.)");
    identifiers.AddAlias("cpi");
    identifiers.AddCommand(ListCommand(factory));
    identifiers.AddCommand(GetCommand(factory));
    identifiers.AddCommand(CreateCommand(factory));
    identifiers.AddCommand(UpdateCommand(factory));
    identifiers.AddCommand(DeleteCommand(factory));
    return identifiers;
  }

  private static Command ListCommand(CliContextFactory factory)
  {
    Option<Guid?> counterpartyId = new("--counterparty", "Filter by counterparty id");
    Option<string?> text = new("--text", "Filter by identifier text");
    Option<int?> page = new("--page", "Page number");
    Option<int?> pageSize = new("--page-size", "Page size (default 25, -1 for all)");
    Command cmd = new("list", "List counterparty identifiers") { counterpartyId, text, page, pageSize };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      PaginatedListOfCounterpartyIdentifiersGetListResponse result =
        await app.CounterpartyIdentifiers.GetListAsync(
          ctx.ParseResult.GetValueForOption(counterpartyId),
          ctx.ParseResult.GetValueForOption(text),
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
    Argument<Guid> id = new("id", "Identifier id");
    Command cmd = new("get", "Get a counterparty identifier") { id };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CounterpartyIdentifiersGetResponse result = await app.CounterpartyIdentifiers.GetAsync(
        ctx.ParseResult.GetValueForArgument(id),
        ctx.GetCancellationToken());
      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command CreateCommand(CliContextFactory factory)
  {
    Option<Guid> counterpartyId = new("--counterparty", "Counterparty id") { IsRequired = true };
    Option<string> text = new("--text", "Identifier text (e.g. IBAN)") { IsRequired = true };
    Command cmd = new("create", "Create a counterparty identifier") { counterpartyId, text };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CounterpartyIdentifiersCreateRequest request = new()
      {
        CounterpartyId = ctx.ParseResult.GetValueForOption(counterpartyId),
        IdentifierText = ctx.ParseResult.GetValueForOption(text)!,
      };
      CounterpartyIdentifiersCreateResponse result = await app.CounterpartyIdentifiers.CreateAsync(
        request, ctx.GetCancellationToken());
      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command UpdateCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Identifier id");
    Option<Guid> counterpartyId = new("--counterparty", "Counterparty id") { IsRequired = true };
    Option<string> text = new("--text", "Identifier text") { IsRequired = true };
    Command cmd = new("update", "Update a counterparty identifier") { id, counterpartyId, text };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CounterpartyIdentifiersUpdateRequest request = new()
      {
        CounterpartyId = ctx.ParseResult.GetValueForOption(counterpartyId),
        IdentifierText = ctx.ParseResult.GetValueForOption(text)!,
      };
      await app.CounterpartyIdentifiers.UpdateAsync(
        ctx.ParseResult.GetValueForArgument(id),
        request,
        ctx.GetCancellationToken());
      Console.WriteLine("updated.");
    });
    return cmd;
  }

  private static Command DeleteCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Identifier id");
    Command cmd = new("delete", "Delete a counterparty identifier") { id };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      using FileResponse _ = await app.CounterpartyIdentifiers.DeleteAsync(
        ctx.ParseResult.GetValueForArgument(id),
        ctx.GetCancellationToken());
      Console.WriteLine("deleted.");
    });
    return cmd;
  }
}
