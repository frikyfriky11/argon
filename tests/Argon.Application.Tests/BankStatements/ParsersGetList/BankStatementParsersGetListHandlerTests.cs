using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.ParsersGetList;
using Moq;

namespace Argon.Application.Tests.BankStatements.ParsersGetList;

public class BankStatementParsersGetListHandlerTests
{
  private Mock<IParser> _mockParser1 = null!;
  private Mock<IParser> _mockParser2 = null!;

  private BankStatementParsersGetListHandler _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _mockParser1 = new Mock<IParser>();
    _mockParser1.Setup(p => p.ParserId).Returns(Guid.NewGuid());
    _mockParser1.Setup(p => p.ParserDisplayName).Returns("Parser One");

    _mockParser2 = new Mock<IParser>();
    _mockParser2.Setup(p => p.ParserId).Returns(Guid.NewGuid());
    _mockParser2.Setup(p => p.ParserDisplayName).Returns("Parser Two");

    _sut = new BankStatementParsersGetListHandler(new List<IParser>
    {
      _mockParser1.Object,
      _mockParser2.Object,
    });
  }

  [Test]
  public async Task Handle_ShouldReturnListOfParsers()
  {
    // Arrange
    BankStatementParsersGetListRequest request = new();

    // Act
    List<BankStatementParsersGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().HaveCount(2);
    result.Should().Contain(p => p.ParserDisplayName == "Parser One");
    result.Should().Contain(p => p.ParserDisplayName == "Parser Two");
  }
}