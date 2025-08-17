using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.CardPayments;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.CardPayments;

[TestFixture]
public class BancomatCardPaymentLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new BancomatCardPaymentLineParser();
  }

  private BancomatCardPaymentLineParser _sut = null!;

  [TestCaseSource(nameof(TestCases))]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput((DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount, CardPaymentItem output) testCase)
  {
    BaseItem result = _sut.Parse(testCase.accountingDate, testCase.currencyDate, testCase.rawDescription, testCase.amount);
    result.Should().BeEquivalentTo(testCase.output);
  }

  [TestCase("PAGAMENTO POS CARTA")]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    bool result = _sut.CanParse(rawDescription);
    result.Should().BeTrue();
  }

  private static IEnumerable<(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount, CardPaymentItem output)> TestCases()
  {
    (DateOnly, DateOnly, string, decimal, CardPaymentItem) CreateTestCase(
      DateOnly accountingDate,
      DateOnly currencyDate,
      string rawDescription,
      decimal amount,
      string circuitName,
      DateOnly paymentDate,
      bool isForeignCountry,
      string? cityName,
      string? currencyCode,
      string? countryName,
      string recipientName,
      string cardNumber)
    {
      return (accountingDate, currencyDate, rawDescription, amount,
        new CardPaymentItem(
          accountingDate,
          currencyDate,
          rawDescription,
          amount,
          circuitName,
          paymentDate,
          isForeignCountry,
          cityName,
          currencyCode,
          countryName,
          recipientName,
          cardNumber
        ));
    }

    yield return CreateTestCase(
      new DateOnly(2024, 12, 31),
      new DateOnly(2024, 12, 30),
      """
      PAGAMENTO POS CARTA 6351753
      eseguito il 30/12/24 c/o 41298 bolzano - resia
      """,
      12.34m,
      "Bancomat",
      new DateOnly(2024, 12, 30),
      false,
      null,
      null,
      null,
      "41298 Bolzano - Resia",
      "6351753"
    );
  }
}