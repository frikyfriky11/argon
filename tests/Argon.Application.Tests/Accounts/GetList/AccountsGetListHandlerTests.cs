using Argon.Application.Accounts.GetList;

namespace Argon.Application.Tests.Accounts.GetList;

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
    Account account1 = new Account { Name = "test account 1", Type = AccountType.Cash };
    Account account2 = new Account { Name = "test account 2", Type = AccountType.Credit };
    Account account3 = new Account { Name = "test account 3", Type = AccountType.Debit };
    List<Account> accounts = new()
    {
      account1,
      account2,
      account3,
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
    result.Should().HaveCount(3);
    
    result[0].Id.Should().Be(account1.Id);
    result[0].Name.Should().Be(account1.Name);
    result[0].Type.Should().Be(account1.Type);
    result[0].IsFavourite.Should().Be(account1.IsFavourite);
    result[0].TotalAmount.Should().Be(0);
    
    result[1].Id.Should().Be(account2.Id);
    result[1].Name.Should().Be(account2.Name);
    result[1].Type.Should().Be(account2.Type);
    result[1].IsFavourite.Should().Be(account2.IsFavourite);
    result[1].TotalAmount.Should().Be(0);
    
    result[2].Id.Should().Be(account3.Id);
    result[2].Name.Should().Be(account3.Name);
    result[2].Type.Should().Be(account3.Type);
    result[2].IsFavourite.Should().Be(account3.IsFavourite);
    result[2].TotalAmount.Should().Be(0);
  }

  [Test]
  public async Task Handle_ShouldReturnCorrectTotalAmount_WithValidRequest()
  {
    // arrange
    Account groceriesAccount = new() { Name = "Groceries", Type = AccountType.Expense };
    Account cashAccount = new() { Name = "Cash", Type = AccountType.Cash };
    Account salaryAccount = new() { Name = "Salary", Type = AccountType.Revenue };
    
    Counterparty marketCounterparty = new() { Name = "Market" };
    Counterparty job1Counterparty = new() { Name = "Job 1" };
    Counterparty job2Counterparty = new() { Name = "Job 2" };

    List<Transaction> transactions = new()
    {
      new Transaction
      {
        Date = new DateOnly(2023, 9, 12),
        Counterparty = marketCounterparty,
        TransactionRows = new List<TransactionRow>
        {
          new() { Account = groceriesAccount, Debit = 100 },
          new() { Account = cashAccount, Credit = 100 },
        },
      },
      new Transaction
      {
        Date = new DateOnly(2023, 9, 7),
        Counterparty = job1Counterparty,
        TransactionRows = new List<TransactionRow>
        {
          new() { Account = cashAccount, Debit = 1000 },
          new() { Account = salaryAccount, Credit = 1000 },
        },
      },
      new Transaction
      {
        Date = new DateOnly(2023, 10, 7),
        Counterparty = job2Counterparty,
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
    
    result[0].Id.Should().Be(cashAccount.Id);
    result[0].Name.Should().Be(cashAccount.Name);
    result[0].Type.Should().Be(cashAccount.Type);
    result[0].IsFavourite.Should().Be(cashAccount.IsFavourite);
    result[0].TotalAmount.Should().Be(900);
    
    result[1].Id.Should().Be(groceriesAccount.Id);
    result[1].Name.Should().Be(groceriesAccount.Name);
    result[1].Type.Should().Be(groceriesAccount.Type);
    result[1].IsFavourite.Should().Be(groceriesAccount.IsFavourite);
    result[1].TotalAmount.Should().Be(100);
    
    result[2].Id.Should().Be(salaryAccount.Id);
    result[2].Name.Should().Be(salaryAccount.Name);
    result[2].Type.Should().Be(salaryAccount.Type);
    result[2].IsFavourite.Should().Be(salaryAccount.IsFavourite);
    result[2].TotalAmount.Should().Be(-1000);
  }
}
