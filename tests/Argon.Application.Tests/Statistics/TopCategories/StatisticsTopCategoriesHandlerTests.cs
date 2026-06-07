using Argon.Application.Statistics.TopCategories;

namespace Argon.Application.Tests.Statistics.TopCategories;

public class StatisticsTopCategoriesHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new StatisticsTopCategoriesHandler(_dbContext);
  }

  private StatisticsTopCategoriesHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldRankExpenseAccountsByTotal_WithCumulativePercentage()
  {
    // arrange
    await SeedCategories();

    // act
    List<StatisticsTopCategoriesResponse> result =
      await _sut.Handle(new StatisticsTopCategoriesRequest(null, null), CancellationToken.None);

    // assert: groceries 60, rent 30, fun 10 → grand total 100
    result.Should().HaveCount(3);
    result[0].Should().BeEquivalentTo(new { AccountName = "Alimentari", Total = 60m, CumulativePercentage = 60m });
    result[1].Should().BeEquivalentTo(new { AccountName = "Affitto", Total = 30m, CumulativePercentage = 90m });
    result[2].Should().BeEquivalentTo(new { AccountName = "Svago", Total = 10m, CumulativePercentage = 100m });
  }

  [Test]
  public async Task Handle_ShouldLimitToTake_ButComputePercentageAgainstTheFullTotal()
  {
    // arrange
    await SeedCategories();

    // act
    List<StatisticsTopCategoriesResponse> result =
      await _sut.Handle(new StatisticsTopCategoriesRequest(null, null, Take: 2), CancellationToken.None);

    // assert
    result.Should().HaveCount(2);
    result[0].CumulativePercentage.Should().Be(60m);
    result[1].CumulativePercentage.Should().Be(90m);
  }

  [Test]
  public async Task Handle_ShouldOnlyCountTransactionsInsideTheWindow()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> groceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    await _dbContext.Transactions.AddRangeAsync(
      Spend(cash.Entity, groceries.Entity, new DateOnly(2024, 12, 31), 99m),
      Spend(cash.Entity, groceries.Entity, new DateOnly(2025, 6, 15), 25m));
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<StatisticsTopCategoriesResponse> result = await _sut.Handle(
      new StatisticsTopCategoriesRequest(
        new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
        new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero)),
      CancellationToken.None);

    // assert
    result.Should().ContainSingle();
    result[0].Total.Should().Be(25m);
  }

  [Test]
  public async Task Handle_ShouldExcludeCategoriesThatNetToZeroOrRefunded()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> refunded = await _dbContext.Accounts.AddAsync(new Account { Name = "Resi", Type = AccountType.Expense });
    EntityEntry<Account> groceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    await _dbContext.Transactions.AddRangeAsync(
      Spend(cash.Entity, refunded.Entity, new DateOnly(2025, 1, 1), 20m),
      Refund(cash.Entity, refunded.Entity, new DateOnly(2025, 1, 2), 20m),
      Spend(cash.Entity, groceries.Entity, new DateOnly(2025, 1, 3), 15m));
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<StatisticsTopCategoriesResponse> result =
      await _sut.Handle(new StatisticsTopCategoriesRequest(null, null), CancellationToken.None);

    // assert
    result.Should().ContainSingle();
    result[0].AccountName.Should().Be("Alimentari");
  }

  private async Task SeedCategories()
  {
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> groceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    EntityEntry<Account> rent = await _dbContext.Accounts.AddAsync(new Account { Name = "Affitto", Type = AccountType.Expense });
    EntityEntry<Account> fun = await _dbContext.Accounts.AddAsync(new Account { Name = "Svago", Type = AccountType.Expense });
    await _dbContext.Transactions.AddRangeAsync(
      Spend(cash.Entity, groceries.Entity, new DateOnly(2025, 1, 1), 60m),
      Spend(cash.Entity, rent.Entity, new DateOnly(2025, 1, 1), 30m),
      Spend(cash.Entity, fun.Entity, new DateOnly(2025, 1, 1), 10m));
    await _dbContext.SaveChangesAsync(CancellationToken.None);
  }

  private static Transaction Spend(Account cash, Account expense, DateOnly date, decimal amount) => new()
  {
    Date = date,
    AccountingDate = date,
    Status = TransactionStatus.Confirmed,
    TransactionRows = new List<TransactionRow>
    {
      new() { RowCounter = 1, Account = cash, Credit = amount },
      new() { RowCounter = 2, Account = expense, Debit = amount },
    },
  };

  private static Transaction Refund(Account cash, Account expense, DateOnly date, decimal amount) => new()
  {
    Date = date,
    AccountingDate = date,
    Status = TransactionStatus.Confirmed,
    TransactionRows = new List<TransactionRow>
    {
      new() { RowCounter = 1, Account = cash, Debit = amount },
      new() { RowCounter = 2, Account = expense, Credit = amount },
    },
  };
}
