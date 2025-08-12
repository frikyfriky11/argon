using Argon.Application.CounterpartyIdentifiers.Update;

namespace Argon.Application.Tests.CounterpartyIdentifiers.Update;

public class CounterpartyIdentifiersUpdateHandlerTests
{
  private IApplicationDbContext _dbContext = null!;

  private CounterpartyIdentifiersUpdateHandler _sut = null!;

  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartyIdentifiersUpdateHandler(_dbContext);
  }

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<CounterpartyIdentifier> existingCounterpartyIdentifier = await _dbContext.CounterpartyIdentifiers.AddAsync(new CounterpartyIdentifier
    {
      Counterparty = new Counterparty
      {
        Name = "test counterparty 1",
      },
      IdentifierText = "test counterparty identifier 1",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartyIdentifiersUpdateRequest request = new(existingCounterpartyIdentifier.Entity.CounterpartyId, "new test counterparty identifier")
    {
      Id = existingCounterpartyIdentifier.Entity.Id,
    };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    CounterpartyIdentifier? dbCounterpartyIdentifier = await _dbContext.CounterpartyIdentifiers.FirstOrDefaultAsync(x => x.Id == existingCounterpartyIdentifier.Entity.Id);

    dbCounterpartyIdentifier.Should().NotBeNull();
    dbCounterpartyIdentifier!.CounterpartyId.Should().Be(request.CounterpartyId);
    dbCounterpartyIdentifier.IdentifierText.Should().Be(request.IdentifierText);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    CounterpartyIdentifiersUpdateRequest request = new(Guid.NewGuid(), "new test counterpartyIdentifier")
    {
      Id = Guid.NewGuid(),
    };

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}