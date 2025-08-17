using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.OutgoingBankTransfers;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.OutgoingBankTransfers;

[TestFixture]
public class OutgoingBankTransferLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new OutgoingBankTransferLineParser();
  }

  private OutgoingBankTransferLineParser _sut = null!;

  [TestCaseSource(nameof(TestCases))]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput((DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount, OutgoingBankTransferItem output) testCase)
  {
    BaseItem result = _sut.Parse(testCase.accountingDate, testCase.currencyDate, testCase.rawDescription, testCase.amount);
    result.Should().BeEquivalentTo(testCase.output);
  }

  [TestCase("DISPOSIZIONE RIPETITIVA")]
  [TestCase("VS DISPOSIZIONE DI BONIFICO")]
  [TestCase("ADDEBITO BONIFICO DA HOME BANKING")]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    bool result = _sut.CanParse(rawDescription);
    result.Should().BeTrue();
  }

  public static IEnumerable<(DateOnly accountingDate, DateOnly currencyDate, string rawDescription, decimal amount, OutgoingBankTransferItem output)> TestCases()
  {
    (DateOnly, DateOnly, string, decimal, OutgoingBankTransferItem) CreateTestCase(
      DateOnly accountingDate,
      DateOnly currencyDate,
      string rawDescription,
      decimal amount,
      string recipientName,
      bool isRecurring,
      DateOnly orderDate,
      DateOnly paymentDate,
      string reason,
      bool isSameBank)
    {
      return (accountingDate, currencyDate, rawDescription, amount,
        new OutgoingBankTransferItem(
          accountingDate,
          currencyDate,
          rawDescription,
          amount,
          recipientName,
          isRecurring,
          orderDate,
          paymentDate,
          reason,
          isSameBank
        ));
    }

    yield return CreateTestCase(
      new DateOnly(2025, 2, 6),
      new DateOnly(2025, 2, 6),
      """
      DISPOSIZIONE RIPETITIVA
      directa sim s.p.a. c/terzi - banca sell bonifico disposto in: accentrato coor.benef.: it15 d032 6822 3000 5231 5040 240 banca destinataria: 03268/22300-selbit2bxxx data ordine: 05/02/25 data regolamento: 06/02/25 cro: 0000028053304803481160011606it codice cliente n. 88190
      """,
      500.00m,
      "Directa Sim S.P.A. C/Terzi - Banca Sell",
      true,
      new DateOnly(2025, 2, 5),
      new DateOnly(2025, 2, 6),
      "codice cliente n. 88190",
      false
    );

    yield return CreateTestCase(
      new DateOnly(2025, 1, 31),
      new DateOnly(2025, 1, 31),
      """
      VS DISPOSIZIONE DI BONIFICO
      cedocs societa' cooperativa sociale bonifico disposto in: internet coor.benef.: it04 f060 4511 6030 0000 1304 000 data ordine: 31/01/25 data accredito: 31/01/25 cro: 28049669612 pag. fatt. 218 del 31/01/2025
      """,
      150.00m,
      "Cedocs Societa' Cooperativa Sociale",
      false,
      new DateOnly(2025, 1, 31),
      new DateOnly(2025, 1, 31),
      "pag. fatt. 218 del 31/01/2025",
      true
    );

    yield return CreateTestCase(
      new DateOnly(2025, 1, 15),
      new DateOnly(2025, 1, 15),
      """
      ADDEBITO BONIFICO DA HOME BANKING
      stefano previato - lisa zanirato bonifico disposto in: internet coor.benef.: it02 t082 6958 9600 0000 3002 8668 1 banca destinataria: 08269/58960-rzsbit21042 data ordine: 14/01/25 data regolamento: 15/01/25 cro: 0000028037048808481160011606it spostamento liquidita
      """,
      1000.00m,
      "Stefano Previato - Lisa Zanirato",
      false,
      new DateOnly(2025, 1, 14),
      new DateOnly(2025, 1, 15),
      "spostamento liquidita",
      false
    );
  }
}