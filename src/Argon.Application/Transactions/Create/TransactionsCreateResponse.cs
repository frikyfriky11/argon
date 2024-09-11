namespace Argon.Application.Transactions.Create;

/// <summary>
///   The result of the creation of a new Transaction entity
/// </summary>
/// <param name="Id">The id of the newly created Transaction</param>
[PublicAPI]
public record TransactionsCreateResponse(
  Guid Id
);