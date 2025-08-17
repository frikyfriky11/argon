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

    Counterparty counterparty1 = new()
    {
      Name = "Amazon 1",
    };
    Counterparty counterparty2 = new()
    {
      Name = "Amazon 2",
    };
    await _dbContext.Counterparties.AddAsync(counterparty1);
    await _dbContext.Counterparties.AddAsync(counterparty2);
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
          CounterpartyName = "Amazon",
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
    response.Warnings.Should().ContainSingle(w => w.Contains("Found 2 counterparties matching 'Amazon' while parsing 'Valid Data'. No counterparty assigned."));
  }

  [Test]
  public async Task Handle_ShouldMatchCounterpartyCorrectly_WhenParsedNameIsSubsetOfExistingCounterparty()
  {
    // Arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account
    {
      Name = "Test Account",
      Type = AccountType.Cash,
    });

    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty
    {
      Name = "Amazon 1",
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
          CounterpartyName = "Amazon",
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
    response.Warnings.Should().BeEmpty();
    response.Should().NotBeNull();
    Transaction importedTransaction = await _dbContext
      .Transactions
      .FirstAsync(bs => bs.BankStatementId == response.BankStatementId);
    importedTransaction.CounterpartyId.Should().Be(counterparty.Entity.Id);
  }

  [Test]
  public async Task Handle_ShouldMatchCounterpartyCorrectly_WhenParsedNameIsSupersetOfExistingCounterparty()
  {
    // Arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account
    {
      Name = "Test Account",
      Type = AccountType.Cash,
    });

    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty
    {
      Name = "Amazon",
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
          CounterpartyName = "Amazon 123",
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
    response.Warnings.Should().BeEmpty();
    response.Should().NotBeNull();
    Transaction importedTransaction = await _dbContext
      .Transactions
      .FirstAsync(bs => bs.BankStatementId == response.BankStatementId);
    importedTransaction.CounterpartyId.Should().Be(counterparty.Entity.Id);
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
