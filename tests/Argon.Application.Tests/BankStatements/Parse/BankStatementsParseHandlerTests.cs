using Argon.Application.BankStatements.Parse;
using Argon.Application.BankStatements.Parse.Parsers;
using Moq;

namespace Argon.Application.Tests.BankStatements.Parse;

public class BankStatementsParseHandlerTests
{
  private IApplicationDbContext _dbContext = null!;
  private Mock<IParser> _mockParser = null!;
  private Mock<IParsersFactory> _mockParsersFactory = null!;
  private BankStatementsParseHandler _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _mockParsersFactory = new Mock<IParsersFactory>();
    _mockParser = new Mock<IParser>();

    _sut = new BankStatementsParseHandler(_mockParsersFactory.Object, _dbContext);
  }

  [Test]
  public async Task Handle_ShouldPersistBankStatementAndTransactions_WhenRequestIsValid()
  {
    // Arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account
    {
      Name = "Test Account",
      Type = AccountType.Cash,
    });

    Guid parserId = Guid.NewGuid();
    string parserDisplayName = "Test Parser";
    _mockParser.Setup(p => p.ParserId).Returns(parserId);
    _mockParser.Setup(p => p.ParserDisplayName).Returns(parserDisplayName);
    _mockParser.Setup(p => p.ParseAsync(It.IsAny<Stream>()))
      .ReturnsAsync(new List<BankStatementItem>
      {
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
          Amount = 100,
          RawInput = "Test Data 1",
          CounterpartyName = "Test Counterparty 1",
        },
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
          Amount = -50,
          RawInput = "Test Data 2",
          CounterpartyName = "Test Counterparty 2",
        },
      });

    _mockParsersFactory.Setup(f => f.CreateParserAsync(parserId)).ReturnsAsync(_mockParser.Object);

    BankStatementsParseRequest request = new(
      new byte[]
      {
        0x01, 0x02,
      },
      "test.xlsx",
      parserId,
      account.Entity.Id
    );

    // Act
    BankStatementsParseResponse response = await _sut.Handle(request, CancellationToken.None);

    // Assert
    response.BankStatementId.Should().NotBeEmpty();

    BankStatement? bankStatement = await _dbContext
      .BankStatements
      .Include(bs => bs.Transactions)
      .ThenInclude(t => t.TransactionRows)
      .FirstOrDefaultAsync(bs => bs.Id == response.BankStatementId);

    bankStatement.Should().NotBeNull();
    bankStatement!.FileName.Should().Be("test.xlsx");
    bankStatement.ParserId.Should().Be(parserId);
    bankStatement.ImportedToAccountId.Should().Be(account.Entity.Id);
    bankStatement.FileContent.Should().NotBeEmpty();
    bankStatement.Transactions.Should().HaveCount(2);

    Transaction transaction1 = bankStatement.Transactions.First();
    transaction1.Date.Should().Be(DateOnly.FromDateTime(DateTime.Now));
    transaction1.Status.Should().Be(TransactionStatus.PendingImportReview);
    transaction1.TransactionRows.Should().HaveCount(2);
    transaction1.TransactionRows.First().AccountId.Should().Be(account.Entity.Id);

    Transaction transaction2 = bankStatement.Transactions.Last();
    transaction2.Date.Should().Be(DateOnly.FromDateTime(DateTime.Now));
    transaction2.Status.Should().Be(TransactionStatus.PendingImportReview);
    transaction2.TransactionRows.Should().HaveCount(2);
    transaction2.TransactionRows.First().AccountId.Should().Be(account.Entity.Id);
  }

  [Test]
  public async Task Handle_ShouldReturnWarnings_WhenParsingIsUnsuccessful()
  {
    // Arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account
    {
      Name = "Test Account",
      Type = AccountType.Cash,
    });

    Guid parserId = Guid.NewGuid();
    string parserDisplayName = "Test Parser";
    _mockParser.Setup(p => p.ParserId).Returns(parserId);
    _mockParser.Setup(p => p.ParserDisplayName).Returns(parserDisplayName);
    _mockParser.Setup(p => p.ParseAsync(It.IsAny<Stream>()))
      .ReturnsAsync(new List<BankStatementItem>
      {
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
          Amount = 100,
          RawInput = "Valid Data",
          CounterpartyName = "Test Counterparty 1",
        },
        new()
        {
          ErrorMessage = "Invalid format",
          RawInput = "Bad Data",
        },
      });

    _mockParsersFactory.Setup(f => f.CreateParserAsync(parserId)).ReturnsAsync(_mockParser.Object);

    BankStatementsParseRequest request = new(
      new byte[]
      {
        0x01, 0x02,
      },
      "test.xlsx",
      parserId,
      account.Entity.Id
    );

    // Act
    BankStatementsParseResponse response = await _sut.Handle(request, CancellationToken.None);

    // Assert
    response.Warnings.Should().NotBeEmpty();
    response.Warnings.Should().ContainSingle(w => w.Contains("Invalid format"));
  }

  [Test]
  public async Task Handle_ShouldReturnWarning_WhenNoCounterpartyMatches()
  {
    // Arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account
    {
      Name = "Test Account",
      Type = AccountType.Cash,
    });

    Guid parserId = Guid.NewGuid();
    string parserDisplayName = "Test Parser";
    _mockParser.Setup(p => p.ParserId).Returns(parserId);
    _mockParser.Setup(p => p.ParserDisplayName).Returns(parserDisplayName);
    _mockParser.Setup(p => p.ParseAsync(It.IsAny<Stream>()))
      .ReturnsAsync(new List<BankStatementItem>
      {
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
          Amount = 100,
          RawInput = "Valid Data",
          CounterpartyName = "NonExistent Counterparty",
        },
      });

    _mockParsersFactory.Setup(f => f.CreateParserAsync(parserId)).ReturnsAsync(_mockParser.Object);

    BankStatementsParseRequest request = new(
      new byte[]
      {
        0x01, 0x02,
      },
      "test.xlsx",
      parserId,
      account.Entity.Id
    );

    // Act
    BankStatementsParseResponse response = await _sut.Handle(request, CancellationToken.None);

    // Assert
    response.Warnings.Should().NotBeEmpty();
    response.Warnings.Should().ContainSingle(w => w.Contains("No counterparty found matching 'NonExistent Counterparty' while parsing 'Valid Data'. No counterparty assigned."));
  }

  [Test]
  public async Task Handle_ShouldReturnWarning_WhenMultipleCounterpartiesMatch()
  {
    // Arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account
    {
      Name = "Test Account",
      Type = AccountType.Cash,
    });

    Counterparty counterparty = new()
    {
      Name = "Multiple Match",
    };
    await _dbContext.Counterparties.AddAsync(counterparty);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    await _dbContext.CounterpartyIdentifiers.AddAsync(new CounterpartyIdentifier
    {
      CounterpartyId = counterparty.Id,
      IdentifierText = "Multiple Match",
    });
    await _dbContext.CounterpartyIdentifiers.AddAsync(new CounterpartyIdentifier
    {
      CounterpartyId = counterparty.Id,
      IdentifierText = "Multiple Match",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    Guid parserId = Guid.NewGuid();
    string parserDisplayName = "Test Parser";
    _mockParser.Setup(p => p.ParserId).Returns(parserId);
    _mockParser.Setup(p => p.ParserDisplayName).Returns(parserDisplayName);
    _mockParser.Setup(p => p.ParseAsync(It.IsAny<Stream>()))
      .ReturnsAsync(new List<BankStatementItem>
      {
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
          Amount = 100,
          RawInput = "Valid Data",
          CounterpartyName = "Multiple Match",
        },
      });

    _mockParsersFactory.Setup(f => f.CreateParserAsync(parserId)).ReturnsAsync(_mockParser.Object);

    BankStatementsParseRequest request = new(
      new byte[]
      {
        0x01, 0x02,
      },
      "test.xlsx",
      parserId,
      account.Entity.Id
    );

    // Act
    BankStatementsParseResponse response = await _sut.Handle(request, CancellationToken.None);

    // Assert
    response.Warnings.Should().NotBeEmpty();
    response.Warnings.Should().ContainSingle(w => w.Contains("Found 2 counterparties matching 'Multiple Match' while parsing 'Valid Data'. No counterparty assigned."));
  }

  [Test]
  public async Task Handle_ShouldSetStatusAndDuplicateId_WhenAPotentialDuplicateIsFound()
  {
    // Arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account
    {
      Name = "Test Account",
      Type = AccountType.Cash,
    });

    Transaction existingTransaction = new()
    {
      Date = DateOnly.FromDateTime(DateTime.Now),
      CounterpartyId = null,
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow>
      {
        new()
        {
          AccountId = account.Entity.Id,
          Debit = 100,
          RowCounter = 1,
        },
      },
    };
    await _dbContext.Transactions.AddAsync(existingTransaction);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    Guid parserId = Guid.NewGuid();
    string parserDisplayName = "Test Parser";
    _mockParser.Setup(p => p.ParserId).Returns(parserId);
    _mockParser.Setup(p => p.ParserDisplayName).Returns(parserDisplayName);
    _mockParser.Setup(p => p.ParseAsync(It.IsAny<Stream>()))
      .ReturnsAsync(new List<BankStatementItem>
      {
        new()
        {
          Date = DateOnly.FromDateTime(DateTime.Now),
          Amount = 100,
          RawInput = "Duplicate Data",
          CounterpartyName = "Some Counterparty",
        },
      });

    _mockParsersFactory.Setup(f => f.CreateParserAsync(parserId)).ReturnsAsync(_mockParser.Object);

    BankStatementsParseRequest request = new(
      new byte[]
      {
        0x01, 0x02,
      },
      "test.xlsx",
      parserId,
      account.Entity.Id
    );

    // Act
    BankStatementsParseResponse response = await _sut.Handle(request, CancellationToken.None);

    // Assert
    response.Should().NotBeNull();
    BankStatement? bankStatement = await _dbContext
      .BankStatements
      .Include(bs => bs.Transactions)
      .FirstOrDefaultAsync(bs => bs.Id == response.BankStatementId);

    bankStatement.Should().NotBeNull();
    bankStatement!.Transactions.Should().HaveCount(1);

    Transaction importedTransaction = bankStatement.Transactions.First();
    importedTransaction.Status.Should().Be(TransactionStatus.PotentialDuplicate);
    importedTransaction.PotentialDuplicateOfTransactionId.Should().Be(existingTransaction.Id);
  }
}