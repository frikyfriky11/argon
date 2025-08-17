using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Emoluments;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.Emoluments;

[TestFixture]
public class DrScharEmolumentLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new DrScharEmolumentLineParser();
  }

  private DrScharEmolumentLineParser _sut = null!;

  [Test]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput()
  {
    // Arrange
    DateOnly accountingDate = new(2025, 2, 7);
    DateOnly currencyDate = new(2025, 2, 6);
    const string rawDescription =
      """
      EMOLUMENTI
      dr. schaer ag data regolamento: 06/02/25 cod.id.ord: it18 z081 3358 5910 0030 1002 538 banca ordinante: 08133/58591-rzsbit21119 cro: b250203643722708275859158710it note: accredito competenze mese di gennaio 2025 id.operazione: notprovided
      """;
    const decimal amount = 2345.67m;

    EmolumentItem expected = new(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      "Dr. Schär AG",
      new DateOnly(2025, 2, 6),
      new DateOnly(2025, 1, 1),
      false
    );

    // Act
    BaseItem result = _sut.Parse(accountingDate, currencyDate, rawDescription, amount);

    // Assert
    result.Should().BeEquivalentTo(expected);
  }

  [TestCase(
    """
    EMOLUMENTI
    dr. schaer ag data regolamento: 06/02/25 cod.id.ord: it18 z081 3358 5910 0030 1002 538 banca ordinante: 08133/58591-rzsbit21119 cro: b250203643722708275859158710it note: accredito competenze mese di gennaio 2025 id.operazione: notprovided
    """)]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    // Act
    bool result = _sut.CanParse(rawDescription);

    // Assert
    result.Should().BeTrue();
  }
}