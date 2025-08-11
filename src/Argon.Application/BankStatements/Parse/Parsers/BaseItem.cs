namespace Argon.Application.BankStatements.Parse.Parsers;

/// <summary>
///   Represents a parsed bank statement record.
///   Specialized parsed records should inherit from this type.
///   This type cannot be instantiated by itself.
/// </summary>
/// <param name="AccountingDate">The accounting date of the item</param>
/// <param name="CurrencyDate">The currency date of the item</param>
/// <param name="RawDescription">The raw item description</param>
/// <param name="Amount">The amount of the item</param>
public abstract record BaseItem(DateOnly AccountingDate, DateOnly CurrencyDate, string RawDescription, decimal Amount)
{
  /// <summary>
  ///   A string that represents the counterparty of the item
  /// </summary>
  public abstract string? CounterpartyName { get; }
}