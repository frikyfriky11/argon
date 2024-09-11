using Argon.Application.Accounts.Get;

namespace Argon.Application.Tests.Accounts;

[TestFixture]
public class AccountsGetHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _mapper = new MapperConfiguration(config => config.AddProfile<AccountsProfile>()).CreateMapper();

    _sut = new AccountsGetHandler(_dbContext, _mapper);
  }

  private AccountsGetHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;
  private IMapper _mapper = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    Account account = new() { Name = "test account", Type = AccountType.Cash };

    await _dbContext.Accounts.AddAsync(account);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsGetRequest request = new(account.Id);

    AccountsGetResponse expected = _mapper.Map<AccountsGetResponse>(account);

    // act
    AccountsGetResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Should().BeEquivalentTo(expected);
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
