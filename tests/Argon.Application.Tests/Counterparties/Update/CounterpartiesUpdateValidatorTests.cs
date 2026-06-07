using Argon.Application.Counterparties.Update;
using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.Counterparties.Update;

public class CounterpartiesUpdateValidatorTests
{
  private IApplicationDbContext _dbContext = null!;

  private CounterpartiesUpdateValidator _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartiesUpdateValidator(_dbContext);
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

  [Test]
  public async Task Validator_ShouldReturnError_WhenNameAlreadyBelongsToAnotherCounterparty()
  {
    // arrange
    await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Amazon" });
    EntityEntry<Counterparty> edited = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Eurospar" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartiesUpdateRequest request = new("amazon") { Id = edited.Entity.Id };

    // act + assert
    await _sut.ShouldFailOnProperty(request, nameof(request.Name));
  }

  [Test]
  public async Task Validator_ShouldNotReturnError_WhenCounterpartyKeepsItsOwnName()
  {
    // arrange
    EntityEntry<Counterparty> edited = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Amazon" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartiesUpdateRequest request = new("Amazon") { Id = edited.Entity.Id };

    // act + assert
    await _sut.ShouldNotFailOnProperty(request, nameof(request.Name));
  }
}