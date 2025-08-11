using Argon.Application.BankStatements.GetList;
using Argon.Application.BankStatements.Parse.Parsers;
using Moq;

namespace Argon.Application.Tests.BankStatements.GetList;

public class BankStatementsGetListHandlerTests
{
  private IApplicationDbContext _dbContext = null!;
  private Mock<IParser> _mockParser = null!;
  private BankStatementsGetListHandler _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _mockParser = new Mock<IParser>();

    _sut = new BankStatementsGetListHandler(_dbContext, new List<IParser>
    {
      _mockParser.Object,
    });
  }

  [Test]
  public async Task Handle_GivenBankStatementsExist_ShouldReturnListOfBankStatements()
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

    BankStatement bankStatement1 = new()
    {
      FileName = "statement1.xlsx",
      ParserId = parserId,
      ImportedToAccountId = account.Entity.Id,
      FileContent = new byte[]
      {
        0x01,
      },
      Transactions = new List<Transaction>
      {
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
        },
      },
    };
    BankStatement bankStatement2 = new()
    {
      FileName = "statement2.xlsx",
      ParserId = parserId,
      ImportedToAccountId = account.Entity.Id,
      FileContent = new byte[]
      {
        0x01,
      },
      Transactions = new List<Transaction>
      {
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
        },
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
        },
      },
    };

    await _dbContext.BankStatements.AddAsync(bankStatement1);
    await _dbContext.BankStatements.AddAsync(bankStatement2);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    BankStatementsGetListRequest request = new();

    // Act
    List<BankStatementsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // Assert
    result.Should().HaveCount(2);

    result.Should().Contain(bs => bs.FileName == "statement1.xlsx" && bs.TransactionsCount == 1 && bs.ParserName == "Test Parser");
    result.Should().Contain(bs => bs.FileName == "statement2.xlsx" && bs.TransactionsCount == 2 && bs.ParserName == "Test Parser");
  }
}