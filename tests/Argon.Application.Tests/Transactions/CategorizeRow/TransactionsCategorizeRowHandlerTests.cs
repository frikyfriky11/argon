using Argon.Application.Transactions.CategorizeRow;

namespace Argon.Application.Tests.Transactions.CategorizeRow;

public class TransactionsCategorizeRowHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new TransactionsCategorizeRowHandler(_dbContext);
  }

  private TransactionsCategorizeRowHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldAssignAccountAndConfirmTransaction_WhenLastMissingRowIsCategorized()
  {
    // arrange
    EntityEntry<Account> bankAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    EntityEntry<Account> groceriesAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Market" });

    TransactionRow filledRow = new() { RowCounter = 1, Account = bankAccount.Entity, Credit = 50m };
    TransactionRow pendingRow = new() { RowCounter = 2, Account = null, AccountId = null, Debit = 50m };

    EntityEntry<Transaction> transaction = await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 1),
      Counterparty = counterparty.Entity,
      Status = TransactionStatus.PendingImportReview,
      PotentialDuplicateOfTransactionId = null,
      TransactionRows = new List<TransactionRow> { filledRow, pendingRow },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsCategorizeRowRequest request = new(groceriesAccount.Entity.Id)
    {
      TransactionId = transaction.Entity.Id,
      RowId = pendingRow.Id,
    };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? saved = await _dbContext.Transactions
      .Include(t => t.TransactionRows)
      .FirstOrDefaultAsync(t => t.Id == transaction.Entity.Id);

    saved.Should().NotBeNull();
    saved!.Status.Should().Be(TransactionStatus.Confirmed);
    saved.TransactionRows.Single(r => r.Id == pendingRow.Id).AccountId.Should().Be(groceriesAccount.Entity.Id);
  }

  [Test]
  public async Task Handle_ShouldKeepStatusPending_WhenAnotherRowStillNeedsAnAccount()
  {
    // arrange
    EntityEntry<Account> bankAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    EntityEntry<Account> groceriesAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Market" });

    TransactionRow filledRow = new() { RowCounter = 1, Account = bankAccount.Entity, Credit = 100m };
    TransactionRow pendingOne = new() { RowCounter = 2, Account = null, AccountId = null, Debit = 60m };
    TransactionRow pendingTwo = new() { RowCounter = 3, Account = null, AccountId = null, Debit = 40m };

    EntityEntry<Transaction> transaction = await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 1),
      Counterparty = counterparty.Entity,
      Status = TransactionStatus.PendingImportReview,
      TransactionRows = new List<TransactionRow> { filledRow, pendingOne, pendingTwo },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsCategorizeRowRequest request = new(groceriesAccount.Entity.Id)
    {
      TransactionId = transaction.Entity.Id,
      RowId = pendingOne.Id,
    };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? saved = await _dbContext.Transactions
      .Include(t => t.TransactionRows)
      .FirstOrDefaultAsync(t => t.Id == transaction.Entity.Id);

    saved.Should().NotBeNull();
    saved!.Status.Should().Be(TransactionStatus.PendingImportReview);
    saved.TransactionRows.Single(r => r.Id == pendingOne.Id).AccountId.Should().Be(groceriesAccount.Entity.Id);
    saved.TransactionRows.Single(r => r.Id == pendingTwo.Id).AccountId.Should().BeNull();
  }

  [Test]
  public async Task Handle_ShouldNotTouchStatus_WhenTransactionIsAlreadyConfirmed()
  {
    // arrange
    EntityEntry<Account> bankAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    EntityEntry<Account> groceriesAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    EntityEntry<Account> restaurantsAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Restaurants" });
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Market" });

    TransactionRow rowOne = new() { RowCounter = 1, Account = bankAccount.Entity, Credit = 30m };
    TransactionRow rowTwo = new() { RowCounter = 2, Account = groceriesAccount.Entity, Debit = 30m };

    EntityEntry<Transaction> transaction = await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 1),
      Counterparty = counterparty.Entity,
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow> { rowOne, rowTwo },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsCategorizeRowRequest request = new(restaurantsAccount.Entity.Id)
    {
      TransactionId = transaction.Entity.Id,
      RowId = rowTwo.Id,
    };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? saved = await _dbContext.Transactions
      .Include(t => t.TransactionRows)
      .FirstOrDefaultAsync(t => t.Id == transaction.Entity.Id);

    saved!.Status.Should().Be(TransactionStatus.Confirmed);
    saved.TransactionRows.Single(r => r.Id == rowTwo.Id).AccountId.Should().Be(restaurantsAccount.Entity.Id);
  }

  [Test]
  public async Task Handle_ShouldSetRowDescription_WhenDescriptionIsSupplied()
  {
    // arrange
    EntityEntry<Account> bankAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    EntityEntry<Account> groceriesAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    TransactionRow filledRow = new() { RowCounter = 1, Account = bankAccount.Entity, Credit = 50m };
    TransactionRow pendingRow = new() { RowCounter = 2, Account = null, AccountId = null, Debit = 50m };
    EntityEntry<Transaction> transaction = await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 1),
      Status = TransactionStatus.PendingImportReview,
      TransactionRows = new List<TransactionRow> { filledRow, pendingRow },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsCategorizeRowRequest request = new(groceriesAccount.Entity.Id, "Sale lavastoviglie")
    {
      TransactionId = transaction.Entity.Id,
      RowId = pendingRow.Id,
    };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? saved = await _dbContext.Transactions
      .Include(t => t.TransactionRows)
      .FirstOrDefaultAsync(t => t.Id == transaction.Entity.Id);
    TransactionRow savedRow = saved!.TransactionRows.Single(r => r.Id == pendingRow.Id);
    savedRow.AccountId.Should().Be(groceriesAccount.Entity.Id);
    savedRow.Description.Should().Be("Sale lavastoviglie");
  }

  [Test]
  public async Task Handle_ShouldLeaveExistingDescriptionUntouched_WhenDescriptionIsNull()
  {
    // arrange
    EntityEntry<Account> bankAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    EntityEntry<Account> groceriesAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    TransactionRow filledRow = new() { RowCounter = 1, Account = bankAccount.Entity, Credit = 50m };
    TransactionRow pendingRow = new() { RowCounter = 2, Account = null, AccountId = null, Debit = 50m, Description = "original" };
    EntityEntry<Transaction> transaction = await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 1),
      Status = TransactionStatus.PendingImportReview,
      TransactionRows = new List<TransactionRow> { filledRow, pendingRow },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsCategorizeRowRequest request = new(groceriesAccount.Entity.Id)
    {
      TransactionId = transaction.Entity.Id,
      RowId = pendingRow.Id,
    };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Transaction? saved = await _dbContext.Transactions
      .Include(t => t.TransactionRows)
      .FirstOrDefaultAsync(t => t.Id == transaction.Entity.Id);
    saved!.TransactionRows.Single(r => r.Id == pendingRow.Id).Description.Should().Be("original");
  }

  [Test]
  public async Task Handle_ShouldThrowNotFound_WhenTransactionDoesNotExist()
  {
    TransactionsCategorizeRowRequest request = new(Guid.NewGuid())
    {
      TransactionId = Guid.NewGuid(),
      RowId = Guid.NewGuid(),
    };

    Func<Task> act = () => _sut.Handle(request, CancellationToken.None);

    await act.Should().ThrowAsync<NotFoundException>();
  }

  [Test]
  public async Task Handle_ShouldThrowNotFound_WhenRowDoesNotExistOnTransaction()
  {
    EntityEntry<Account> bankAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank" });
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Market" });
    TransactionRow row = new() { RowCounter = 1, Account = bankAccount.Entity, Credit = 10m };
    EntityEntry<Transaction> transaction = await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 1),
      Counterparty = counterparty.Entity,
      Status = TransactionStatus.PendingImportReview,
      TransactionRows = new List<TransactionRow> { row },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsCategorizeRowRequest request = new(bankAccount.Entity.Id)
    {
      TransactionId = transaction.Entity.Id,
      RowId = Guid.NewGuid(),
    };

    Func<Task> act = () => _sut.Handle(request, CancellationToken.None);

    await act.Should().ThrowAsync<NotFoundException>();
  }
}
