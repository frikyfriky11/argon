using Argon.Application.Statistics.Cashflow;

namespace Argon.Application.Tests.Statistics.Cashflow;

public class StatisticsCashflowHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new StatisticsCashflowHandler(_dbContext);
  }

  private StatisticsCashflowHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldReturnMonthlyIncomeAndExpense_OrderedChronologically()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> salary = await _dbContext.Accounts.AddAsync(new Account { Name = "Stipendio", Type = AccountType.Revenue });
    EntityEntry<Account> groceries = await _dbContext.Accounts.AddAsync(new Account { Name = "Alimentari", Type = AccountType.Expense });
    await _dbContext.Transactions.AddRangeAsync(
      Income(cash.Entity, salary.Entity, new DateOnly(2025, 1, 27), 2000m),
      Expense(cash.Entity, groceries.Entity, new DateOnly(2025, 1, 10), 300m),
      Expense(cash.Entity, groceries.Entity, new DateOnly(2025, 2, 10), 250m));
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<StatisticsCashflowResponse> result =
      await _sut.Handle(new StatisticsCashflowRequest(null, null), CancellationToken.None);

    // assert
    result.Should().HaveCount(2);
    result[0].Should().BeEquivalentTo(new { Year = 2025, Month = 1, Income = 2000m, Expense = 300m });
    result[1].Should().BeEquivalentTo(new { Year = 2025, Month = 2, Income = 0m, Expense = 250m });
  }

  [Test]
  public async Task Handle_ShouldExcludeTransfersBetweenCashAccounts()
  {
    // arrange
    EntityEntry<Account> cash = await _dbContext.Accounts.AddAsync(new Account { Name = "Sparkasse", Type = AccountType.Cash });
    EntityEntry<Account> wallet = await _dbContext.Accounts.AddAsync(new Account { Name = "Portafoglio", Type = AccountType.Cash });
    await _dbContext.Transactions.AddAsync(new Transaction
    {
      Date = new DateOnly(2025, 3, 1),
      AccountingDate = new DateOnly(2025, 3, 1),
      Status = TransactionStatus.Confirmed,
      TransactionRows = new List<TransactionRow>
      {
        new() { RowCounter = 1, Account = cash.Entity, Credit = 100m },
        new() { RowCounter = 2, Account = wallet.Entity, Debit = 100m },
      },
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    List<StatisticsCashflowResponse> result =
      await _sut.Handle(new StatisticsCashflowRequest(null, null), CancellationToken.None);

    // assert
    result.Should().BeEmpty();
  }

  private static Transaction Income(Account cash, Account revenue, DateOnly date, decimal amount) => new()
  {
    Date = date,
    AccountingDate = date,
    Status = TransactionStatus.Confirmed,
    TransactionRows = new List<TransactionRow>
    {
      new() { RowCounter = 1, Account = cash, Debit = amount },
      new() { RowCounter = 2, Account = revenue, Credit = amount },
    },
  };

  private static Transaction Expense(Account cash, Account expense, DateOnly date, decimal amount) => new()
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
}
