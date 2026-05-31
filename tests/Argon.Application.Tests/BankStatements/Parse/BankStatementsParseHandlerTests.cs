using Argon.Application.BankStatements.Parse;
using Argon.Application.BankStatements.Parse.Parsers;
using Argon.Application.BankStatements.Parse.Parsers.SparkasseV1.Cash;
using Argon.Application.Counterparties.Common;
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

    _sut = new BankStatementsParseHandler(
      _mockParsersFactory.Object,
      _dbContext,
      new CounterpartyResolver(_dbContext));
  }

  private Guid SetupParser(params BankStatementItem[] items)
  {
    Guid parserId = Guid.NewGuid();
    _mockParser.Setup(p => p.ParserId).Returns(parserId);
    _mockParser.Setup(p => p.ParserDisplayName).Returns("Test Parser");
    _mockParser.Setup(p => p.ParseAsync(It.IsAny<Stream>())).ReturnsAsync(items.ToList());
    _mockParsersFactory.Setup(f => f.CreateParserAsync(parserId)).ReturnsAsync(_mockParser.Object);
    return parserId;
  }

  private static BankStatementsParseRequest RequestFor(Guid parserId, Guid accountId) =>
    new(new byte[] { 0x01, 0x02 }, "test.xlsx", parserId, accountId);

  private async Task<Account> SeedAccountAsync()
  {
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account
    {
      Name = "Test Account",
      Type = AccountType.Cash,
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);
    return account.Entity;
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

    // positive amount (incoming 100): the bank-side row is debited, the offsetting row credited
    Transaction positiveTransaction = bankStatement.Transactions.Single(t => t.TransactionRows.Any(r => r.Debit == 100));
    positiveTransaction.Date.Should().Be(DateOnly.FromDateTime(DateTime.Now));
    positiveTransaction.Status.Should().Be(TransactionStatus.PendingImportReview);
    positiveTransaction.TransactionRows.Should().HaveCount(2);

    TransactionRow positiveBankRow = positiveTransaction.TransactionRows.Single(r => r.AccountId == account.Entity.Id);
    positiveBankRow.RowCounter.Should().Be(1);
    positiveBankRow.Debit.Should().Be(100);
    positiveBankRow.Credit.Should().BeNull();

    TransactionRow positiveOffsetRow = positiveTransaction.TransactionRows.Single(r => r.AccountId == null);
    positiveOffsetRow.RowCounter.Should().Be(2);
    positiveOffsetRow.Debit.Should().BeNull();
    positiveOffsetRow.Credit.Should().Be(100);

    // negative amount (outgoing 50): the bank-side row is credited with the absolute amount
    Transaction negativeTransaction = bankStatement.Transactions.Single(t => t.TransactionRows.Any(r => r.Credit == 50));
    negativeTransaction.Status.Should().Be(TransactionStatus.PendingImportReview);
    negativeTransaction.TransactionRows.Should().HaveCount(2);

    TransactionRow negativeBankRow = negativeTransaction.TransactionRows.Single(r => r.AccountId == account.Entity.Id);
    negativeBankRow.RowCounter.Should().Be(2);
    negativeBankRow.Credit.Should().Be(50);
    negativeBankRow.Debit.Should().BeNull();

    TransactionRow negativeOffsetRow = negativeTransaction.TransactionRows.Single(r => r.AccountId == null);
    negativeOffsetRow.RowCounter.Should().Be(1);
    negativeOffsetRow.Debit.Should().Be(50);
    negativeOffsetRow.Credit.Should().BeNull();
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

  [Test]
  public async Task Handle_ShouldFlagDuplicate_WhenAnAmountMatchFallsWithinTheDateWindow()
  {
    // arrange: an existing transaction two days before the parsed item, same amount
    Account account = await SeedAccountAsync();
    DateOnly today = DateOnly.FromDateTime(DateTime.Now);

    Transaction existing = new()
    {
      Date = today.AddDays(-2),
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow> { new() { AccountId = account.Id, Debit = 100, RowCounter = 1 } },
    };
    await _dbContext.Transactions.AddAsync(existing);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    Guid parserId = SetupParser(new BankStatementItem
    {
      Date = today, Amount = 100, RawInput = "Within window", CounterpartyName = "Nobody",
    });

    // act
    BankStatementsParseResponse response = await _sut.Handle(RequestFor(parserId, account.Id), CancellationToken.None);

    // assert
    Transaction imported = await _dbContext.Transactions.FirstAsync(t => t.BankStatementId == response.BankStatementId);
    imported.Status.Should().Be(TransactionStatus.PotentialDuplicate);
    imported.PotentialDuplicateOfTransactionId.Should().Be(existing.Id);
  }

  [Test]
  public async Task Handle_ShouldNotFlagDuplicate_WhenTheOnlyAmountMatchIsOutsideTheDateWindow()
  {
    // arrange: an existing transaction four days before the parsed item (outside the +/-3 day window)
    Account account = await SeedAccountAsync();
    DateOnly today = DateOnly.FromDateTime(DateTime.Now);

    Transaction existing = new()
    {
      Date = today.AddDays(-4),
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow> { new() { AccountId = account.Id, Debit = 100, RowCounter = 1 } },
    };
    await _dbContext.Transactions.AddAsync(existing);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    Guid parserId = SetupParser(new BankStatementItem
    {
      Date = today, Amount = 100, RawInput = "Outside window", CounterpartyName = "Nobody",
    });

    // act
    BankStatementsParseResponse response = await _sut.Handle(RequestFor(parserId, account.Id), CancellationToken.None);

    // assert
    Transaction imported = await _dbContext.Transactions.FirstAsync(t => t.BankStatementId == response.BankStatementId);
    imported.Status.Should().Be(TransactionStatus.PendingImportReview);
    imported.PotentialDuplicateOfTransactionId.Should().BeNull();
  }

  [Test]
  public async Task Handle_ShouldFlagDuplicate_WhenOnlyTheCounterpartyMatches_WithinTheDateWindow()
  {
    // arrange: existing transaction with a different amount but the same counterparty, same date
    Account account = await SeedAccountAsync();
    DateOnly today = DateOnly.FromDateTime(DateTime.Now);

    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Acme" });
    Transaction existing = new()
    {
      Date = today,
      CounterpartyId = counterparty.Entity.Id,
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow> { new() { AccountId = account.Id, Debit = 999, RowCounter = 1 } },
    };
    await _dbContext.Transactions.AddAsync(existing);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    Guid parserId = SetupParser(new BankStatementItem
    {
      Date = today, Amount = 100, RawInput = "Counterparty match", CounterpartyName = "Acme",
    });

    // act
    BankStatementsParseResponse response = await _sut.Handle(RequestFor(parserId, account.Id), CancellationToken.None);

    // assert
    Transaction imported = await _dbContext.Transactions.FirstAsync(t => t.BankStatementId == response.BankStatementId && t.Status == TransactionStatus.PotentialDuplicate);
    imported.PotentialDuplicateOfTransactionId.Should().Be(existing.Id);
  }

  [Test]
  public async Task Handle_ShouldPersistAccountingDate_SeparateFromTheCurrencyDate()
  {
    // arrange
    Account account = await SeedAccountAsync();
    Guid parserId = SetupParser(new BankStatementItem
    {
      Date = new DateOnly(2025, 10, 30),
      AccountingDate = new DateOnly(2025, 11, 1),
      Amount = 100,
      RawInput = "Raw line",
      CounterpartyName = "Nobody",
    });

    // act
    BankStatementsParseResponse response = await _sut.Handle(RequestFor(parserId, account.Id), CancellationToken.None);

    // assert
    Transaction imported = await _dbContext.Transactions.FirstAsync(t => t.BankStatementId == response.BankStatementId);
    imported.Date.Should().Be(new DateOnly(2025, 10, 30));
    imported.AccountingDate.Should().Be(new DateOnly(2025, 11, 1));
  }

  [Test]
  public async Task Handle_ShouldPersistRawImportData_FromTheSpecificParsedItem()
  {
    // arrange
    Account account = await SeedAccountAsync();
    DateOnly today = DateOnly.FromDateTime(DateTime.Now);
    WithdrawalItem specificItem = new(today, today, "Bancomat withdrawal", -100);

    Guid parserId = SetupParser(new BankStatementItem
    {
      Date = today, Amount = -100, RawInput = "Raw line", CounterpartyName = "Nobody", SpecificParsedItem = specificItem,
    });

    // act
    BankStatementsParseResponse response = await _sut.Handle(RequestFor(parserId, account.Id), CancellationToken.None);

    // assert
    Transaction imported = await _dbContext.Transactions.FirstAsync(t => t.BankStatementId == response.BankStatementId);
    imported.RawImportData.Should().NotBeNullOrEmpty();
    imported.RawImportData.Should().Contain("Bancomat withdrawal");
  }
}
