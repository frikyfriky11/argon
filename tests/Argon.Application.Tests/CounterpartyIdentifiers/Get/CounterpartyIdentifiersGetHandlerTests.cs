using Argon.Application.CounterpartyIdentifiers.Get;

namespace Argon.Application.Tests.CounterpartyIdentifiers.Get;

public class CounterpartyIdentifiersGetHandlerTests
{
  private IApplicationDbContext _dbContext = null!;

  private CounterpartyIdentifiersGetHandler _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartyIdentifiersGetHandler(_dbContext);
  }

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty
    {
      Name = "test counterparty",
    });
    CounterpartyIdentifier counterpartyIdentifier = new()
    {
      CounterpartyId = counterparty.Entity.Id,
      IdentifierText = "test counterparty identifier",
    };

    await _dbContext.CounterpartyIdentifiers.AddAsync(counterpartyIdentifier);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartyIdentifiersGetRequest request = new(counterpartyIdentifier.Id);

    // act
    CounterpartyIdentifiersGetResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Id.Should().Be(counterpartyIdentifier.Id);
    result.CounterpartyId.Should().Be(counterpartyIdentifier.CounterpartyId);
    result.IdentifierText.Should().Be(counterpartyIdentifier.IdentifierText);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    CounterpartyIdentifiersGetRequest request = new(Guid.NewGuid());

    // act
    Func<Task<CounterpartyIdentifiersGetResponse>> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}