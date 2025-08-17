using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Emoluments;

namespace Argon.Application.Tests.BankStatements.Parse.Parsers.SparkasseV1.Emoluments;

[TestFixture]
public class AziendaSanitariaEmolumentLineParserTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new AziendaSanitariaEmolumentLineParser();
  }

  private AziendaSanitariaEmolumentLineParser _sut = null!;

  [Test]
  [SetCulture("en-US")] // Set culture to ensure dates are parsed correctly regardless of the running thread's culture
  public void Parse_ShouldReturnCorrectOutput_GivenValidInput()
  {
    // Arrange
    DateOnly accountingDate = new(2025, 1, 28);
    DateOnly currencyDate = new(2025, 1, 27);
    const string rawDescription =
      """
      EMOLUMENTI
      uri : 2025-01-21 16:38:00.980437 emolumenti ordinante : suedtiroler sanitaetsbetrieb - azienda compensi gennaio 2025 *data ordine 270125*    
      """;
    const decimal amount = 1234.56m;

    EmolumentItem expected = new(
      accountingDate,
      currencyDate,
      rawDescription,
      amount,
      "Azienda Sanitaria dell'Alto Adige",
      new DateOnly(2025, 1, 27),
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
    uri : 2025-01-21 16:38:00.980437 emolumenti ordinante : suedtiroler sanitaetsbetrieb - azienda compensi gennaio 2025 *data ordine 270125*
    """)]
  public void CanParse_ShouldReturnTrue_GivenValidInput(string rawDescription)
  {
    // Act
    bool result = _sut.CanParse(rawDescription);

    // Assert
    result.Should().BeTrue();
  }
}