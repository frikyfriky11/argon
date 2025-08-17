using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.IncomingBankTransfers;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.IncomingBankTransfers;

[TestFixture]
public class IncomingBankTransferLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new IncomingBankTransferLineParser();
  }

  private IncomingBankTransferLineParser _sut = null!;

  [TestCaseSource(nameof(TestCases))]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput((DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount, IncomingBankTransferItem output) testCase)
  {
    BaseItem result = _sut.Parse(testCase.accountingDate, testCase.currencyDate, testCase.rawDescription, testCase.amount);
    result.Should().BeEquivalentTo(testCase.output);
  }

  [TestCase("BONIFICO A VOSTRO FAVORE")]
  [TestCase("BONIFICO VS. FAVORE")]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    bool result = _sut.CanParse(rawDescription);
    result.Should().BeTrue();
  }

  public static IEnumerable<(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount, IncomingBankTransferItem output)> TestCases()
  {
    (DateOnly, DateOnly, string, decimal, IncomingBankTransferItem) CreateTestCase(
      DateOnly accountingDate,
      DateOnly currencyDate,
      string rawDescription,
      decimal amount,
      string senderName,
      DateOnly orderDate,
      string senderIban,
      string reason,
      bool isSameBank)
    {
      return (accountingDate, currencyDate, rawDescription, amount,
        new IncomingBankTransferItem(
          accountingDate,
          currencyDate,
          rawDescription,
          amount,
          senderName,
          orderDate,
          senderIban,
          reason,
          isSameBank
        ));
    }

    yield return CreateTestCase(
      new DateOnly(2025, 1, 15),
      new DateOnly(2025, 1, 14),
      """
      BONIFICO A VOSTRO FAVORE
      vicentini miriam data regolamento: 14/01/25 cod.id.ord: it18 i081 1558 4910 0030 4031 458 banca ordinante: 08115/58491-rzsbit21314 cro: b250101341034109485849158490it note: rimborso ikea id.operazione: notprovided
      """,
      100.00m,
      "Vicentini Miriam",
      new DateOnly(2025, 1, 14),
      "IT18I0811558491000304031458",
      "rimborso ikea",
      false
    );

    yield return CreateTestCase(
      new DateOnly(2025, 1, 14),
      new DateOnly(2025, 1, 13),
      """
      BONIFICO VS. FAVORE
      zanirato giuseppe val. accredito: 13/01/25 cod.id.ord: it41 a060 4558 9600 0000 5350 025 cro: 28035873002 note: rimborso pizza
      """,
      20.00m,
      "Zanirato Giuseppe",
      new DateOnly(2025, 1, 13),
      "IT41A0604558960000005350025",
      "rimborso pizza",
      true
    );

    yield return CreateTestCase(
      new DateOnly(2024, 12, 28),
      new DateOnly(2024, 12, 27),
      """
      BONIFICO VS. FAVORE
      langebner walter, kammerlander karmen wertstellung: 27/12/24 id.zahler: it97 d060 4558 9600 0000 5001 391 cro: 28023069012 beschreibung: telecamera
      """,
      50.00m,
      "Langebner Walter, Kammerlander Karmen",
      new DateOnly(2024, 12, 27),
      "IT97D0604558960000005001391",
      "telecamera",
      true
    );
  }
}