using Argon.Application.Accounts.Delete;

namespace Argon.Application.Tests.Accounts.Delete;

[TestFixture]
public class AccountsDeleteHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsDeleteHandler(_dbContext);
  }

  private AccountsDeleteHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Account> account = await _dbContext.Accounts.AddAsync(new Account { Name = "test account" });

    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsDeleteRequest request = new(account.Entity.Id);

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    bool entityExists = await _dbContext.Accounts.AnyAsync(x => x.Id == account.Entity.Id);
    entityExists.Should().BeFalse();
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    AccountsDeleteRequest request = new(Guid.NewGuid());

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
