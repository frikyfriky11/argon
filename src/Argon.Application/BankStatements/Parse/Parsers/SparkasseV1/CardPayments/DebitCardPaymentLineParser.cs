namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.CardPayments;

public class DebitCardPaymentLineParser : ILineParser
{
  private readonly CultureInfo _cultureInfo = new("it-IT");

  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("PAGAMENTO DEBITO VISA/MASTERCARD", StringComparison.InvariantCultureIgnoreCase)
           || rawDescription.Contains("PAGAM. POS MAESTRO CARTA", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
     * PAGAMENTO DEBITO VISA/MASTERCARD
     * del 06/02/25 in italia a terlano valuta eur paese italia c/o eurospin s15 carta n. 416363******3171
     */
    /*
     * PAGAM. POS MAESTRO CARTA 6351744
     * del 02/12/24 16:37 in italia a merano ita valuta eur paese italia c/o hotel therme meran thermenplatz 1
     */
    string circuitName = ParseCircuitName(rawDescription);
    DateOnly paymentDate = ParsePaymentDate(rawDescription);
    bool isForeignCountry = ParseIsForeignCountry(rawDescription);
    string cityName = ParseCityName(rawDescription);
    string currencyCode = ParseCurrencyCode(rawDescription);
    string countryName = ParseCountryName(rawDescription);
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
    if (rawDescription.Contains("MAESTRO"))
      return rawDescription
        .Split(Constants.NewLines, StringSplitOptions.None)[0]
        .Split(" CARTA ")[1];

    return rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" carta n. ")[1];
  }

  private string ParseRecipientName(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" c/o ")[1]
      .Split(" carta n. ")[0];

    return _cultureInfo.TextInfo.ToTitleCase(raw);
  }

  private string ParseCountryName(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" paese ")[1]
      .Split(" c/o ")[0];

    return _cultureInfo.TextInfo.ToTitleCase(raw);
  }

  private string ParseCurrencyCode(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" valuta ")[1]
      .Split(" ")[0];

    return _cultureInfo.TextInfo.ToUpper(raw);
  }

  private string ParseCityName(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" valuta ")[0]
      .Split(" a ")[1];

    return _cultureInfo.TextInfo.ToTitleCase(raw);
  }

  private static bool ParseIsForeignCountry(string rawDescription)
  {
    return rawDescription.Contains("all'estero", StringComparison.InvariantCultureIgnoreCase);
  }

  private static DateOnly ParsePaymentDate(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" ")[1];

    return DateOnly.Parse(raw);
  }

  private string ParseCircuitName(string rawDescription)
  {
    if (rawDescription.Contains("PAGAM. POS MAESTRO CARTA")) return "Maestro";

    return "Visa/Mastercard";
  }
}