namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Emoluments;

public class DrScharEmolumentLineParser : ILineParser
{
  private readonly CultureInfo _cultureInfo = new("it-IT");

  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("EMOLUMENTI", StringComparison.InvariantCultureIgnoreCase)
           && rawDescription.Contains("dr. schaer ag", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
       EMOLUMENTI
       dr. schaer ag data regolamento: 06/02/25 cod.id.ord: it18 z081 3358 5910 0030 1002 538 banca ordinante: 08133/58591-rzsbit21119 cro: b250203643722708275859158710it note: accredito competenze mese di gennaio 2025 id.operazione: notprovided
    */
    const string employer = "Dr. Schär AG";
    DateOnly paymentDate = ParsePaymentDate(rawDescription);
    (DateOnly salaryPeriod, bool isExtraSalary) = ParseSalaryPeriod(rawDescription);

    return new EmolumentItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      employer,
      paymentDate,
      salaryPeriod,
      isExtraSalary
    );
  }

  private (DateOnly salaryPeriod, bool isExtraSalary) ParseSalaryPeriod(string rawDescription)
  {
    if (rawDescription.Contains("mens. agg."))
    {
      string raw = rawDescription
        .Split(Constants.NewLines, StringSplitOptions.None)[1]
        .Split("note: ")[1]
        .Split(" id.operazione: ")[0]
        .Split(" mens. agg. di ")[1];

      return (DateOnly.ParseExact(raw, "MMMM yyyy", _cultureInfo), true);
    }
    else
    {
      string raw = rawDescription
        .Split(Constants.NewLines, StringSplitOptions.None)[1]
        .Split("note: ")[1]
        .Split(" id.operazione: ")[0]
        .Split(" mese di ")[1];

      return (DateOnly.ParseExact(raw, "MMMM yyyy", _cultureInfo), false);
    }
  }

  private static DateOnly ParsePaymentDate(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split("data regolamento: ")[1]
      .Split(" ")[0];

    return DateOnly.Parse(raw);
  }
}