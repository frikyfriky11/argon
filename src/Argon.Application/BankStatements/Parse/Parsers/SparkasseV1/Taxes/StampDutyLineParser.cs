namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Taxes;

public class StampDutyLineParser : ILineParser
{
  private readonly CultureInfo _cultureInfo = new("it-IT");

  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("IMPOSTA DI BOLLO SU RENDICONTO", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
     * IMPOSTA DI BOLLO SU RENDICONTO
     * recupero periodo dal 01/01/25 al 31/01/25
     */
    DateOnly periodStart = ParsePeriodStart(rawDescription);
    DateOnly periodEnd = ParsePeriodEnd(rawDescription);

    return new StampDutyItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      periodStart,
      periodEnd
    );
  }

  private DateOnly ParsePeriodEnd(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" al ")[1];

    return DateOnly.ParseExact(raw, "dd/MM/yy", _cultureInfo);
  }

  private DateOnly ParsePeriodStart(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" dal ")[1]
      .Split(" al ")[0];

    return DateOnly.ParseExact(raw, "dd/MM/yy", _cultureInfo);
  }
}