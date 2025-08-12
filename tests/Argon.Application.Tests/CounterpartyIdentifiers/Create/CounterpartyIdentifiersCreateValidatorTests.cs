using Argon.Application.CounterpartyIdentifiers.Create;
using Argon.Application.Tests.Extensions;

namespace Argon.Application.Tests.CounterpartyIdentifiers.Create;

public class CounterpartyIdentifiersCreateValidatorTests
{
  private IApplicationDbContext _dbContext = null!;

  private CounterpartyIdentifiersCreateValidator _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartyIdentifiersCreateValidator(_dbContext);
  }

  [Test]
  public async Task Validator_ShouldReturnZeroErrors_WhenObjectIsValid()
  {
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty
    {
      Name = "test counterparty",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartyIdentifiersCreateRequest request = new(
      counterparty.Entity.Id,
      "test counterparty identifier"
    );

    TestValidationResult<CounterpartyIdentifiersCreateRequest>? result = await _sut.TestValidateAsync(request);

    result.ShouldNotHaveAnyValidationErrors();
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenIdentifierTextIsTooLong()
  {
    CounterpartyIdentifiersCreateRequest request = new(
      Guid.NewGuid(),
      "x".Repeat(251)
    );

    await _sut.ShouldFailOnProperty(request, nameof(request.IdentifierText));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenIdentifierTextIsEmpty()
  {
    CounterpartyIdentifiersCreateRequest request = new(
      Guid.NewGuid(),
      string.Empty
    );

    await _sut.ShouldFailOnProperty(request, nameof(request.IdentifierText));
  }

  [Test]
  public async Task Validator_ShouldReturnError_WhenCounterpartyIdDoesNotExist()
  {
    CounterpartyIdentifiersCreateRequest request = new(Guid.NewGuid(), "test counterparty identifier");

    await _sut.ShouldFailOnProperty(request, nameof(request.CounterpartyId));
  }
}