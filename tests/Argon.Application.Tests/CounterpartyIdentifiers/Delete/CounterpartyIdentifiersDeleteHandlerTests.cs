using Argon.Application.CounterpartyIdentifiers.Delete;

namespace Argon.Application.Tests.CounterpartyIdentifiers.Delete;

public class CounterpartyIdentifiersDeleteHandlerTests
{
  private IApplicationDbContext _dbContext = null!;

  private CounterpartyIdentifiersDeleteHandler _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartyIdentifiersDeleteHandler(_dbContext);
  }

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty
    {
      Name = "test counterparty",
    });
    EntityEntry<CounterpartyIdentifier> counterpartyIdentifier = await _dbContext.CounterpartyIdentifiers.AddAsync(new CounterpartyIdentifier
    {
      CounterpartyId = counterparty.Entity.Id,
      IdentifierText = "test counterpartyIdentifier",
    });

    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartyIdentifiersDeleteRequest request = new(counterpartyIdentifier.Entity.Id);

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    CounterpartyIdentifier? dbCounterpartyIdentifier = await _dbContext.CounterpartyIdentifiers.FirstOrDefaultAsync(x => x.Id == counterpartyIdentifier.Entity.Id);
    dbCounterpartyIdentifier.Should().BeNull();
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    CounterpartyIdentifiersDeleteRequest request = new(Guid.NewGuid());

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}