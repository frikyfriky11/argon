using Argon.Application.Counterparties.Common;

namespace Argon.Application.Tests.Counterparties.Common;

public class CounterpartyResolverTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
    _sut = new CounterpartyResolver(_dbContext);
  }

  private CounterpartyResolver _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task ResolveAsync_ShouldReturnEmpty_WhenRawTextIsNullOrWhitespace()
  {
    (await _sut.ResolveAsync(null, CancellationToken.None)).Should().BeEmpty();
    (await _sut.ResolveAsync("", CancellationToken.None)).Should().BeEmpty();
    (await _sut.ResolveAsync("   ", CancellationToken.None)).Should().BeEmpty();
  }

  [Test]
  public async Task ResolveAsync_ShouldMatchByName_BidirectionalSubstring()
  {
    EntityEntry<Counterparty> amazon = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Amazon" });
    await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Unrelated" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<CounterpartyResolution> longerHasName = await _sut.ResolveAsync("AMAZON EU SARL", CancellationToken.None);
    longerHasName.Should().ContainSingle(r => r.Id == amazon.Entity.Id);
    longerHasName.Single(r => r.Id == amazon.Entity.Id).MatchedByName.Should().BeTrue();

    List<CounterpartyResolution> nameHasShorter = await _sut.ResolveAsync("Amaz", CancellationToken.None);
    nameHasShorter.Should().ContainSingle(r => r.Id == amazon.Entity.Id);
  }

  [Test]
  public async Task ResolveAsync_ShouldMatchByIdentifier_AndExposeMatchSource()
  {
    EntityEntry<Counterparty> stadtwerke = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Stadtwerke Bruneck" });
    await _dbContext.CounterpartyIdentifiers.AddAsync(new CounterpartyIdentifier
    {
      Counterparty = stadtwerke.Entity,
      IdentifierText = "STADTWERKE",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<CounterpartyResolution> matches = await _sut.ResolveAsync(
      "BONIFICO STADTWERKE BRUNECK 0001",
      CancellationToken.None);

    matches.Should().ContainSingle(r => r.Id == stadtwerke.Entity.Id);
    CounterpartyResolution match = matches.Single(r => r.Id == stadtwerke.Entity.Id);
    match.MatchedByIdentifier.Should().BeTrue();
    match.MatchedByName.Should().BeTrue("the counterparty name itself is also a substring of the raw text");
  }

  [Test]
  public async Task ResolveAsync_ShouldMatchByIdentifierOnly_WhenNameIsNotASubstring()
  {
    EntityEntry<Counterparty> acme = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "ACME Corp" });
    await _dbContext.CounterpartyIdentifiers.AddAsync(new CounterpartyIdentifier
    {
      Counterparty = acme.Entity,
      IdentifierText = "DE89370400440532013000",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<CounterpartyResolution> matches = await _sut.ResolveAsync(
      "SEPA DD DE89370400440532013000 REF 42",
      CancellationToken.None);

    matches.Should().ContainSingle(r => r.Id == acme.Entity.Id);
    CounterpartyResolution match = matches.Single(r => r.Id == acme.Entity.Id);
    match.MatchedByIdentifier.Should().BeTrue();
    match.MatchedByName.Should().BeFalse("the counterparty name is not a substring of the raw text");
    match.Name.Should().Be("ACME Corp");
  }

  [Test]
  public async Task ResolveAsync_ShouldSkipOrphanedIdentifiers_WithoutThrowing()
  {
    // arrange: an identifier whose counterparty row does not exist
    await _dbContext.CounterpartyIdentifiers.AddAsync(new CounterpartyIdentifier
    {
      CounterpartyId = Guid.NewGuid(),
      IdentifierText = "ORPHANED",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    // act
    Func<Task<List<CounterpartyResolution>>> act = async () =>
      await _sut.ResolveAsync("PAYMENT ORPHANED REF", CancellationToken.None);

    // assert
    List<CounterpartyResolution> matches = (await act.Should().NotThrowAsync()).Subject;
    matches.Should().BeEmpty();
  }

  [Test]
  public async Task ResolveAsync_ShouldDedupe_WhenSameCounterpartyMatchesByBothPaths()
  {
    EntityEntry<Counterparty> cp = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Eurospar" });
    await _dbContext.CounterpartyIdentifiers.AddAsync(new CounterpartyIdentifier
    {
      Counterparty = cp.Entity,
      IdentifierText = "EUROSPAR",
    });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<CounterpartyResolution> matches = await _sut.ResolveAsync("EUROSPAR BOZEN", CancellationToken.None);

    matches.Should().ContainSingle(r => r.Id == cp.Entity.Id);
  }

  [Test]
  public async Task ResolveAsync_ShouldReturnNothing_WhenNothingMatches()
  {
    await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "Foo" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    List<CounterpartyResolution> matches = await _sut.ResolveAsync("Bar", CancellationToken.None);

    matches.Should().BeEmpty();
  }
}
