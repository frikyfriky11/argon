using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Net.Http;
using Argon.Cli;
using Argon.Cli.Auth;
using Argon.Cli.Commands;
using Argon.Cli.Generated;
using Argon.Cli.Output;

namespace Argon.Cli.Tests.Infrastructure;

/// <summary>
///   Drives the whole System.CommandLine pipeline against an in-memory HTTP backend so
///   tests can assert on the exact requests the CLI emits and on the text the user sees.
/// </summary>
internal sealed class CliTestHarness : IDisposable
{
  public FakeHttpMessageHandler Handler { get; }
  public HttpClient HttpClient { get; }
  public AccountsClient Accounts { get; }
  public CounterpartiesClient Counterparties { get; }
  public CounterpartyIdentifiersClient CounterpartyIdentifiers { get; }
  public TransactionsClient Transactions { get; }
  public ReferenceResolver Resolver { get; }
  public TokenStore TokenStore { get; }

  private readonly string _credentialsDirectory;

  public CliTestHarness(string baseUrl = "http://test.local/")
  {
    Handler = new FakeHttpMessageHandler();
    HttpClient = new HttpClient(Handler) { BaseAddress = new Uri(baseUrl) };
    Accounts = new AccountsClient(HttpClient);
    Counterparties = new CounterpartiesClient(HttpClient);
    CounterpartyIdentifiers = new CounterpartyIdentifiersClient(HttpClient);
    Transactions = new TransactionsClient(HttpClient);
    Resolver = new ReferenceResolver(Accounts, Counterparties);
    _credentialsDirectory = Path.Combine(Path.GetTempPath(), "argon-cli-test-" + Guid.NewGuid().ToString("N"));
    TokenStore = new TokenStore(Path.Combine(_credentialsDirectory, "credentials.json"));
  }

  public Task<CliInvocationResult> InvokeAsync(string commandLine)
  {
    string[] args = CommandLineStringSplitter.Instance.Split(commandLine).ToArray();
    return InvokeAsync(args);
  }

  public async Task<CliInvocationResult> InvokeAsync(params string[] args)
  {
    Option<string?> baseUrlOption = new("--base-url");
    Option<string?> authorityOption = new("--authority");
    Option<string?> clientIdOption = new("--client-id");
    Option<OutputFormat> outputOption = new(
      new[] { "--output", "-o" },
      () => OutputFormat.Table,
      "Output format");

    StubCliContextFactory factory = new(
      this,
      baseUrlOption, authorityOption, clientIdOption, outputOption);

    RootCommand root = new("argon test")
    {
      baseUrlOption, authorityOption, clientIdOption, outputOption,
    };

    foreach (Command cmd in LoginCommand.Build(factory))
    {
      root.AddCommand(cmd);
    }

    root.AddCommand(AccountsCommand.Build(factory));
    root.AddCommand(CounterpartiesCommand.Build(factory));
    root.AddCommand(CounterpartyIdentifiersCommand.Build(factory));
    root.AddCommand(TransactionsCommand.Build(factory));

    Parser parser = new CommandLineBuilder(root)
      .UseDefaults()
      .UseExceptionHandler((ex, ctx) =>
      {
        string message = ex is ApiException apiEx
          ? Program.FormatApiException(apiEx)
          : ex.Message;
        ctx.Console.Error.Write($"error: {message}{Environment.NewLine}");
        ctx.ExitCode = 1;
      }, errorExitCode: 1)
      .Build();

    StringWriter stdout = new();
    StringWriter stderr = new();
    TextWriter originalOut = Console.Out;
    TextWriter originalErr = Console.Error;
    Console.SetOut(stdout);
    Console.SetError(stderr);
    try
    {
      int exitCode = await parser.InvokeAsync(args);
      return new CliInvocationResult(exitCode, stdout.ToString(), stderr.ToString());
    }
    finally
    {
      Console.SetOut(originalOut);
      Console.SetError(originalErr);
    }
  }

  public void Dispose()
  {
    HttpClient.Dispose();
    Handler.Dispose();
    try
    {
      if (Directory.Exists(_credentialsDirectory))
      {
        Directory.Delete(_credentialsDirectory, recursive: true);
      }
    }
    catch
    {
      // best-effort cleanup
    }
  }
}

internal sealed class StubCliContextFactory : CliContextFactory
{
  private readonly CliTestHarness _harness;

  public StubCliContextFactory(
    CliTestHarness harness,
    Option<string?> baseUrl,
    Option<string?> authority,
    Option<string?> clientId,
    Option<OutputFormat> output)
    : base(baseUrl, authority, clientId, output)
  {
    _harness = harness;
  }

  public override CliContext Build(InvocationContext context)
  {
    AuthOptions auth = new() { Authority = "test", ClientId = "test" };
    DeviceCodeFlow flow = new(_harness.HttpClient, auth);
    OutputFormat output = context.ParseResult.GetValueForOption(OutputOption);
    return new CliContext(
      auth,
      _harness.TokenStore,
      flow,
      _harness.HttpClient,
      output,
      _harness.Accounts,
      _harness.Counterparties,
      _harness.CounterpartyIdentifiers,
      _harness.Transactions,
      _harness.Resolver);
  }
}
