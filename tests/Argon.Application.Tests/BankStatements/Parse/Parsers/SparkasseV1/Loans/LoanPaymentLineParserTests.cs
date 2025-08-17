using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Loans;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.Loans;

[TestFixture]
public class LoanPaymentLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new LoanPaymentLineParser();
  }

  private LoanPaymentLineParser _sut = null!;

  [TestCaseSource(nameof(TestCases))]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput((DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount, LoanPaymentItem output) testCase)
  {
    BaseItem result = _sut.Parse(testCase.accountingDate, testCase.currencyDate, testCase.rawDescription, testCase.amount);
    result.Should().BeEquivalentTo(testCase.output);
  }

  [TestCase("PAGAMENTO PRESTITO")]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    bool result = _sut.CanParse(rawDescription);
    result.Should().BeTrue();
  }

  public static IEnumerable<(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount, LoanPaymentItem output)> TestCases()
  {
    (DateOnly, DateOnly, string, decimal, LoanPaymentItem) CreateTestCase(
      DateOnly accountingDate,
      DateOnly currencyDate,
      string rawDescription,
      decimal amount,
      string loanId,
      int installmentNumber)
    {
      return (accountingDate, currencyDate, rawDescription, amount,
        new LoanPaymentItem(
          accountingDate,
          currencyDate,
          rawDescription,
          amount,
          loanId,
          installmentNumber
        ));
    }

    yield return CreateTestCase(
      new DateOnly(2025, 1, 15),
      new DateOnly(2025, 1, 14),
      """
      PAGAMENTO PRESTITO
      006/00119277 r*900
      """,
      300.00m,
      "006/00119277",
      0
    );

    yield return CreateTestCase(
      new DateOnly(2025, 2, 15),
      new DateOnly(2025, 2, 14),
      """
      PAGAMENTO PRESTITO
      006/00119277 r.001
      """,
      300.00m,
      "006/00119277",
      1
    );
  }
}