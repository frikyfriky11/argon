using Argon.Application.Statistics.TopCounterparties;

namespace Argon.Application.Tests.Statistics.TopCounterparties;

public class StatisticsTopCounterpartiesHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new StatisticsTopCounterpartiesHandler(_dbContext);
  }

  private StatisticsTopCounterpartiesHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldRankCounterpartiesBySpendDescending()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> groceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    EntityEntry<Counterparty> amazon = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Amazon" });
    EntityEntry<Counterparty> eurospar = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Eurospar" });
    await _dbContext.Transactions.AddRangeAsync(
      Spend(cash.Entity, groceries.Entity, amazon.Entity, new DateOnly(2025, 1, 1), 80m),
      Spend(cash.Entity, groceries.Entity, eurospar.Entity, new DateOnly(2025, 1, 2), 30m),
      Spend(cash.Entity, groceries.Entity, eurospar.Entity, new DateOnly(2025, 1, 3), 20m));
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<StatisticsTopCounterpartiesResponse> result =
      await _sut.Handle(new StatisticsTopCounterpartiesRequest(null, null), CancellationToken.None);

    // assert: Amazon 80, Eurospar 50
    result.Should().HaveCount(2);
    result[0].Should().BeEquivalentTo(new { CounterpartyName = "Amazon", Total = 80m });
    result[1].Should().BeEquivalentTo(new { CounterpartyName = "Eurospar", Total = 50m });
  }

  [Test]
  public async Task Handle_ShouldAggregateUnlinkedTransactionsUnderASingleBucket()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> groceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    await _dbContext.Transactions.AddRangeAsync(
      Spend(cash.Entity, groceries.Entity, null, new DateOnly(2025, 1, 1), 10m),
      Spend(cash.Entity, groceries.Entity, null, new DateOnly(2025, 1, 2), 15m));
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<StatisticsTopCounterpartiesResponse> result =
      await _sut.Handle(new StatisticsTopCounterpartiesRequest(null, null), CancellationToken.None);

    // assert
    result.Should().ContainSingle();
    result[0].CounterpartyId.Should().BeNull();
    result[0].Total.Should().Be(25m);
  }

  [Test]
  public async Task Handle_ShouldRespectTheTakeLimit()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> groceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    EntityEntry<Counterparty> a = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "A" });
    EntityEntry<Counterparty> b = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "B" });
    EntityEntry<Counterparty> c = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "C" });
    await _dbContext.Transactions.AddRangeAsync(
      Spend(cash.Entity, groceries.Entity, a.Entity, new DateOnly(2025, 1, 1), 30m),
      Spend(cash.Entity, groceries.Entity, b.Entity, new DateOnly(2025, 1, 1), 20m),
      Spend(cash.Entity, groceries.Entity, c.Entity, new DateOnly(2025, 1, 1), 10m));
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<StatisticsTopCounterpartiesResponse> result =
      await _sut.Handle(new StatisticsTopCounterpartiesRequest(null, null, Take: 2), CancellationToken.None);

    // assert
    result.Should().HaveCount(2);
    result.Select(r => r.CounterpartyName).Should().ContainInOrder("A", "B");
  }

  private static Transaction Spend(Account cash, Account expense, Counterparty? counterparty, DateOnly date, decimal amount) => new()
  {
    Date = date,
    AccountingDate = date,
    Counterparty = counterparty,
    Status = TransactionStatus.Confirmed,
    TransactionRows = new List<TransactionRow>
    {
      new() { RowCounter = 1, Account = cash, Credit = amount },
      new() { RowCounter = 2, Account = expense, Debit = amount },
    },
  };
}
