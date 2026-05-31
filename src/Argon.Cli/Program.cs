using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Argon.Cli.Auth;
using Argon.Cli.Commands;
using Argon.Cli.Generated;
using Argon.Cli.Output;
using Microsoft.Extensions.Configuration;

namespace Argon.Cli;

public static class Program
{
  public static async Task<int> Main(string[] args)
  {
    RootCommand root = BuildRootCommand();
    Parser parser = new CommandLineBuilder(root)
      .UseDefaults()
      .UseExceptionHandler(HandleException, errorExitCode: 1)
      .Build();
    return await parser.InvokeAsync(args);
  }

  private static void HandleException(Exception ex, InvocationContext context)
  {
    string message = ex switch
    {
      ApiException apiEx => FormatApiException(apiEx),
      _ => ex.Message,
    };
    context.Console.Error.Write($"error: {message}{Environment.NewLine}");
    // System.CommandLine v2 beta4 does not apply the UseExceptionHandler
    // errorExitCode when a custom handler runs — set it explicitly so scripts
    // wrapping the CLI see a non-zero exit on failure.
    context.ExitCode = 1;
  }

  internal static string FormatApiException(ApiException ex)
  {
    if (string.IsNullOrWhiteSpace(ex.Response))
    {
      return $"API error ({ex.StatusCode}): {ex.Message}";
    }

    try
    {
      using JsonDocument doc = JsonDocument.Parse(ex.Response);
      if (doc.RootElement.TryGetProperty("errors", out JsonElement errors) && errors.ValueKind == JsonValueKind.Object)
      {
        StringBuilder sb = new();
        sb.Append($"validation failed ({ex.StatusCode}):");
        foreach (JsonProperty field in errors.EnumerateObject())
        {
          foreach (JsonElement msg in field.Value.EnumerateArray())
          {
            sb.Append($"\n  {field.Name}: {msg.GetString()}");
          }
        }

        return sb.ToString();
      }

      if (doc.RootElement.TryGetProperty("title", out JsonElement title))
      {
        return $"API error ({ex.StatusCode}): {title.GetString()}";
      }
    }
    catch
    {
      // not JSON — fall through
    }

    return $"API error ({ex.StatusCode}): {ex.Response}";
  }

  private static RootCommand BuildRootCommand()
  {
    Option<string?> baseUrlOption = new("--base-url", "Override the API base URL");
    Option<string?> authorityOption = new("--authority", "Override the OIDC authority");
    Option<string?> clientIdOption = new("--client-id", "Override the OAuth client id");
    Option<OutputFormat> outputOption = new(
      new[] { "--output", "-o" },
      () => OutputFormat.Table,
      "Output format: table|json|csv");

    RootCommand root = new("Argon command-line client");

    // Register as global (recursive) options so they are accepted both before and
    // after the subcommand — `argon -o json tx list` and `argon tx list -o json`
    // both parse. Plain (non-global) options are only recognised at the level they
    // are declared, which is what used to make the trailing form error.
    root.AddGlobalOption(baseUrlOption);
    root.AddGlobalOption(authorityOption);
    root.AddGlobalOption(clientIdOption);
    root.AddGlobalOption(outputOption);

    CliContextFactory factory = new(baseUrlOption, authorityOption, clientIdOption, outputOption);

    foreach (Command cmd in LoginCommand.Build(factory))
    {
      root.AddCommand(cmd);
    }

    root.AddCommand(AccountsCommand.Build(factory));
    root.AddCommand(CounterpartiesCommand.Build(factory));
    root.AddCommand(CounterpartyIdentifiersCommand.Build(factory));
    root.AddCommand(TransactionsCommand.Build(factory));

    return root;
  }
}

internal class CliContextFactory
{
  public Option<string?> BaseUrlOption { get; }
  public Option<string?> AuthorityOption { get; }
  public Option<string?> ClientIdOption { get; }
  public Option<OutputFormat> OutputOption { get; }

  public CliContextFactory(
    Option<string?> baseUrl,
    Option<string?> authority,
    Option<string?> clientId,
    Option<OutputFormat> output)
  {
    BaseUrlOption = baseUrl;
    AuthorityOption = authority;
    ClientIdOption = clientId;
    OutputOption = output;
  }

  public virtual CliContext Build(InvocationContext context)
  {
    string? cliBaseUrl = context.ParseResult.GetValueForOption(BaseUrlOption);
    string? cliAuthority = context.ParseResult.GetValueForOption(AuthorityOption);
    string? cliClientId = context.ParseResult.GetValueForOption(ClientIdOption);
    OutputFormat output = context.ParseResult.GetValueForOption(OutputOption);

    IConfiguration config = BuildConfiguration();

    string baseUrl = cliBaseUrl
                     ?? Environment.GetEnvironmentVariable("ARGON_BASE_URL")
                     ?? config["BaseUrl"]
                     ?? "http://localhost:5000";

    AuthOptions auth = new()
    {
      Authority = cliAuthority
                  ?? Environment.GetEnvironmentVariable("ARGON_AUTHORITY")
                  ?? config["Auth:Authority"]
                  ?? throw new InvalidOperationException("Auth authority is not configured."),
      ClientId = cliClientId
                 ?? Environment.GetEnvironmentVariable("ARGON_CLIENT_ID")
                 ?? config["Auth:ClientId"]
                 ?? throw new InvalidOperationException("Auth client id is not configured."),
      Scope = config["Auth:Scope"] ?? "openid profile offline_access",
    };

    if (!baseUrl.EndsWith('/'))
    {
      baseUrl += "/";
    }

    TokenStore store = new();
    HttpClient flowHttpClient = new();
    DeviceCodeFlow flow = new(flowHttpClient, auth);
    AuthenticatedHttpHandler authHandler = new(store, flow)
    {
      InnerHandler = new HttpClientHandler(),
    };
    HttpClient apiClient = new(authHandler)
    {
      BaseAddress = new Uri(baseUrl),
    };

    AccountsClient accounts = new(apiClient);
    CounterpartiesClient counterparties = new(apiClient);
    return new CliContext(
      auth,
      store,
      flow,
      apiClient,
      output,
      accounts,
      counterparties,
      new CounterpartyIdentifiersClient(apiClient),
      new TransactionsClient(apiClient),
      new ReferenceResolver(accounts, counterparties));
  }

  private static IConfiguration BuildConfiguration()
  {
    string? userConfigDir = OperatingSystem.IsWindows()
      ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
      : (Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
         ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config"));

    string userConfigPath = Path.Combine(userConfigDir!, "argon-cli", "config.json");

    ConfigurationBuilder builder = new();
    builder.SetBasePath(System.AppContext.BaseDirectory);
    builder.AddJsonFile("appsettings.json", optional: true);
    builder.AddUserSecrets(typeof(Program).Assembly, optional: true);
    builder.AddJsonFile(userConfigPath, optional: true);
    builder.AddEnvironmentVariables(prefix: "ARGON_");
    return builder.Build();
  }
}

internal sealed record CliContext(
  AuthOptions Auth,
  TokenStore TokenStore,
  DeviceCodeFlow DeviceCodeFlow,
  HttpClient ApiHttpClient,
  OutputFormat Output,
  AccountsClient Accounts,
  CounterpartiesClient Counterparties,
  CounterpartyIdentifiersClient CounterpartyIdentifiers,
  TransactionsClient Transactions,
  ReferenceResolver Resolver);
