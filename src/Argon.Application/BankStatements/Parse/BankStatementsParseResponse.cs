namespace Argon.Application.BankStatements.Parse;

/// <summary>
///   The result of the parsing of a new Bank Statement
/// </summary>
/// <param name="BankStatementId">The id of the newly created bank statement</param>
[PublicAPI]
public record BankStatementsParseResponse(
  Guid BankStatementId
);