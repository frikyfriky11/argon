namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.OutgoingBankTransfers;

public record OutgoingBankTransferItem(
  DateOnly AccountingDate,
  DateOnly CurrencyDate,
  string RawDescription,
  decimal Amount,
  string RecipientName, // directa sim s.p.a. c/terzi - banca sell
  bool IsRecurring, // accentrato
  DateOnly OrderDate, // 05/02/25
  DateOnly PaymentDate, // 06/02/25
  string Reason, // codice cliente n. 88190
  bool IsSameBank
) : BaseItem(AccountingDate, CurrencyDate, RawDescription, Amount)
{
  public override string CounterpartyName => RecipientName;
}