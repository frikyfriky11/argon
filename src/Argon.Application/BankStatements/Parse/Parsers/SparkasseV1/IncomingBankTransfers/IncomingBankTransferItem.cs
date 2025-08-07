namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.IncomingBankTransfers;

public record IncomingBankTransferItem(
  DateOnly AccountingDate,
  DateOnly CurrencyDate,
  string RawDescription,
  decimal Amount,
  string SenderName,
  DateOnly OrderDate,
  string SenderIban,
  string Reason,
  bool IsSameBank
) : BaseItem(AccountingDate, CurrencyDate, RawDescription, Amount)
{
  public override string CounterpartyName => SenderName;
}