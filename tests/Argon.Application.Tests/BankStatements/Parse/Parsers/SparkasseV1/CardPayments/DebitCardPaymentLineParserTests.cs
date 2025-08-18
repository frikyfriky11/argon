using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.CardPayments;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.CardPayments;

[TestFixture]
public class DebitCardPaymentLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new DebitCardPaymentLineParser();
  }

  private DebitCardPaymentLineParser _sut = null!;

  [TestCaseSource(nameof(TestCases))]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput((DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount, CardPaymentItem output) testCase)
  {
    BaseItem result = _sut.Parse(testCase.accountingDate, testCase.currencyDate, testCase.rawDescription, testCase.amount);
    result.Should().BeEquivalentTo(testCase.output);
  }

  [TestCase("PAGAMENTO DEBITO VISA/MASTERCARD")]
  [TestCase("PAGAM. POS MAESTRO CARTA")]
  [TestCase("ACCREDITO VISA")]
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
      string cityName,
      string currencyCode,
      string countryName,
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
      new DateOnly(2025, 2, 7),
      new DateOnly(2025, 2, 6),
      """
      PAGAMENTO DEBITO VISA/MASTERCARD
      del 06/02/25 in italia a terlano valuta eur paese italia c/o eurospin s15 carta n. 416363******3171
      """,
      70.38m,
      "Visa/Mastercard",
      new DateOnly(2025, 2, 6),
      false,
      "Terlano",
      "EUR",
      "Italia",
      "Eurospin S15",
      "416363******3171"
    );

    yield return CreateTestCase(
      new DateOnly(2024, 12, 8),
      new DateOnly(2024, 12, 5),
      """
      PAGAM. POS MAESTRO CARTA 6351744
      del 02/12/24 16:37 in italia a merano ita valuta eur paese italia c/o hotel therme meran thermenplatz 1
      """,
      15.35m,
      "Maestro",
      new DateOnly(2024, 12, 2),
      false,
      "Merano Ita",
      "EUR",
      "Italia",
      "Hotel Therme Meran Thermenplatz 1",
      "6351744"
    );

    yield return CreateTestCase(
      new DateOnly(2025, 3, 5),
      new DateOnly(2025, 2, 27),
      """
      PAGAMENTO DEBITO VISA/MASTERCARD
      del 27/02/25 all'estero a 917696959 valuta eur paese germania c/o paypal *zalandose carta n. 416363******3171
      """,
      38.98m,
      "Visa/Mastercard",
      new DateOnly(2025, 2, 27),
      true,
      "917696959",
      "EUR",
      "Germania",
      "Paypal *Zalandose",
      "416363******3171"
    );

    yield return CreateTestCase(
      new DateOnly(2025, 5, 7),
      new DateOnly(2025, 5, 3),
      """
      ACCREDITO VISA
      del 03/05/25 all'estero a 800-279-6620 valuta eur paese lussemburgo c/o amzn mktp it carta n. 416363******3171
      """,
      10.01m,
      "Visa/Mastercard",
      new DateOnly(2025, 5, 3),
      true,
      "800-279-6620",
      "EUR",
      "Lussemburgo",
      "Amzn Mktp It",
      "416363******3171"
    );
  }
}
