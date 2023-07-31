using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.Accounts;

[TestFixture]
public class AccountsUpdateRequestValidatorTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsUpdateRequestValidator(_dbContext);
  }

  private AccountsUpdateRequestValidator _sut = null!;
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
  [TestCase(null)]
  [TestCase("")]
  [TestCase(" ")]
  public async Task Validator_ShouldReturnError_WhenNameIsNullOrWhiteSpace(string name)
  {
    AccountsUpdateRequest request = new(name, AccountType.Revenue);

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenTypeIsNotInEnum()
  {
    AccountsUpdateRequest request = new("new test account", (AccountType)999);

    await _sut.ShouldFailOnProperty(request, nameof(request.Type));
  }
}

[TestFixture]
public class AccountsUpdateRequestHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _mapper = new MapperConfiguration(config => config.AddProfile<AccountsProfile>()).CreateMapper();

    _sut = new AccountsUpdateRequestHandler(_dbContext, _mapper);
  }

  private AccountsUpdateRequestHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;
  private IMapper _mapper = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Account> existingAccount = await _dbContext.Accounts.AddAsync(new Account { Name = "test account", Type = AccountType.Cash });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    AccountsUpdateRequest request = new("new test account", AccountType.Revenue) { Id = existingAccount.Entity.Id };

    Account? expected = _mapper.Map<Account>(request);

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Account? entity = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == existingAccount.Entity.Id);

    entity.Should().BeEquivalentTo(expected, config => config
      .Excluding(x => x.Created)
      .Excluding(x => x.LastModified));
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
