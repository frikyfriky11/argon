namespace Argon.Application.Transactions.GetList;

/// <summary>
///   The row of a transaction get list response
/// </summary>
/// <param name="Id">The id of the transaction row</param>
/// <param name="RowCounter">The progressive number of the transaction row in the scope of the transaction</param>
/// <param name="AccountId">The id of the account</param>
/// <param name="AccountName">The name of the account</param>
/// <param name="AccountType">The type of the account</param>
/// <param name="Debit">The debit amount of the transaction row</param>
/// <param name="Credit">The credit amount of the transaction row</param>
/// <param name="Description">The description of the transaction row</param>
[PublicAPI]
public record TransactionRowsGetListResponse(
  Guid Id,
  int RowCounter,
  Guid? AccountId,
  string? AccountName,
  AccountType? AccountType,
  decimal? Debit, 
  decimal? Credit,
  string? Description
);