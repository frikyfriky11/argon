namespace Argon.Cli.Tests.Infrastructure;

internal sealed record CliInvocationResult(int ExitCode, string StdOut, string StdErr);
