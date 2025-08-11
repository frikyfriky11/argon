namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Taxes;

public record BankFeeItem(
  DateOnly AccountingDate,
  DateOnly CurrencyDate,
  string RawDescription,
  decimal Amount
) : BaseItem(AccountingDate, CurrencyDate, RawDescription, Amount)
{
  public override string? CounterpartyName => null;
}