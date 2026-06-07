namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Taxes;

public class BankFeeLineParser : ILineParser
{
  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("CANONE", StringComparison.InvariantCultureIgnoreCase)
           || rawDescription.Contains("COMPETENZE", StringComparison.InvariantCultureIgnoreCase)
           || rawDescription.Contains("COMMISSIONE", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
     * CANONE
     *
     */
    /*
     * COMPETENZE
     * spese spese per operazioni 8,00-
     */
    /*
     * COMMISSIONE
     */
    return new BankFeeItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount
    );
  }
}
