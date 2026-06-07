namespace Argon.Application.Counterparties.AccountHistory;

/// <summary>
///   One entry in the account-frequency table for a counterparty.
/// </summary>
/// <param name="AccountId">The id of the account</param>
/// <param name="AccountName">The name of the account</param>
/// <param name="AccountType">The type of the account</param>
/// <param name="Count">How many transaction rows tagged this counterparty are posted on this account</param>
/// <param name="Total">The net amount (sum of debit minus credit) posted on this account for this counterparty</param>
/// <param name="Average">The average net amount per posting (Total divided by Count)</param>
/// <param name="LastDate">The most recent transaction date posted on this account for this counterparty</param>
/// <param name="MostCommonDescription">The most frequent non-empty row description on this account for this counterparty, if any</param>
[PublicAPI]
public record CounterpartiesAccountHistoryResponse(
  Guid AccountId,
  string AccountName,
  AccountType AccountType,
  int Count,
  decimal Total,
  decimal Average,
  DateOnly LastDate,
  string? MostCommonDescription
);
