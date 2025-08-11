using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Sepa;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.Sepa;

[TestFixture]
public class DirectDebitLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new DirectDebitLineParser();
  }

  private DirectDebitLineParser _sut = null!;

  [Test]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput()
  {
    // Arrange
    DateOnly accountingDate = new(2025, 1, 1);
    DateOnly currencyDate = new(2025, 1, 1);
    const string rawDescription =
      """
      ADDEBITO DIRETTO
      core rcur prg.car.: 243581263023787 c1264100108714 previato stefano it540010000000734480 213 stadtwerke bruneck - dok.nr./n.doc. 2024-455721 - telecomunicazioni mese 12/2024 - crbzit2bxxx
      """;
    const decimal amount = 50.00m;

    DirectDebitItem expected = new(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      "243581263023787",
      "c1264100108714",
      "previato stefano",
      "IT540010000000734480213",
      "Stadtwerke Bruneck",
      "dok.nr./n.doc. 2024-455721 - telecomunicazioni mese 12/2024",
      "CRBZIT2BXXX"
    );

    // Act
    BaseItem result = _sut.Parse(accountingDate, currencyDate, rawDescription, amount);

    // Assert
    result.Should().BeEquivalentTo(expected);
  }

  [TestCase("ADDEBITO DIRETTO")]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    // Act
    bool result = _sut.CanParse(rawDescription);

    // Assert
    result.Should().BeTrue();
  }
}