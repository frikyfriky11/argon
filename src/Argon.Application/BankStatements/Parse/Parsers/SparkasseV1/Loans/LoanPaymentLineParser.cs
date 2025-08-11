namespace Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Loans;

public class LoanPaymentLineParser : ILineParser
{
  public bool CanParse(string rawDescription)
  {
    return rawDescription.Contains("PAGAMENTO PRESTITO", StringComparison.InvariantCultureIgnoreCase);
  }

  public BaseItem Parse(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount)
  {
    // sample raw description string:
    /*
     * PAGAMENTO PRESTITO
     * 006/00119277 r*900
     */
    /*
     * PAGAMENTO PRESTITO
     * 006/00119277 r.001
     */
    string loanId = ParseLoanId(rawDescription);
    int installmentNumber = ParseInstallmentNumber(rawDescription);

    return new LoanPaymentItem(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      loanId,
      installmentNumber
    );
  }

  private static int ParseInstallmentNumber(string rawDescription)
  {
    if (!rawDescription.Contains(" r.")) return 0;

    string raw = rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" r.")[1];

    return int.Parse(raw, CultureInfo.InvariantCulture);
  }

  private static string ParseLoanId(string rawDescription)
  {
    return rawDescription
      .Split(Constants.NewLines, StringSplitOptions.None)[1]
      .Split(" ")[0];
  }
}