using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Cash;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.Cash;

[TestFixture]
public class WithdrawalLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new WithdrawalLineParser();
  }

  private WithdrawalLineParser _sut = null!;

  [Test]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput()
  {
    // Arrange
    DateOnly accountingDate = new(2024, 12, 31);
    DateOnly currencyDate = new(2024, 12, 30);
    const string rawDescription = "PRELIEVO DI CONTANTE";
    const decimal amount = 100.00m;

    WithdrawalItem expected = new(
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

  [TestCase("PRELIEVO DI CONTANTE")]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    // Act
    bool result = _sut.CanParse(rawDescription);

    // Assert
    result.Should().BeTrue();
  }
}