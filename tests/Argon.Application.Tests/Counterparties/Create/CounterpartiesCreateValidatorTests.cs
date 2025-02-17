using Argon.Application.Counterparties.Create;
using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.Counterparties.Create;

public class CounterpartiesCreateValidatorTests
{
  private IApplicationDbContext _dbContext = null!;

  private CounterpartiesCreateValidator _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartiesCreateValidator(_dbContext);
  }

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenObjectIsValid()
  {
    CounterpartiesCreateRequest request = new(
      "test counterparty"
    );

    TestValidationResult<CounterpartiesCreateRequest>? result = await _sut.TestValidateAsync(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameIsTooLong()
  {
    CounterpartiesCreateRequest request = new(
      "x".Repeat(101)
    );

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameIsEmpty()
  {
    CounterpartiesCreateRequest request = new(
      string.Empty
    );

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }
}