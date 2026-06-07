using Argon.Application.BudgetItems.Upsert;

namespace Argon.Application.Tests.BudgetItems.Upsert;

public class BudgetItemsUpsertHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new BudgetItemsUpsertHandler(_dbContext);
  }

  private IApplicationDbContext _dbContext = null!;
  private BudgetItemsUpsertHandler _sut = null!;

  [Test]
  public async Task Handle_ShouldInsertBudgetItem_WhenNoneExistsForThePeriod()
  {
    // arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries", Type = AccountType.Expense });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    BudgetItemsUpsertRequest request = new(account.Entity.Id, 2026, 5, 250.00m);

    // act
    BudgetItemsUpsertResponse response = await _sut.Handle(request, CancellationToken.None);

    // assert
    response.Id.Should().NotBeNull();
    BudgetItem? persisted = await _dbContext.BudgetItems.FirstOrDefaultAsync(b => b.Id == response.Id);
    persisted.Should().NotBeNull();
    persisted!.AccountId.Should().Be(account.Entity.Id);
    persisted.Year.Should().Be(2026);
    persisted.Month.Should().Be(5);
    persisted.Amount.Should().Be(250.00m);
  }

  [Test]
  public async Task Handle_ShouldUpdateExistingBudgetItemAndKeepItsId_WhenOneExistsForThePeriod()
  {
    // arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries", Type = AccountType.Expense });
    BudgetItem existing = new() { AccountId = account.Entity.Id, Year = 2026, Month = 5, Amount = 100.00m };
    await _dbContext.BudgetItems.AddAsync(existing);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    BudgetItemsUpsertRequest request = new(account.Entity.Id, 2026, 5, 300.00m);

    // act
    BudgetItemsUpsertResponse response = await _sut.Handle(request, CancellationToken.None);

    // assert
    response.Id.Should().Be(existing.Id);
    (await _dbContext.BudgetItems.CountAsync()).Should().Be(1);
    BudgetItem? persisted = await _dbContext.BudgetItems.FirstOrDefaultAsync(b => b.Id == existing.Id);
    persisted!.Amount.Should().Be(300.00m);
  }

  [Test]
  public async Task Handle_ShouldDeleteBudgetItem_WhenAmountIsNullAndItExists()
  {
    // arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries", Type = AccountType.Expense });
    BudgetItem existing = new() { AccountId = account.Entity.Id, Year = 2026, Month = 5, Amount = 100.00m };
    await _dbContext.BudgetItems.AddAsync(existing);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    BudgetItemsUpsertRequest request = new(account.Entity.Id, 2026, 5, null);

    // act
    BudgetItemsUpsertResponse response = await _sut.Handle(request, CancellationToken.None);

    // assert
    response.Id.Should().BeNull();
    (await _dbContext.BudgetItems.AnyAsync(b => b.Id == existing.Id)).Should().BeFalse();
  }

  [Test]
  public async Task Handle_ShouldReturnNullIdWithoutPersisting_WhenAmountIsNullAndNoneExists()
  {
    // arrange
    BudgetItemsUpsertRequest request = new(Guid.NewGuid(), 2026, 5, null);

    // act
    BudgetItemsUpsertResponse response = await _sut.Handle(request, CancellationToken.None);

    // assert
    response.Id.Should().BeNull();
    (await _dbContext.BudgetItems.AnyAsync()).Should().BeFalse();
  }
}
