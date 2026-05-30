using Argon.Application.Accounts.Update;
using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.Accounts.Update;

public class AccountsUpdateValidatorTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsUpdateValidator(_dbContext);
  }

  private AccountsUpdateValidator _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenObjectIsValid()
  {
    AccountsUpdateRequest request = new("new test account", AccountType.Revenue);

    TestValidationResult<AccountsUpdateRequest>? result = await _sut.TestValidateAsync(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameIsTooLong()
  {
    AccountsUpdateRequest request = new("x".Repeat(51), AccountType.Revenue);

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameIsEmpty()
  {
    AccountsUpdateRequest request = new(string.Empty, AccountType.Revenue);

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenTypeIsNotInEnum()
  {
    AccountsUpdateRequest request = new("new test account", (AccountType)999);

    await _sut.ShouldFailOnProperty(request, nameof(request.Type));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameAlreadyBelongsToAnotherAccount()
  {
    // arrange
    EntityEntry<Account> other = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries", Type = AccountType.Expense });
    EntityEntry<Account> edited = await _dbContext.Accounts.AddAsync(new Account { Name = "Restaurants", Type = AccountType.Expense });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsUpdateRequest request = new("groceries", AccountType.Expense) { Id = edited.Entity.Id };

    // act + assert
    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldNotReturnError_WhenAccountKeepsItsOwnName()
  {
    // arrange
    EntityEntry<Account> edited = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries", Type = AccountType.Expense });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsUpdateRequest request = new("Groceries", AccountType.Expense) { Id = edited.Entity.Id };

    // act + assert
    await _sut.ShouldNotFailOnProperty(request, nameof(request.Name));
  }
}