using Argon.Application.Counterparties.Update;

namespace Argon.Application.Tests.Counterparties.Update;

public class CounterpartiesUpdateHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartiesUpdateHandler(_dbContext);
  }

  private CounterpartiesUpdateHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Counterparty> existingCounterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "test counterparty" });
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartiesUpdateRequest request = new("new test counterparty") { Id = existingCounterparty.Entity.Id };

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Counterparty? dbCounterparty = await _dbContext.Counterparties.FirstOrDefaultAsync(x => x.Id == existingCounterparty.Entity.Id);
    
    dbCounterparty.Should().NotBeNull();
    dbCounterparty!.Name.Should().Be(request.Name);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    CounterpartiesUpdateRequest request = new("new test counterparty") { Id = Guid.NewGuid() };

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
