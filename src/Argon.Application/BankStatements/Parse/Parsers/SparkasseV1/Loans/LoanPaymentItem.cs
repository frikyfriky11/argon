namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Loans;

public record LoanPaymentItem(
  DateOnly AccountingDate,
  DateOnly CurrencyDate,
  string RawDescription,
  decimal Amount,
  string LoanId,
  int InstallmentNumber
) : BaseItem(AccountingDate, CurrencyDate, RawDescription, Amount)
{
  public override string? CounterpartyName => null;
}