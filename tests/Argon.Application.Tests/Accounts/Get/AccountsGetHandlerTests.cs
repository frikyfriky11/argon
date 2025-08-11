using Argon.Application.Accounts.Get;

namespace Argon.Application.Tests.Accounts.Get;

public class AccountsGetHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsGetHandler(_dbContext);
  }

  private AccountsGetHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    Account account = new() { Name = "test account", Type = AccountType.Cash };

    await _dbContext.Accounts.AddAsync(account);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsGetRequest request = new(account.Id);

    // act
    AccountsGetResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Id.Should().Be(account.Id);
    result.Name.Should().Be(account.Name);
    result.Type.Should().Be(account.Type);
    result.IsFavourite.Should().Be(false);
    result.TotalAmount.Should().Be(0);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    AccountsGetRequest request = new(Guid.NewGuid());

    // act
    Func<Task<AccountsGetResponse>> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
