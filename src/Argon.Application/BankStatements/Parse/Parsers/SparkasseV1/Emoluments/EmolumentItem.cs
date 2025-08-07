namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Emoluments;

public record EmolumentItem(
  DateOnly AccountingDate,
  DateOnly CurrencyDate,
  string RawDescription,
  decimal Amount,
  string Employer, // dr. schaer ag
  DateOnly PaymentDate, // 2025-02-06
  DateOnly SalaryPeriod, // accredito competenze mese di gennaio 2025
  bool IsExtraSalary
) : BaseItem(AccountingDate, CurrencyDate, RawDescription, Amount)
{
  public override string CounterpartyName => Employer;
}