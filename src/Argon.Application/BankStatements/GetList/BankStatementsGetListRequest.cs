namespace Argon.Application.BankStatements.GetList;

/// <summary>
///   The request to get a list of BankStatement entities
/// </summary>
[PublicAPI]
public record BankStatementsGetListRequest : IRequest<List<BankStatementsGetListResponse>>;