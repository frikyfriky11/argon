using Argon.Application.CounterpartyIdentifiers.Update;
using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.CounterpartyIdentifiers.Update;

public class CounterpartyIdentifiersUpdateValidatorTests
{
  private IApplicationDbContext _dbContext = null!;

  private CounterpartyIdentifiersUpdateValidator _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartyIdentifiersUpdateValidator(_dbContext);
  }

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenObjectIsValid()
  {
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty
    {
      Name = "test counterparty",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartyIdentifiersUpdateRequest request = new(counterparty.Entity.Id, "new test counterpartyIdentifier");

    TestValidationResult<CounterpartyIdentifiersUpdateRequest>? result = await _sut.TestValidateAsync(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenIdentifierTextIsTooLong()
  {
    CounterpartyIdentifiersUpdateRequest request = new(Guid.NewGuid(), "x".Repeat(251));

    await _sut.ShouldFailOnProperty(request, nameof(request.IdentifierText));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenIdentifierTextIsEmpty()
  {
    CounterpartyIdentifiersUpdateRequest request = new(Guid.NewGuid(), string.Empty);

    await _sut.ShouldFailOnProperty(request, nameof(request.IdentifierText));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenCounterpartyIdDoesNotExist()
  {
    CounterpartyIdentifiersUpdateRequest request = new(Guid.NewGuid(), "test counterparty identifier");

    await _sut.ShouldFailOnProperty(request, nameof(request.CounterpartyId));
  }
}