namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Sepa;

public class DirectDebitLineParser : ILineParser
{
  private readonly CultureInfo _cultureInfo = new("it-IT");

  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("ADDEBITO DIRETTO", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
     * ADDEBITO DIRETTO
     * core rcur prg.car.: 243581263023787 c1264100108714 previato stefano it540010000000734480 213 stadtwerke bruneck - dok.nr./n.doc. 2024-455721 - telecomunicazioni mese 12/2024 - crbzit2bxxx
     */
    string transactionId = ParseTransactionId(rawDescription);
    string contractId = ParseContractId(rawDescription);
    string owner = ParseOwner(rawDescription);
    string creditorId = ParseCreditorId(rawDescription);
    string creditorName = ParseCreditorName(rawDescription);
    string reason = ParseReason(rawDescription);
    string swiftCode = ParseSwiftCode(rawDescription);

    return new DirectDebitItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      transactionId,
      contractId,
      owner,
      creditorId,
      creditorName,
      reason,
      swiftCode
    );
  }

  private string ParseSwiftCode(string rawDescription)
  {
    return rawDescription
      .Split(Constants.NewLines, StringSplitOptions.RemoveEmptyEntries)[1]
      .Split(" - ")[^1]
      .ToUpper(_cultureInfo);
  }

  private static string ParseReason(string rawDescription)
  {
    string[] raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.RemoveEmptyEntries)[1]
      .Split(" - ")[1..^1];

    return string.Join(" - ", raw);
  }

  private string ParseCreditorName(string rawDescription)
  {
    string[] raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.RemoveEmptyEntries)[1]
      .Split(StartingHeaders, StringSplitOptions.None)[1]
      .Split(" - ")[0]
      .Split(" ")[6..];

    string joined = string.Join(" ", raw);

    return _cultureInfo.TextInfo.ToTitleCase(joined);
  }

  private string ParseCreditorId(string rawDescription)
  {
    string[] raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.RemoveEmptyEntries)[1]
      .Split(StartingHeaders, StringSplitOptions.None)[1]
      .Split(" ")[4..6];

    return string.Join("", raw).ToUpper(_cultureInfo);
  }

  private static string ParseOwner(string rawDescription)
  {
    string[] raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.RemoveEmptyEntries)[1]
      .Split(StartingHeaders, StringSplitOptions.None)[1]
      .Split(" ")[2..4];

    return string.Join(" ", raw);
  }

  private static string ParseContractId(string rawDescription)
  {
    return rawDescription
      .Split(Constants.NewLines, StringSplitOptions.RemoveEmptyEntries)[1]
      .Split(StartingHeaders, StringSplitOptions.None)[1]
      .Split(" ")[1];
  }

  private static string ParseTransactionId(string rawDescription)
  {
    return rawDescription
      .Split(Constants.NewLines, StringSplitOptions.RemoveEmptyEntries)[1]
      .Split(StartingHeaders, StringSplitOptions.None)[1]
      .Split(" ")[0];
  }

  private static readonly string[] StartingHeaders = ["core rcur prg.car.: ", "core frst prg.car.: "];
}