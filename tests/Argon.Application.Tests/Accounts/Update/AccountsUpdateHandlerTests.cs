using Argon.Application.Accounts.Update;

namespace Argon.Application.Tests.Accounts.Update;

public class AccountsUpdateHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsUpdateHandler(_dbContext);
  }

  private AccountsUpdateHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Account> existingAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "test account", Type = AccountType.Cash });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsUpdateRequest request = new("new test account", AccountType.Revenue) { Id = existingAccount.Entity.Id };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Account? dbAccount = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == existingAccount.Entity.Id);
    
    dbAccount.Should().NotBeNull();
    dbAccount!.Name.Should().Be(request.Name);
    dbAccount.Type.Should().Be(request.Type);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    AccountsUpdateRequest request = new("new test account", AccountType.Revenue) { Id = Guid.NewGuid() };

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
