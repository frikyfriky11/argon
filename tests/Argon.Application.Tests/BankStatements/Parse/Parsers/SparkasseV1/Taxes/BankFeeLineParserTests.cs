using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Taxes;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.Taxes;

[TestFixture]
public class BankFeeLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new BankFeeLineParser();
  }

  private BankFeeLineParser _sut = null!;

  [Test]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput()
  {
    // Arrange
    DateOnly accountingDate = new(2025, 1, 1);
    DateOnly currencyDate = new(2025, 1, 1);
    const string rawDescription = "CANONE";
    const decimal amount = 5.00m;

    BankFeeItem expected = new(
      accountingDate,
      currencyDate,
      rawDescription,
      amount
    );

    // Act
    BaseItem result = _sut.Parse(accountingDate, currencyDate, rawDescription, amount);

    // Assert
    result.Should().BeEquivalentTo(expected);
  }

  [TestCase("CANONE")]
  [TestCase("COMPETENZE")]
  [TestCase("COMMISSIONE")]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput(string rawDescription)
  {
    // Arrange
    DateOnly accountingDate = new(2025, 1, 1);
    DateOnly currencyDate = new(2025, 1, 1);
    const decimal amount = 2.00m;

    BankFeeItem expected = new(
      accountingDate,
      currencyDate,
      rawDescription,
      amount
    );

    // Act
    BaseItem result = _sut.Parse(accountingDate, currencyDate, rawDescription, amount);

    // Assert
    result.Should().BeEquivalentTo(expected);
  }

  [TestCase("CANONE")]
  [TestCase("COMPETENZE")]
  [TestCase("COMMISSIONE")]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    // Act
    bool result = _sut.CanParse(rawDescription);

    // Assert
    result.Should().BeTrue();
  }
}
