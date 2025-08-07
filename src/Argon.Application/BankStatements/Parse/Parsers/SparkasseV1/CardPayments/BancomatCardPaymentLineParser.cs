namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.CardPayments;

public class BancomatCardPaymentLineParser : ILineParser
{
  private readonly CultureInfo _cultureInfo = new("it-IT");

  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("PAGAMENTO POS CARTA", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
       PAGAMENTO POS CARTA 6351753
       eseguito il 30/12/24 c/o 41298 bolzano - resia
    */
    const string circuitName = "Bancomat";
    DateOnly paymentDate = ParsePaymentDate(rawDescription);
    const bool isForeignCountry = false;
    string? cityName = null;
    string? currencyCode = null;
    string? countryName = null;
    string recipientName = ParseRecipientName(rawDescription);
    string cardNumber = ParseCardNumber(rawDescription);

    return new CardPaymentItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      circuitName,
      paymentDate,
      isForeignCountry,
      cityName,
      currencyCode,
      countryName,
      recipientName,
      cardNumber
    );
  }

  private static string ParseCardNumber(string rawDescription)
  {
    return rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[0]
      .Split("PAGAMENTO POS CARTA ")[1];
  }

  private string ParseRecipientName(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" c/o ")[1];

    return _cultureInfo.TextInfo.ToTitleCase(raw);
  }

  private DateOnly ParsePaymentDate(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split("eseguito il ")[1]
      .Split(" ")[0];

    return DateOnly.ParseExact(raw, "dd/MM/yy", _cultureInfo);
  }
}