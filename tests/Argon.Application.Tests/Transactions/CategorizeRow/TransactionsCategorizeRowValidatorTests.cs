using Argon.Application.Tests.Extensions;
using Argon.Application.Transactions.CategorizeRow;

namespace Argon.Application.Tests.Transactions.CategorizeRow;

public class TransactionsCategorizeRowValidatorTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new TransactionsCategorizeRowValidator(_dbContext);
  }

  private IApplicationDbContext _dbContext = null!;
  private TransactionsCategorizeRowValidator _sut = null!;

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenAccountExists()
  {
    // arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account { Name = "Groceries" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsCategorizeRowRequest request = new(account.Entity.Id);

    // act
    TestValidationResult<TransactionsCategorizeRowRequest> result = await _sut.TestValidateAsync(request);

    // assert
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenAccountDoesNotExist()
  {
    // arrange
    TransactionsCategorizeRowRequest request = new(Guid.NewGuid());

    // act + assert
    await _sut.ShouldFailOnProperty(request, nameof(request.AccountId));
  }
}
