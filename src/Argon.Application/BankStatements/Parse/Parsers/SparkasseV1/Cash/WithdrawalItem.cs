namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Cash;

public record WithdrawalItem(
  DateOnly AccountingDate,
  DateOnly CurrencyDate,
  string RawDescription,
  decimal Amount
) : BaseItem(AccountingDate, CurrencyDate, RawDescription, Amount)
{
  public override string? CounterpartyName => null;
}