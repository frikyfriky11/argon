namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Emoluments;

public class AziendaSanitariaEmolumentLineParser : ILineParser
{
  private readonly CultureInfo _cultureInfo = new("it-IT");

  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("EMOLUMENTI", StringComparison.InvariantCultureIgnoreCase)
           && rawDescription.Contains("suedtiroler sanitaetsbetrieb - azienda",
             StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
       EMOLUMENTI
       uri : 2025-01-21 16:38:00.980437 emolumenti ordinante : suedtiroler sanitaetsbetrieb - azienda compensi gennaio 2025 *data ordine 270125*
    */
    const string employer = "Azienda Sanitaria dell'Alto Adige";
    DateOnly paymentDate = ParsePaymentDate(rawDescription);
    DateOnly salaryPeriod = ParseSalaryPeriod(rawDescription);

    return new EmolumentItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      employer,
      paymentDate,
      salaryPeriod,
      false // extra salary is aggregated with the ordinary salary
    );
  }

  private DateOnly ParseSalaryPeriod(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split("compensi ")[1]
      .Split(" *")[0];

    return DateOnly.ParseExact(raw, "MMMM yyyy", _cultureInfo);
  }

  private DateOnly ParsePaymentDate(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split("data ordine ")[1]
      .Split("*")[0];

    return DateOnly.ParseExact(raw, "ddMMyy", _cultureInfo);
  }
}