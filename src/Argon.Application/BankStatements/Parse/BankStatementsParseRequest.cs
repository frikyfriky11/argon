namespace Argon.Application.BankStatements.Parse;

/// <summary>
///   The request to parse a new Bank Statement
/// </summary>
/// <param name="InputFileContents">The content of the bank statement</param>
/// <param name="InputFileName">The filename of the bank statement</param>
/// <param name="ParserId">The id of the parser to use</param>
/// <param name="ImportToAccountId">The id of the account to which this bank statement refers to</param>
[PublicAPI]
public record BankStatementsParseRequest(
  byte[] InputFileContents,
  string InputFileName,
  Guid ParserId,
  Guid ImportToAccountId
) : IRequest<BankStatementsParseResponse>;