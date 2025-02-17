using Argon.Application.Counterparties.Create;

namespace Argon.Application.Tests.Counterparties.Create;

public class CounterpartiesCreateHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartiesCreateHandler(_dbContext);
  }

  private CounterpartiesCreateHandler _sut = null!;
  private IApplicationDbContext _dbContext = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithValidRequest()
  {
    // arrange
    CounterpartiesCreateRequest request = new(
      "test counterparty"
    );

    // act
    CounterpartiesCreateResponse result = await _sut.Handle(request, CancellationToken.None);

    // assert
    Counterparty? dbCounterparty = await _dbContext.Counterparties.FirstOrDefaultAsync(x => x.Id == result.Id);
    dbCounterparty.Should().NotBeNull();
    dbCounterparty!.Name.Should().Be(request.Name);
  }
}
