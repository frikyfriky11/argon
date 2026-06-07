namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.OutgoingBankTransfers;

public class OutgoingBankTransferLineParser : ILineParser
{
  private readonly CultureInfo _cultureInfo = new("it-IT");

  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("DISPOSIZIONE RIPETITIVA", StringComparison.InvariantCultureIgnoreCase)
           || rawDescription.Contains("VS DISPOSIZIONE DI BONIFICO", StringComparison.InvariantCultureIgnoreCase)
           || rawDescription.Contains("ADDEBITO BONIFICO DA HOME BANKING", StringComparison.InvariantCultureIgnoreCase)
           || rawDescription.Contains("DISPOSIZIONE VS. FAVORE", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
     * DISPOSIZIONE RIPETITIVA
     * directa sim s.p.a. c/terzi - banca sell bonifico disposto in: accentrato coor.benef.: it15 d032 6822 3000 5231 5040 240 banca destinataria: 03268/22300-selbit2bxxx data ordine: 05/02/25 data regolamento: 06/02/25 cro: 0000028053304803481160011606it codice cliente n. 88190
     */
    /*
     * VS DISPOSIZIONE DI BONIFICO
     * cedocs societa' cooperativa sociale bonifico disposto in: internet coor.benef.: it04 f060 4511 6030 0000 1304 000 data ordine: 31/01/25 data accredito: 31/01/25 cro: 28049669612 pag. fatt. 218 del 31/01/2025
     */
    /*
     * ADDEBITO BONIFICO DA HOME BANKING
     * stefano previato - lisa zanirato bonifico disposto in: internet coor.benef.: it02 t082 6958 9600 0030 0286 681 banca destinataria: 08269/58960-rzsbit21042 data ordine: 14/01/25 data regolamento: 15/01/25 cro: 0000028037048808481160011606it spostamento liquidita
     */
    /*
     * DISPOSIZIONE VS. FAVORE
     * uri : ez8a4ox3px3treyakvayno4zanxopryt home banking ordinante : sanipro rimborso sn25-010030 - conteggio delle prestazioni d el 28/03/2025 *data ordine 020425*
     */
    string recipientName = ParseRecipientName(rawDescription);
    bool isAutomated = ParseIsAutomated(rawDescription);
    DateOnly orderDate = ParseOrderDate(rawDescription);
    DateOnly paymentDate = ParsePaymentDate(rawDescription);
    string reason = ParseReason(rawDescription);
    bool isSameBank = ParseIsSameBank(rawDescription);

    return new OutgoingBankTransferItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      recipientName,
      isAutomated,
      orderDate,
      paymentDate,
      reason,
      isSameBank
    );
  }

  private static bool ParseIsSameBank(string rawDescription)
  {
    return !rawDescription.Contains("banca destinataria");
  }

  private static string ParseReason(string rawDescription)
  {
    if (rawDescription.Contains("home banking ordinante : "))
    {
      return rawDescription
        .Split(Constants.NewLines, StringSplitOptions.None)[1]
        .Split(" home banking ordinante : ")[1]
        .Split(" ", 2)[1]
        .Split(" *data")[0];
    }
    else
    {
    IEnumerable<string> raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" cro: ")[1]
      .Split(" ")
      .Skip(1);

    return string.Join(" ", raw);
    }
  }

  private DateOnly ParsePaymentDate(string rawDescription)
  {
    if (rawDescription.Contains("*data ordine "))
    {
      string raw = rawDescription
        .Split(Constants.NewLines, StringSplitOptions.None)[1]
        .Split("*data ordine ")[1]
        .Split("*")[0];

      return DateOnly.ParseExact(raw, "ddMMyy", _cultureInfo);
    }
    else
    {
      string raw = rawDescription
        .Split(Constants.NewLines, StringSplitOptions.None)[1]
        .Split([" data accredito: ", " data regolamento: "], StringSplitOptions.None)[1]
        .Split(" cro: ")[0];

      return DateOnly.ParseExact(raw, "dd/MM/yy", _cultureInfo);
    }
  }

  private DateOnly ParseOrderDate(string rawDescription)
  {
    if (rawDescription.Contains("*data ordine "))
    {
      string raw = rawDescription
        .Split(Constants.NewLines, StringSplitOptions.None)[1]
        .Split("*data ordine ")[1]
        .Split("*")[0];

      return DateOnly.ParseExact(raw, "ddMMyy", _cultureInfo);
    }
    else
    {
      string raw = rawDescription
        .Split(Constants.NewLines, StringSplitOptions.None)[1]
        .Split(" data ordine: ")[1]
        .Split([" data accredito: ", " data regolamento: "], StringSplitOptions.None)[0];

      return DateOnly.ParseExact(raw, "dd/MM/yy", _cultureInfo);
    }
  }

  private static bool ParseIsAutomated(string rawDescription)
  {
    return rawDescription.Contains("DISPOSIZIONE RIPETITIVA");
  }

  private string ParseRecipientName(string rawDescription)
  {
    string raw;

    if (rawDescription.Contains("home banking ordinante : "))
    {
      raw = rawDescription
        .Split(Constants.NewLines, StringSplitOptions.None)[1]
        .Split(" home banking ordinante : ")[1]
        .Split(" ")[0];
    }
    else
    {
      raw = rawDescription.Split(Constants.NewLines, StringSplitOptions.None)[1]
        .Split(" bonifico disposto in: ")[0];
    }

    return _cultureInfo.TextInfo.ToTitleCase(raw);
  }
}
