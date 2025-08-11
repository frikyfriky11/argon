namespace Argon.Application.Transactions.Delete;

/// <summary>
///   The request to delete an existing Transaction entity
/// </summary>
/// <param name="Id">The id of the transaction</param>
[PublicAPI]
public record TransactionsDeleteRequest(
  Guid Id
) : IRequest;