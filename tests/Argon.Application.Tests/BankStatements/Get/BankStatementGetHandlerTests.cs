using Argon.Application.BankStatements.Get;
using Argon.Application.BankStatements.Parse.Parsers;
using Moq;

namespace Argon.Application.Tests.BankStatements.Get;

public class BankStatementGetHandlerTests
{
  private IApplicationDbContext _dbContext = null!;
  private Mock<IParser> _mockParser = null!;
  private BankStatementGetHandler _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _mockParser = new Mock<IParser>();

    _sut = new BankStatementGetHandler(_dbContext, new List<IParser>
    {
      _mockParser.Object,
    });
  }

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // Arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account
    {
      Name = "Test Account",
      Type = AccountType.Cash,
    });

    Guid parserId = Guid.NewGuid();
    _mockParser.Setup(p => p.ParserId).Returns(parserId);
    _mockParser.Setup(p => p.ParserDisplayName).Returns("Test Parser");

    BankStatement bankStatement = new()
    {
      FileName = "test.xlsx",
      ParserId = parserId,
      ImportedToAccountId = account.Entity.Id,
      FileContent = [0x01],
      Transactions = new List<Transaction>
      {
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
          Status = TransactionStatus.PendingImportReview,
          RawImportData = "{}",
        },
      },
    };

    await _dbContext.BankStatements.AddAsync(bankStatement);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    BankStatementGetRequest request = new(bankStatement.Id);

    // Act
    BankStatementGetResponse result = await _sut.Handle(request, CancellationToken.None);

    // Assert
    result.Id.Should().Be(bankStatement.Id);
    result.FileName.Should().Be("test.xlsx");
    result.ParserName.Should().Be("Test Parser");
    result.Transactions.Should().HaveCount(1);
    result.Transactions.First().Status.Should().Be(TransactionStatus.PendingImportReview);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    BankStatementGetRequest request = new(Guid.NewGuid());

    // act
    Func<Task<BankStatementGetResponse>> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}