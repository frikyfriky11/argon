using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.Accounts;

[TestFixture]
public class AccountsCreateRequestValidatorTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsCreateRequestValidator(_dbContext);
  }

  private AccountsCreateRequestValidator _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenObjectIsValid()
  {
    AccountsCreateRequest request = new("test account", AccountType.Cash);

    TestValidationResult<AccountsCreateRequest>? result = await _sut.TestValidateAsync(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameIsTooLong()
  {
    AccountsCreateRequest request = new("x".Repeat(51), AccountType.Cash);

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  [TestCase(null)]
  [TestCase("")]
  [TestCase(" ")]
  public async Task Validator_ShouldReturnError_WhenNameIsNullOrWhiteSpace(string name)
  {
    AccountsCreateRequest request = new(name, AccountType.Cash);

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenTypeIsNotInEnum()
  {
    AccountsCreateRequest request = new("test account", (AccountType)999);

    await _sut.ShouldFailOnProperty(request, nameof(request.Type));
  }
}

[TestFixture]
public class AccountsCreateRequestHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _mapper = new MapperConfiguration(config => config.AddProfile<AccountsProfile>()).CreateMapper();

    _sut = new AccountsCreateRequestHandler(_dbContext, _mapper);
  }

  private AccountsCreateRequestHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;
  private IMapper _mapper = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithValidRequest()
  {
    // arrange
    AccountsCreateRequest request = new("test account", AccountType.Cash);

    Account? expected = _mapper.Map<Account>(request);

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
