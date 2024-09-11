using Argon.Application.Accounts.Favourite;

namespace Argon.Application.Tests.Accounts;

[TestFixture]
public class AccountsFavouriteHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsFavouriteHandler(_dbContext);
  }

  private AccountsFavouriteHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  [TestCase(true)]
  [TestCase(false)]
  public async Task Handle_ShouldToggleFlagCorrectly_WithValidRequest(bool value)
  {
    // arrange
    EntityEntry<Account> existingAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "test account", Type = AccountType.Cash, IsFavourite = !value });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsFavouriteRequest request = new(value) { Id = existingAccount.Entity.Id };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Account? entity = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == existingAccount.Entity.Id);

    entity!.IsFavourite.Should().Be(value);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    AccountsFavouriteRequest request = new(false) { Id = Guid.NewGuid() };

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
