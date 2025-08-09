namespace Argon.Application.Transactions.Get;

/// <summary>
///   The row of a transaction get response
/// </summary>
/// <param name="Id">The id of the transaction row</param>
/// <param name="RowCounter">The progressive number of the transaction row in the scope of the transaction</param>
/// <param name="AccountId">The id of the account</param>
/// <param name="Debit">The debit amount of the transaction row</param>
/// <param name="Credit">The credit amount of the transaction row</param>
/// <param name="Description">The description of the transaction row</param>
[PublicAPI]
public record TransactionRowsGetResponse(
  Guid Id,
  int RowCounter,
  Guid? AccountId,
  decimal? Debit,
  decimal? Credit,
  string? Description
);