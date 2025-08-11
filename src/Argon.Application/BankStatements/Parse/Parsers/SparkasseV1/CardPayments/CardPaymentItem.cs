namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.CardPayments;

public record CardPaymentItem(
  DateOnly AccountingDate,
  DateOnly CurrencyDate,
  string RawDescription,
  decimal Amount,
  string CircuitName, // VISA/MASTERCARD
  DateOnly PaymentDate, // 06/02/25
  bool IsForeignCountry, // all'estero
  string? CityName, // terlano
  string? CurrencyCode, // eur
  string? CountryName, // italia
  string RecipientName, // eurospin s15
  string CardNumber // 416363******3171
) : BaseItem(AccountingDate, CurrencyDate, RawDescription, Amount)
{
  public override string CounterpartyName => RecipientName;
}