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
    AccountsCreateRequest request = new("test account", AccountType.Cash);

    Account expected = new()
    {
      Name = request.Name,
      Type = request.Type,
    };

    // act
    AccountsCreateResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    Account? entity = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == result.Id);
    entity.Should().BeEquivalentTo(expected, config => config
      .Excluding(x => x.Id)
      .Excluding(x => x.Created)
      .Excluding(x => x.LastModified));
  }
}
