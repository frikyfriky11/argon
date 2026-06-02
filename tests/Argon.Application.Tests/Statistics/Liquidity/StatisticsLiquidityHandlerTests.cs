using Argon.Application.Statistics.Liquidity;

namespace Argon.Application.Tests.Statistics.Liquidity;

public class StatisticsLiquidityHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new StatisticsLiquidityHandler(_dbContext);
  }

  private StatisticsLiquidityHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldAccumulateCashMovementsIntoARunningMonthlyBalance()
  {
    // arrange
    await SeedThreeMonthsOfMovements();

    // act
    List<StatisticsLiquidityResponse> result =
      await _sut.Handle(new StatisticsLiquidityRequest(null, null), CancellationToken.None);

    // assert
    result.Should().HaveCount(3);
    result[0].Should().BeEquivalentTo(new { Year = 2025, Month = 1, Balance = 100m });
    result[1].Should().BeEquivalentTo(new { Year = 2025, Month = 2, Balance = 70m });
    result[2].Should().BeEquivalentTo(new { Year = 2025, Month = 3, Balance = 120m });
  }

  [Test]
  public async Task Handle_ShouldFallBackToValueDate_WhenAccountingDateIsNull()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2025, 5, 10),
      AccountingDate = null,
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow> { new() { RowCounter = 1, Account = cash.Entity, Debit = 200m } },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<StatisticsLiquidityResponse> result =
      await _sut.Handle(new StatisticsLiquidityRequest(null, null), CancellationToken.None);

    // assert
    result.Should().ContainSingle();
    result[0].Should().BeEquivalentTo(new { Year = 2025, Month = 5, Balance = 200m });
  }

  [Test]
  public async Task Handle_ShouldSliceToTheWindowButKeepAccumulatingFromTheStart()
  {
    // arrange
    await SeedThreeMonthsOfMovements();

    // act
    List<StatisticsLiquidityResponse> result = await _sut.Handle(
      new StatisticsLiquidityRequest(new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero), null),
      CancellationToken.None);

    // assert
    result.Should().HaveCount(2);
    result[0].Should().BeEquivalentTo(new { Year = 2025, Month = 2, Balance = 70m });
    result[1].Should().BeEquivalentTo(new { Year = 2025, Month = 3, Balance = 120m });
  }

  [Test]
  public async Task Handle_ShouldIgnoreNonCashAccounts()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> expense = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2025, 1, 15),
      AccountingDate = new DateOnly(2025, 1, 15),
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow>
      {
        new() { RowCounter = 1, Account = cash.Entity, Credit = 40m },
        new() { RowCounter = 2, Account = expense.Entity, Debit = 40m },
      },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<StatisticsLiquidityResponse> result =
      await _sut.Handle(new StatisticsLiquidityRequest(null, null), CancellationToken.None);

    // assert
    result.Should().ContainSingle();
    result[0].Balance.Should().Be(-40m);
  }

  private async Task SeedThreeMonthsOfMovements()
  {
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    await _dbContext.Transactions.AddRangeAsync(
      new Transaction
      {
        Date = new DateOnly(2025, 1, 5),
        AccountingDate = new DateOnly(2025, 1, 5),
        Status = TransactionStatus.Confirmed,
        TransactionRows = new List<TransactionRow> { new() { RowCounter = 1, Account = cash.Entity, Debit = 100m } },
      },
      new Transaction
      {
        Date = new DateOnly(2025, 2, 5),
        AccountingDate = new DateOnly(2025, 2, 5),
        Status = TransactionStatus.Confirmed,
        TransactionRows = new List<TransactionRow> { new() { RowCounter = 1, Account = cash.Entity, Credit = 30m } },
      },
      new Transaction
      {
        Date = new DateOnly(2025, 3, 5),
        AccountingDate = new DateOnly(2025, 3, 5),
        Status = TransactionStatus.Confirmed,
        TransactionRows = new List<TransactionRow> { new() { RowCounter = 1, Account = cash.Entity, Debit = 50m } },
      });
    await _dbContext.SaveChangesAsync(CancellationToken.None);
  }
}
