namespace Argon.Application.Tests.Accounts;

[TestFixture]
public class AccountsGetListRequestHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _mapper = new MapperConfiguration(config => config.AddProfile<AccountsProfile>()).CreateMapper();

    _sut = new AccountsGetListRequestHandler(_dbContext, _mapper);
  }

  private IApplicationDbContext _dbContext = null!;
  private AccountsGetListRequestHandler _sut = null!;
  private IMapper _mapper = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithValidRequest()
  {
    // arrange
    List<Account> accounts = new()
    {
      new Account { Name = "test account 1", Type = AccountType.Cash },
      new Account { Name = "test account 2", Type = AccountType.Credit },
      new Account { Name = "test account 3", Type = AccountType.Debit },
    };

    await _dbContext.Accounts.AddRangeAsync(accounts);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsGetListRequest request = new();

    List<AccountsGetListResponse>? expected = _mapper.Map<List<AccountsGetListResponse>>(accounts
      .OrderBy(x => x.Name)
      .ToList());

    // act
    List<AccountsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Should().BeEquivalentTo(expected);
  }
}
