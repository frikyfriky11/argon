using Argon.Application.Accounts.GetList;

namespace Argon.Application.Tests.Accounts.GetList;

[TestFixture]
public class AccountsGetListHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsGetListHandler(_dbContext);
  }

  private IApplicationDbContext _dbContext = null!;
  private AccountsGetListHandler _sut = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithValidRequest()
  {
    // arrange
    List<Account> accounts = new()
    {
      new Account { Name = "test account 1", Type = AccountType.Cash },
      new Account { Name = "test account 2", Type = AccountType.Credit },
      new Account { Name = "test account 3", Type = AccountType.Debit },
    };

    await _dbContext.Accounts.AddRangeAsync(accounts);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsGetListRequest request = new(null, null);

    List<AccountsGetListResponse> expected = accounts.Select(account => new AccountsGetListResponse(
        account.Id,
        account.Name,
        account.Type,
        account.IsFavourite,
        0
      ))
      .OrderBy(x => x.Name)
      .ToList();

    // act
    List<AccountsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Should().BeEquivalentTo(expected);
  }

  [Test]
  public async Task Handle_ShouldReturnCorrectTotalAmount_WithValidRequest()
  {
    // arrange
    Account groceriesAccount = new() { Name = "Groceries", Type = AccountType.Expense };
    Account cashAccount = new() { Name = "Cash", Type = AccountType.Cash };
    Account salaryAccount = new() { Name = "Salary", Type = AccountType.Revenue };

    List<Transaction> transactions = new()
    {
      new Transaction
      {
        Date = new DateOnly(2023, 9, 12),
        Description = "Groceries market",
        TransactionRows = new List<TransactionRow>
        {
          new() { Account = groceriesAccount, Debit = 100 },
          new() { Account = cashAccount, Credit = 100 },
        },
      },
      new Transaction
      {
        Date = new DateOnly(2023, 9, 7),
        Description = "Salary from job 1",
        TransactionRows = new List<TransactionRow>
        {
          new() { Account = cashAccount, Debit = 1000 },
          new() { Account = salaryAccount, Credit = 1000 },
        },
      },
      new Transaction
      {
        Date = new DateOnly(2023, 10, 7),
        Description = "Salary from job 2",
        TransactionRows = new List<TransactionRow>
        {
          new() { Account = cashAccount, Debit = 500 },
          new() { Account = salaryAccount, Credit = 500 },
        },
      },
    };

    await _dbContext.Transactions.AddRangeAsync(transactions);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsGetListRequest request = new(new DateTimeOffset(new DateTime(2023, 9, 1)), new DateTimeOffset(new DateTime(2023, 9, 30)));

    // act
    List<AccountsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Should().HaveCount(3);
    result.Should().ContainEquivalentOf(new AccountsGetListResponse(Guid.Empty, "Groceries", AccountType.Expense, false, 100), options => options.Excluding(x => x.Id));
    result.Should().ContainEquivalentOf(new AccountsGetListResponse(Guid.Empty, "Cash", AccountType.Cash, false, 900), options => options.Excluding(x => x.Id));
    result.Should().ContainEquivalentOf(new AccountsGetListResponse(Guid.Empty, "Salary", AccountType.Revenue, false, -1000), options => options.Excluding(x => x.Id));
  }
}
