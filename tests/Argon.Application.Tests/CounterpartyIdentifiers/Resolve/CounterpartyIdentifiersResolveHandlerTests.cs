using Argon.Application.Counterparties.Common;
using Argon.Application.CounterpartyIdentifiers.Resolve;

namespace Argon.Application.Tests.CounterpartyIdentifiers.Resolve;

public class CounterpartyIdentifiersResolveHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new CounterpartyIdentifiersResolveHandler(new CounterpartyResolver(_dbContext));
  }

  private CounterpartyIdentifiersResolveHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldProjectResolverOutputIntoResponse()
  {
    EntityEntry<Counterparty> cp = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Amazon" });
    await _dbContext.CounterpartyIdentifiers.AddAsync(new CounterpartyIdentifier
    {
      Counterparty = cp.Entity,
      IdentifierText = "AMAZON",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<CounterpartyIdentifiersResolveResponse> result =
      await _sut.Handle(new CounterpartyIdentifiersResolveRequest("AMAZON EU SARL"), CancellationToken.None);

    result.Should().ContainSingle();
    result[0].CounterpartyId.Should().Be(cp.Entity.Id);
    result[0].CounterpartyName.Should().Be("Amazon");
    result[0].MatchedByIdentifier.Should().BeTrue();
    result[0].MatchedByName.Should().BeTrue();
  }

  [Test]
  public async Task Handle_ShouldReturnEmpty_WhenNothingMatches()
  {
    await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Other" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<CounterpartyIdentifiersResolveResponse> result =
      await _sut.Handle(new CounterpartyIdentifiersResolveRequest("Nothing matches"), CancellationToken.None);

    result.Should().BeEmpty();
  }
}
