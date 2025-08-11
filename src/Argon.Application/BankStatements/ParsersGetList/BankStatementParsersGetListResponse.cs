namespace Argon.Application.BankStatements.ParsersGetList;

/// <summary>
///   The result of the BankStatement parsers get list
/// </summary>
/// <param name="ParserId">The globally unique id of the parser</param>
/// <param name="ParserDisplayName">The parser display name</param>
[PublicAPI]
public record BankStatementParsersGetListResponse(
  Guid ParserId,
  string ParserDisplayName
);