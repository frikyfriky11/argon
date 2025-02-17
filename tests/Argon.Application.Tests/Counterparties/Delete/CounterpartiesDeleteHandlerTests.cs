using Argon.Application.Counterparties.Delete;

namespace Argon.Application.Tests.Counterparties.Delete;

public class CounterpartiesDeleteHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartiesDeleteHandler(_dbContext);
  }

  private CounterpartiesDeleteHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    EntityEntry<Counterparty> counterparty = await _dbContext.Counterparties.AddAsync(new Counterparty { Name = "test counterparty" });

    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartiesDeleteRequest request = new(counterparty.Entity.Id);

    // act
    await _sut.Handle(request, CancellationToken.None);

    // assert
    Counterparty? dbCounterparty = await _dbContext.Counterparties.FirstOrDefaultAsync(x => x.Id == counterparty.Entity.Id);
    dbCounterparty.Should().BeNull();
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    CounterpartiesDeleteRequest request = new(Guid.NewGuid());

    // act
    Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
