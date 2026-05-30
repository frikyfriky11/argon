using Argon.Application.Counterparties.Update;
using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.Counterparties.Update;

public class CounterpartiesUpdateValidatorTests
{
  private CounterpartiesUpdateValidator _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _sut = new CounterpartiesUpdateValidator();
  }

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenObjectIsValid()
  {
    CounterpartiesUpdateRequest request = new("new test counterparty");

    TestValidationResult<CounterpartiesUpdateRequest>? result = await _sut.TestValidateAsync(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameIsTooLong()
  {
    CounterpartiesUpdateRequest request = new("x".Repeat(101));

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameIsEmpty()
  {
    CounterpartiesUpdateRequest request = new(string.Empty);

    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }
}