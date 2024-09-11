using Argon.Application.Accounts.Create;
using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.Accounts;

[TestFixture]
public class AccountsCreateValidatorTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new AccountsCreateValidator(_dbContext);
  }

  private AccountsCreateValidator _sut = null!;
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
