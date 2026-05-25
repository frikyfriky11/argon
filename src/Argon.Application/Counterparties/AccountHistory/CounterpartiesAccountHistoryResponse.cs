namespace Argon.Application.Counterparties.AccountHistory;

/// <summary>
///   One entry in the account-frequency table for a counterparty.
/// </summary>
/// <param name="AccountId">The id of the account</param>
/// <param name="AccountName">The name of the account</param>
/// <param name="AccountType">The type of the account</param>
/// <param name="Count">How many transaction rows tagged this counterparty are posted on this account</param>
[PublicAPI]
public record CounterpartiesAccountHistoryResponse(
  Guid AccountId,
  string AccountName,
  AccountType AccountType,
  int Count
);
