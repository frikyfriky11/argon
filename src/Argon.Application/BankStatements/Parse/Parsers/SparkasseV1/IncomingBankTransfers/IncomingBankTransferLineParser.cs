namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.IncomingBankTransfers;

public class IncomingBankTransferLineParser : ILineParser
{
  private readonly CultureInfo _cultureInfo = new("it-IT");

  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("BONIFICO A VOSTRO FAVORE", StringComparison.InvariantCultureIgnoreCase)
           || rawDescription.Contains("BONIFICO VS. FAVORE", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
     * BONIFICO A VOSTRO FAVORE
     * vicentini miriam data regolamento: 14/01/25 cod.id.ord: it18 i081 1558 4910 0030 4031 458 banca ordinante: 08115/58491-rzsbit21314 cro: b250101341034109485849158490it note: rimborso ikea id.operazione: notprovided
     */
    /*
     * BONIFICO VS. FAVORE
     * zanirato giuseppe val. accredito: 13/01/25 cod.id.ord: it41 a060 4558 9600 0000 5350 025 cro: 28035873002 note: rimborso pizza
     */
    /*
     * BONIFICO VS. FAVORE
     * langebner walter, kammerlander karmen wertstellung: 27/12/24 id.zahler: it97 d060 4558 9600 0000 5001 391 cro: 28023069012 beschreibung: telecamera
     */
    /*
     * BONIFICO - SEPA ISTANTANEO A VS FAVORE
     * chiara zanirato & luca bellemo data regolamento: 23/05/25 cod.id.ord: lt26 3250 0593 9996 0120 banca ordinante: revolt21xxx cro: revitr25052338266547366 valuta fissa: 23/05/25 note: hotel milano id.operazione: notprovided
     */
    string senderName = ParseSenderName(rawDescription);
    DateOnly orderDate = ParseOrderDate(rawDescription);
    string senderIban = ParseSenderIban(rawDescription);
    string reason = ParseReason(rawDescription);
    bool isSameBank = ParseIsSameBank(rawDescription);
    bool isInstantPayment = ParseIsInstantPayment(rawDescription);

    return new IncomingBankTransferItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      senderName,
      orderDate,
      senderIban,
      reason,
      isSameBank,
      isInstantPayment
    );
  }

  private static bool ParseIsSameBank(string rawDescription)
  {
    return !rawDescription.Contains("banca ordinante");
  }

  private static string ParseReason(string rawDescription)
  {
    return rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split([" note: ", " beschreibung: "], StringSplitOptions.None)[1]
      .Split(" id.operazione: ")[0];
  }

  private string ParseSenderIban(string rawDescription)
  {
    return rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split([" cod.id.ord: ", " id.zahler: "], StringSplitOptions.None)[1]
      .Split([" banca ordinante: ", " cro: "], StringSplitOptions.None)[0]
      .Replace(" ", "")
      .ToUpper(_cultureInfo);
  }

  private DateOnly ParseOrderDate(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split([" data regolamento: ", " val. accredito: ", " wertstellung: "], StringSplitOptions.None)[1]
      .Split(" ")[0];

    return DateOnly.ParseExact(raw, "dd/MM/yy", _cultureInfo);
  }

  private string ParseSenderName(string rawDescription)
  {
    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split([" data regolamento: ", " val. accredito: ", " wertstellung: "], StringSplitOptions.None)[0];

    return _cultureInfo.TextInfo.ToTitleCase(raw);
  }

  private static bool ParseIsInstantPayment(string rawDescription)
  {
    return rawDescription.Contains("SEPA ISTANTANEO");
  }
}
