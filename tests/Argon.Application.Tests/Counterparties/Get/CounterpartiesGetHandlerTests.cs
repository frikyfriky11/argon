using Argon.Application.Counterparties.Get;

namespace Argon.Application.Tests.Counterparties.Get;

public class CounterpartiesGetHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartiesGetHandler(_dbContext);
  }

  private CounterpartiesGetHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithExistingId()
  {
    // arrange
    Counterparty counterparty = new() { Name = "test counterparty" };

    await _dbContext.Counterparties.AddAsync(counterparty);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartiesGetRequest request = new(counterparty.Id);

    // act
    CounterpartiesGetResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Id.Should().Be(counterparty.Id);
    result.Name.Should().Be(counterparty.Name);
  }

  [Test]
  public async Task Handle_ShouldThrowNotFoundException_WithNonExistingId()
  {
    // arrange
    CounterpartiesGetRequest request = new(Guid.NewGuid());

    // act
    Func<Task<CounterpartiesGetResponse>> act = async () => await _sut.Handle(request, CancellationToken.None);

    // assert
    await act.Should().ThrowAsync<NotFoundException>();
  }
}
