namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Sepa;

public record DirectDebitItem(
  DateOnly AccountingDate,
  DateOnly CurrencyDate,
  string RawDescription,
  decimal Amount,
  string TransactionId,
  string ContractId,
  string Owner,
  string CreditorId,
  string CreditorName,
  string Reason,
  string SwiftCode
) : BaseItem(AccountingDate, CurrencyDate, RawDescription, Amount)
{
  public override string CounterpartyName => CreditorName;
}