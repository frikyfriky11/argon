using Argon.Application.Accounts.Create;

namespace Argon.Application.Tests.Accounts.Create;

public class AccountsCreateHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsCreateHandler(_dbContext);
  }

  private AccountsCreateHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithValidRequest()
  {
    // arrange
    AccountsCreateRequest request = new(
      "test account",
      AccountType.Cash
    );

    // act
    AccountsCreateResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    Account? dbAccount = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == result.Id);
    dbAccount.Should().NotBeNull();
    dbAccount!.Name.Should().Be(request.Name);
    dbAccount.Type.Should().Be(request.Type);
  }
}
