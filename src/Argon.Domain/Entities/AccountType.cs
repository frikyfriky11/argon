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
  ///   Setup accounts are used only for setting up the initial transactions that populate the initial situation.
  ///   There should be only one setup account ever.
  /// </summary>
  Setup,

  /// <summary>
  ///   Debit accounts are used to track money that is owed to others such as loans, unpaid invoices, etc.
  /// </summary>
  Debit,

  /// <summary>
  ///   Credit accounts are used to track money that is owed from others such as emitted invoices, etc.
  /// </summary>
  Credit,
}
