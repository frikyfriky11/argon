namespace Argon.Domain.Entities;

/// <summary>
///   Represents the type of account
/// </summary>
public enum AccountType
{
  /// <summary>
  ///   Cash accounts are used to track liquidity such as bank accounts, pocket money, etc.
  /// </summary>
  Cash,

  /// <summary>
  ///   Expense accounts are used to track expenses such as restaurants, groceries, etc.
  /// </summary>
  Expense,

  /// <summary>
  ///   Revenue accounts are used to track revenue streams such as salaries, occasional jobs, etc.
  /// </summary>
  Revenue,

  /// <summary>
  ///   Equity accounts hold the owner's capital — most notably the opening-balance entry
  ///   ("Capitale iniziale") that plugs the assets − liabilities residual on the books, and any
  ///   later capital injections, write-offs or net-worth reconciliation adjustments.
  /// </summary>
  Equity,

  /// <summary>
  ///   Liability accounts track payables — money owed to others such as mortgages, loans and unpaid invoices.
  /// </summary>
  Liability,

  /// <summary>
  ///   Receivable accounts track money owed by others such as emitted invoices and reimbursements due.
  /// </summary>
  Receivable,

  /// <summary>
  ///   Asset accounts hold non-liquid assets carried at cost, such as real estate or equipment.
  ///   They behave like <see cref="Cash" /> for balance purposes (a debit raises them), but are
  ///   excluded from the liquid net-worth headline — they count only towards total net worth.
  /// </summary>
  Asset,
}
