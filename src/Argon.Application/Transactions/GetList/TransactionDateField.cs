namespace Argon.Application.Transactions.GetList;

/// <summary>
///   Selects which date field the transactions list filters (and is interpreted) on.
/// </summary>
[PublicAPI]
public enum TransactionDateField
{
  /// <summary>
  ///   The transaction <see cref="Argon.Domain.Entities.Transaction.Date" /> — the currency
  ///   (value) date for parsed bank lines. This is the historical default.
  /// </summary>
  Date = 0,

  /// <summary>
  ///   The accounting (booking) date — when the line hit the bank statement. Falls back to
  ///   <see cref="Argon.Domain.Entities.Transaction.Date" /> for transactions that have no
  ///   accounting date (manual entries).
  /// </summary>
  AccountingDate = 1,
}
