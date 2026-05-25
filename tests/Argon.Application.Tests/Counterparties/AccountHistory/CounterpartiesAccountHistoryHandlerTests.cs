using Argon.Application.Counterparties.AccountHistory;

namespace Argon.Application.Tests.Counterparties.AccountHistory;

public class CounterpartiesAccountHistoryHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new CounterpartiesAccountHistoryHandler(_dbContext);
  }

  private CounterpartiesAccountHistoryHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldReturnFrequencyTable_OrderedByDescendingCount()
  {
    // arrange
    EntityEntry<Account> bank = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> groceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    EntityEntry<Account> cleaning = await _dbContext.Accounts.AddAsync(new Account { Name = "Prodotti pulizia", Type = AccountType.Expense });
    EntityEntry<Counterparty> eurospar = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Eurospar" });
    EntityEntry<Counterparty> other = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Other" });

    for (int i = 0; i < 3; i++)
    {
      await _dbContext.Transactions.AddAsync(new Transaction
      {
        Date = new DateOnly(2024, 1, 1).AddDays(i),
        Counterparty = eurospar.Entity,
        Status = TransactionStatus.Confirmed,
        TransactionRows = new List<TransactionRow>
        {
          new() { RowCounter = 1, Account = bank.Entity, Credit = 30m },
          new() { RowCounter = 2, Account = groceries.Entity, Debit = 30m },
        },
      });
    }

    await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 10),
      Counterparty = eurospar.Entity,
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow>
      {
        new() { RowCounter = 1, Account = bank.Entity, Credit = 10m },
        new() { RowCounter = 2, Account = cleaning.Entity, Debit = 10m },
      },
    });

    await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 11),
      Counterparty = other.Entity,
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow>
      {
        new() { RowCounter = 1, Account = bank.Entity, Credit = 5m },
        new() { RowCounter = 2, Account = groceries.Entity, Debit = 5m },
      },
    });

    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<CounterpartiesAccountHistoryResponse> result =
      await _sut.Handle(new CounterpartiesAccountHistoryRequest(eurospar.Entity.Id), CancellationToken.None);

    // assert
    result.Should().HaveCount(3);
    result[0].AccountName.Should().Be("Sparkasse");
    result[0].Count.Should().Be(4);
    result[1].AccountName.Should().Be("Alimentari");
    result[1].Count.Should().Be(3);
    result[2].AccountName.Should().Be("Prodotti pulizia");
    result[2].Count.Should().Be(1);
  }

  [Test]
  public async Task Handle_ShouldIgnoreRowsWithoutAccount()
  {
    EntityEntry<Account> bank = await _dbContext.Accounts.AddAsync(new Account { Name = "Bank", Type = AccountType.Cash });
    EntityEntry<Counterparty> cp = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Pending" });

    await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2024, 1, 1),
      Counterparty = cp.Entity,
      Status = TransactionStatus.PendingImportReview,
      TransactionRows = new List<TransactionRow>
      {
        new() { RowCounter = 1, Account = bank.Entity, Credit = 20m },
        new() { RowCounter = 2, AccountId = null, Account = null, Debit = 20m },
      },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<CounterpartiesAccountHistoryResponse> result =
      await _sut.Handle(new CounterpartiesAccountHistoryRequest(cp.Entity.Id), CancellationToken.None);

    result.Should().HaveCount(1);
    result[0].AccountName.Should().Be("Bank");
  }

  [Test]
  public async Task Handle_ShouldThrowNotFound_WhenCounterpartyDoesNotExist()
  {
    Func<Task> act = () => _sut.Handle(new CounterpartiesAccountHistoryRequest(Guid.NewGuid()), CancellationToken.None);

    await act.Should().ThrowAsync<NotFoundException>();
  }

  [Test]
  public async Task Handle_ShouldReturnEmptyList_WhenCounterpartyHasNoTransactions()
  {
    EntityEntry<Counterparty> cp = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "New" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<CounterpartiesAccountHistoryResponse> result =
      await _sut.Handle(new CounterpartiesAccountHistoryRequest(cp.Entity.Id), CancellationToken.None);

    result.Should().BeEmpty();
  }
}
