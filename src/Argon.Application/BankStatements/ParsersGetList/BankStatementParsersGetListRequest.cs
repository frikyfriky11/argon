namespace Argon.Application.BankStatements.ParsersGetList;

/// <summary>
///   The request to get a list of BankStatement parsers
/// </summary>
[PublicAPI]
public record BankStatementParsersGetListRequest : IRequest<List<BankStatementParsersGetListResponse>>;