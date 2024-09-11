namespace Argon.Application.Transactions.Get;

/// <summary>
///   The request to get an existing Transaction entity
/// </summary>
/// <param name="Id">The id of the transaction</param>
[PublicAPI]
public record TransactionsGetRequest(
  Guid Id
) : IRequest<TransactionsGetResponse>;