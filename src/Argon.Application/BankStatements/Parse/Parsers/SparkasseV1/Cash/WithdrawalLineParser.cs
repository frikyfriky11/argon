namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Cash;

public class WithdrawalLineParser : ILineParser
{
  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("PRELIEVO DI CONTANTE", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
     * PRELIEVO DI CONTANTE
     *
     */
    return new WithdrawalItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount
    );
  }
}