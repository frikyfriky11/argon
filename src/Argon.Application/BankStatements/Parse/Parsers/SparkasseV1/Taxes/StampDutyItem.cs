namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Taxes;

public record StampDutyItem(
  DateOnly AccountingDate,
  DateOnly CurrencyDate,
  string RawDescription,
  decimal Amount,
  DateOnly PeriodStart,
  DateOnly PeriodEnd
) : BaseItem(AccountingDate, CurrencyDate, RawDescription, Amount)
{
  public override string? CounterpartyName => null;
}