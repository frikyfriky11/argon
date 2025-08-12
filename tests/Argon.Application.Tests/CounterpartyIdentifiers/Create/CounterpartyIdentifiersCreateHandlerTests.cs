using Argon.Application.CounterpartyIdentifiers.Create;

namespace Argon.Application.Tests.CounterpartyIdentifiers.Create;

public class CounterpartyIdentifiersCreateHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartyIdentifiersCreateHandler(_dbContext);
  }

  private CounterpartyIdentifiersCreateHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithValidRequest()
  {
    // arrange
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty
    {
      Name = "test counterparty",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);
    
    CounterpartyIdentifiersCreateRequest request = new(
      counterparty.Entity.Id,
      "test counterparty identifier"
    );

    // act
    CounterpartyIdentifiersCreateResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CounterpartyIdentifier? dbCounterpartyIdentifier = await _dbContext.CounterpartyIdentifiers.FirstOrDefaultAsync(x => x.Id == result.Id);
    dbCounterpartyIdentifier.Should().NotBeNull();
    dbCounterpartyIdentifier!.CounterpartyId.Should().Be(request.CounterpartyId);
    dbCounterpartyIdentifier.IdentifierText.Should().Be(request.IdentifierText);
  }
}
