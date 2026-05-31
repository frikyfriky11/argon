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
    identifiers.AddCommand(ResolveCommand(factory));
    identifiers.AddCommand(DeleteCommand(factory));
    return identifiers;
  }

  private static Command ListCommand(CliContextFactory factory)
  {
    Option<string?> counterpartyRef = new("--counterparty", "Filter by counterparty name or id");
    Option<string?> text = new("--text", "Filter by identifier text");
    Option<int?> page = new("--page", "Page number");
    Option<int?> pageSize = new("--page-size", "Page size (default 25, -1 for all)");
    Command cmd = new("list", "List counterparty identifiers") { counterpartyRef, text, page, pageSize };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();
      string? cpRef = ctx.ParseResult.GetValueForOption(counterpartyRef);
      Guid? counterpartyId = cpRef is null ? null : await app.Resolver.ResolveCounterpartyAsync(cpRef, ct);

      PaginatedListOfCounterpartyIdentifiersGetListResponse result =
        await app.CounterpartyIdentifiers.GetListAsync(
          counterpartyId,
          ctx.ParseResult.GetValueForOption(text),
          ctx.ParseResult.GetValueForOption(page),
          ctx.ParseResult.GetValueForOption(pageSize),
          ct);
      OutputFormatter.Write(result.Items, app.Output);
      PaginationFooter.Write(
        app.Output, result.PageNumber, result.TotalPages, result.TotalCount,
        result.Items?.Count ?? 0, result.HasNextPage);
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
    Option<string> counterpartyRef = new("--counterparty", "Counterparty name or id") { IsRequired = true };
    Option<string> text = new("--text", "Identifier text (e.g. IBAN)") { IsRequired = true };
    Command cmd = new("create", "Create a counterparty identifier") { counterpartyRef, text };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();
      CounterpartyIdentifiersCreateRequest request = new()
      {
        CounterpartyId = await app.Resolver.ResolveCounterpartyAsync(ctx.ParseResult.GetValueForOption(counterpartyRef)!, ct),
        IdentifierText = ctx.ParseResult.GetValueForOption(text)!,
      };
      CounterpartyIdentifiersCreateResponse result = await app.CounterpartyIdentifiers.CreateAsync(request, ct);
      OutputFormatter.Write(result, app.Output);
    });
    return cmd;
  }

  private static Command UpdateCommand(CliContextFactory factory)
  {
    Argument<Guid> id = new("id", "Identifier id");
    Option<string> counterpartyRef = new("--counterparty", "Counterparty name or id") { IsRequired = true };
    Option<string> text = new("--text", "Identifier text") { IsRequired = true };
    Command cmd = new("update", "Update a counterparty identifier") { id, counterpartyRef, text };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();
      CounterpartyIdentifiersUpdateRequest request = new()
      {
        CounterpartyId = await app.Resolver.ResolveCounterpartyAsync(ctx.ParseResult.GetValueForOption(counterpartyRef)!, ct),
        IdentifierText = ctx.ParseResult.GetValueForOption(text)!,
      };
      await app.CounterpartyIdentifiers.UpdateAsync(
        ctx.ParseResult.GetValueForArgument(id),
        request,
        ct);
      Console.WriteLine("updated.");
    });
    return cmd;
  }

  private static Command ResolveCommand(CliContextFactory factory)
  {
    Argument<string> rawText = new("raw-text", "The raw text to resolve (e.g. a snippet of a bank statement line)");

    Command cmd = new("resolve", "Preview which counterparties the importer would match for a raw snippet") { rawText };
    cmd.SetHandler(async ctx =>
    {
      CliContext app = factory.Build(ctx);
      CancellationToken ct = ctx.GetCancellationToken();

      CounterpartyIdentifiersResolveRequest request = new()
      {
        RawText = ctx.ParseResult.GetValueForArgument(rawText),
      };

      ICollection<CounterpartyIdentifiersResolveResponse> result =
        await app.CounterpartyIdentifiers.ResolveAsync(request, ct);

      OutputFormatter.Write(result, app.Output);
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
