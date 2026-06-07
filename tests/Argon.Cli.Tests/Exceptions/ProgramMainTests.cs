using System.Text.RegularExpressions;

namespace Argon.Cli.Tests.Exceptions;

[NonParallelizable]
public class ProgramMainTests
{
  // ----- top-level help -----

  [Test]
  public async Task TopLevelHelp_ShouldListAllCommandsAndAliases()
  {
    // act
    (int exitCode, string stdout, string _) = await RunAsync("--help");

    // assert
    exitCode.Should().Be(0);
    AssertListsEntry(stdout, "accounts");
    AssertListsEntry(stdout, "counterparties");
    AssertListsEntry(stdout, "counterparty-identifiers");
    AssertListsEntry(stdout, "transactions");
    AssertListsEntry(stdout, "login");
    AssertListsEntry(stdout, "logout");
    AssertListsEntry(stdout, "whoami");
    AssertAliasOf(stdout, "counterparties", "cp");
    AssertAliasOf(stdout, "counterparty-identifiers", "cpi");
    AssertAliasOf(stdout, "transactions", "tx");
  }

  [Test]
  public async Task TopLevelHelp_ShouldListAllGlobalOptions()
  {
    // act
    (int exitCode, string stdout, string _) = await RunAsync("--help");

    // assert
    exitCode.Should().Be(0);
    AssertListsEntry(stdout, "--base-url");
    AssertListsEntry(stdout, "--authority");
    AssertListsEntry(stdout, "--client-id");
    Regex.IsMatch(stdout, @"(?m)^\s+-o,\s*--output\b").Should().BeTrue(
      "the --output option should advertise its -o short form on the same line");
  }

  // ----- subcommand help -----

  [Test]
  public async Task AccountsHelp_ShouldListAllSixSubcommands()
  {
    // act
    (int exitCode, string stdout, string _) = await RunAsync("accounts", "--help");

    // assert
    exitCode.Should().Be(0);
    foreach (string name in new[] { "list", "get", "create", "update", "delete", "favourite" })
    {
      AssertListsEntry(stdout, name);
    }
  }

  [Test]
  public async Task TransactionsHelp_ShouldListEverySubcommandIncludingAliasOfTheTopCommand()
  {
    // act
    (int exitCode, string stdout, string _) = await RunAsync("tx", "--help");

    // assert
    exitCode.Should().Be(0);
    foreach (string name in new[]
             {
               "list", "get", "create", "update", "categorize", "set-counterparty", "history", "delete",
             })
    {
      AssertListsEntry(stdout, name);
    }
  }

  [Test]
  public async Task CounterpartyIdentifiersHelp_ShouldListResolveSubcommand()
  {
    // act
    (int exitCode, string stdout, string _) = await RunAsync("cpi", "--help");

    // assert
    exitCode.Should().Be(0);
    AssertListsEntry(stdout, "resolve");
  }

  private static void AssertListsEntry(string output, string name)
  {
    // System.CommandLine renders each command/option as a line indented by whitespace
    // with the entry name as its leading token, e.g. "  --base-url <base-url>   ...".
    // Matching the leading-token shape avoids false positives from the same string
    // appearing inside a sibling's description.
    Regex.IsMatch(output, $@"(?m)^\s+{Regex.Escape(name)}\b").Should().BeTrue(
      $"the help output should list an entry whose first token is '{name}'");
  }

  private static void AssertAliasOf(string output, string canonical, string alias)
  {
    // Aliased commands render as "  canonical, alias  <description>".
    Regex.IsMatch(output, $@"(?m)^\s+{Regex.Escape(canonical)},\s*{Regex.Escape(alias)}\b").Should().BeTrue(
      $"the help output should advertise '{alias}' as an alias of '{canonical}' on the same line");
  }

  // ----- error path -----

  [Test]
  public async Task UnknownTopLevelCommand_ShouldFailWithNonZeroExitCode()
  {
    // act
    (int exitCode, string _, string stderr) = await RunAsync("bogus-command-name");

    // assert
    exitCode.Should().NotBe(0);
    stderr.Should().NotBeEmpty();
  }

  private static async Task<(int ExitCode, string StdOut, string StdErr)> RunAsync(params string[] args)
  {
    using StringWriter stdout = new();
    using StringWriter stderr = new();
    TextWriter originalOut = Console.Out;
    TextWriter originalErr = Console.Error;
    Console.SetOut(stdout);
    Console.SetError(stderr);
    try
    {
      int exitCode = await Argon.Cli.Program.Main(args);
      return (exitCode, stdout.ToString(), stderr.ToString());
    }
    finally
    {
      Console.SetOut(originalOut);
      Console.SetError(originalErr);
    }
  }
}
