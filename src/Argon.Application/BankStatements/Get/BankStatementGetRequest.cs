namespace Argon.Application.BankStatements.Get;

/// <summary>
///   The request to get a BankStatement entity
/// </summary>
/// <param name="Id">The id of the bank statement to get</param>
[PublicAPI]
public record BankStatementGetRequest(Guid Id) : IRequest<BankStatementGetResponse>;
