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
    stdout.Should().ContainAll(
      "accounts",
      "counterparties",
      "counterparty-identifiers",
      "transactions",
      "login",
      "logout",
      "whoami");
    stdout.Should().ContainAll("cp", "cpi", "tx");
  }

  [Test]
  public async Task TopLevelHelp_ShouldListAllGlobalOptions()
  {
    // act
    (int exitCode, string stdout, string _) = await RunAsync("--help");

    // assert
    exitCode.Should().Be(0);
    stdout.Should().ContainAll("--base-url", "--authority", "--client-id", "--output", "-o");
  }

  // ----- subcommand help -----

  [Test]
  public async Task AccountsHelp_ShouldListAllSixSubcommands()
  {
    // act
    (int exitCode, string stdout, string _) = await RunAsync("accounts", "--help");

    // assert
    exitCode.Should().Be(0);
    stdout.Should().ContainAll("list", "get", "create", "update", "delete", "favourite");
  }

  [Test]
  public async Task TransactionsHelp_ShouldListEverySubcommandIncludingAliasOfTheTopCommand()
  {
    // act
    (int exitCode, string stdout, string _) = await RunAsync("tx", "--help");

    // assert
    exitCode.Should().Be(0);
    stdout.Should().ContainAll(
      "list", "get", "create", "update", "categorize", "set-counterparty", "history", "delete");
  }

  [Test]
  public async Task CounterpartyIdentifiersHelp_ShouldListResolveSubcommand()
  {
    // act
    (int exitCode, string stdout, string _) = await RunAsync("cpi", "--help");

    // assert
    exitCode.Should().Be(0);
    stdout.Should().Contain("resolve");
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
