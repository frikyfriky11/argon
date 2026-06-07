using Argon.Application.Statistics.NetWorth;

namespace Argon.Application.Tests.Statistics.NetWorth;

public class StatisticsNetWorthHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new StatisticsNetWorthHandler(_dbContext);
  }

  private StatisticsNetWorthHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldReturnAssetsLessLiabilities_AcrossBalanceSheetAccounts()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> house = await _dbContext.Accounts.AddAsync(new Account { Name = "Immobile", Type = AccountType.Asset });
    EntityEntry<Account> loan = await _dbContext.Accounts.AddAsync(new Account { Name = "Mutuo", Type = AccountType.Liability });
    EntityEntry<Account> owed = await _dbContext.Accounts.AddAsync(new Account { Name = "Crediti vs Luca", Type = AccountType.Receivable });
    await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2025, 1, 15),
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow>
      {
        new() { RowCounter = 1, Account = cash.Entity, Debit = 100m },
        new() { RowCounter = 2, Account = house.Entity, Debit = 600m },
        new() { RowCounter = 3, Account = owed.Entity, Debit = 30m },
        new() { RowCounter = 4, Account = loan.Entity, Credit = 480m },
      },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    StatisticsNetWorthResponse result =
      await _sut.Handle(new StatisticsNetWorthRequest(), CancellationToken.None);

    // assert
    // 100 cash + 600 house + 30 receivable − 480 liability
    result.Total.Should().Be(250m);
  }

  [Test]
  public async Task Handle_ShouldExcludeExpenseRevenueAndEquityAccounts()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> expense = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    EntityEntry<Account> equity = await _dbContext.Accounts.AddAsync(new Account { Name = "Capitale iniziale", Type = AccountType.Equity });
    await _dbContext.Transactions.AddRangeAsync(
      new Transaction
      {
        Date = new DateOnly(2025, 1, 1),
        Status = TransactionStatus.Confirmed,
        TransactionRows = new List<TransactionRow>
        {
          new() { RowCounter = 1, Account = cash.Entity, Debit = 1000m },
          new() { RowCounter = 2, Account = equity.Entity, Credit = 1000m },
        },
      },
      new Transaction
      {
        Date = new DateOnly(2025, 1, 10),
        Status = TransactionStatus.Confirmed,
        TransactionRows = new List<TransactionRow>
        {
          new() { RowCounter = 1, Account = cash.Entity, Credit = 40m },
          new() { RowCounter = 2, Account = expense.Entity, Debit = 40m },
        },
      });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    StatisticsNetWorthResponse result =
      await _sut.Handle(new StatisticsNetWorthRequest(), CancellationToken.None);

    // assert
    // only the cash leg (1000 − 40) counts; equity and expense legs are excluded
    result.Total.Should().Be(960m);
  }

  [Test]
  public async Task Handle_ShouldReturnZero_WhenThereAreNoBalanceSheetMovements()
  {
    // arrange
    // act
    StatisticsNetWorthResponse result =
      await _sut.Handle(new StatisticsNetWorthRequest(), CancellationToken.None);

    // assert
    result.Total.Should().Be(0m);
  }
}
