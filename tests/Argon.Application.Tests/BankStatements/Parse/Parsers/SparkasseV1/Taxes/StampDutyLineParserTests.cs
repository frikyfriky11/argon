using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Taxes;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.Taxes;

[TestFixture]
public class StampDutyLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new StampDutyLineParser();
  }

  private StampDutyLineParser _sut = null!;

  [Test]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput()
  {
    // Arrange
    DateOnly accountingDate = new(2025, 2, 1);
    DateOnly currencyDate = new(2025, 2, 1);
    const string rawDescription =
      """
      IMPOSTA DI BOLLO SU RENDICONTO
      recupero periodo dal 01/01/25 al 31/01/25
      """;
    const decimal amount = 2.52m;

    StampDutyItem expected = new(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      new DateOnly(2025, 1, 1),
      new DateOnly(2025, 1, 31)
    );

    // Act
    BaseItem result = _sut.Parse(accountingDate, currencyDate, rawDescription, amount);

    // Assert
    result.Should().BeEquivalentTo(expected);
  }

  [TestCase("IMPOSTA DI BOLLO SU RENDICONTO")]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    // Act
    bool result = _sut.CanParse(rawDescription);

    // Assert
    result.Should().BeTrue();
  }
}