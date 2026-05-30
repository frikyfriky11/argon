using Argon.Application.BudgetItems.GetList;

namespace Argon.Application.Tests.BudgetItems.GetList;

public class BudgetItemsGetListHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new BudgetItemsGetListHandler(_dbContext);
  }

  private IApplicationDbContext _dbContext = null!;
  private BudgetItemsGetListHandler _sut = null!;

  [Test]
  public async Task Handle_ShouldReturnOnlyItemsMatchingYearAndMonth_AndProjectAccountType()
  {
    // arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries", Type = AccountType.Expense });
    await _dbContext.BudgetItems.AddRangeAsync(
      new BudgetItem { AccountId = account.Entity.Id, Year = 2026, Month = 5, Amount = 100.00m },
      new BudgetItem { AccountId = account.Entity.Id, Year = 2026, Month = 6, Amount = 200.00m }, // wrong month
      new BudgetItem { AccountId = account.Entity.Id, Year = 2025, Month = 5, Amount = 300.00m }); // wrong year
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    BudgetItemsGetListRequest request = new(2026, 5);

    // act
    List<BudgetItemsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Should().ContainSingle();
    result[0].Year.Should().Be(2026);
    result[0].Month.Should().Be(5);
    result[0].Amount.Should().Be(100.00m);
    result[0].AccountId.Should().Be(account.Entity.Id);
    result[0].AccountType.Should().Be(AccountType.Expense);
  }

  [Test]
  public async Task Handle_ShouldReturnEmpty_WhenNothingMatchesThePeriod()
  {
    // arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries", Type = AccountType.Expense });
    await _dbContext.BudgetItems.AddAsync(new BudgetItem { AccountId = account.Entity.Id, Year = 2026, Month = 5, Amount = 100.00m });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    BudgetItemsGetListRequest request = new(2026, 7);

    // act
    List<BudgetItemsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Should().BeEmpty();
  }
}
