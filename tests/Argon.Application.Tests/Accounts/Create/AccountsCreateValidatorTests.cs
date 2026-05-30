using Argon.Application.Accounts.Create;
using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.Accounts.Create;

public class AccountsCreateValidatorTests
{
  [SetUp]
  public void SetUp()
  {
    _sut = new AccountsCreateValidator();
  }

  private AccountsCreateValidator _sut = null!;

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenObjectIsValid()
  {
    AccountsCreateRequest request = new(
      "test account", 
      AccountType.Cash
    );

    TestValidationResult<AccountsCreateRequest>? result = await _sut.TestValidateAsync(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameIsTooLong()
  {
    AccountsCreateRequest request = new(
      "x".Repeat(51),
      AccountType.Cash
    );

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameIsEmpty()
  {
    AccountsCreateRequest request = new(
      string.Empty,
      AccountType.Cash
    );

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenTypeIsNotInEnum()
  {
    AccountsCreateRequest request = new(
      "test account",
      (AccountType)999
    );

    await _sut.ShouldFailOnProperty(request, nameof(request.Type));
  }
}