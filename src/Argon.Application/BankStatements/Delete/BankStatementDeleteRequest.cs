namespace Argon.Application.BankStatements.Delete;

/// <summary>
///   The request to delete a BankStatement entity
/// </summary>
/// <param name="Id">The id of the bank statement to delete</param>
[PublicAPI]
public record BankStatementDeleteRequest(Guid Id) : IRequest;
