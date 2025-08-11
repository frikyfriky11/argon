using Argon.Application.Counterparties.GetList;

namespace Argon.Application.Tests.Counterparties.GetList;

public class CounterpartiesGetListHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartiesGetListHandler(_dbContext);
  }

  private IApplicationDbContext _dbContext = null!;
  private CounterpartiesGetListHandler _sut = null!;

  private List<Counterparty> CreateTestCounterparties()
  {
    Counterparty counterparty1 = new()
    {
      Name = "Market",
    };
    
    Counterparty counterparty2 = new()
    {
      Name = "Gas station",
    };
    
    Counterparty counterparty3 = new()
    {
      Name = "Homeowner",
    };

    return new List<Counterparty>
    {
      counterparty1,
      counterparty2,
      counterparty3,
    };
  }

  private static void CheckResults(
    PaginatedList<CounterpartiesGetListResponse> result,
    List<Counterparty> counterparties,
    int count,
    int totalPages,
    int totalCount,
    int[] expectedItems
    )
  {
    result.Items.Should().HaveCount(count);

    for (int i = 0; i < result.Items.Count; i++)
    {
      CounterpartiesGetListResponse resultItem = result.Items[i];
      Counterparty expectedItem = counterparties[expectedItems[i] - 1];

      resultItem.Id.Should().Be(expectedItem.Id);
      resultItem.Name.Should().Be(expectedItem.Name);
    }
    
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(totalPages);
    result.TotalCount.Should().Be(totalCount);
    result.HasNextPage.Should().Be(totalPages > 1);
    result.HasPreviousPage.Should().BeFalse();
  }

  [Test]
  public async Task Handle_ShouldRetrieveAllCounterparties_WithoutFilters()
  {
    // arrange
    List<Counterparty> counterparties = CreateTestCounterparties();

    await _dbContext.Counterparties.AddRangeAsync(counterparties);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartiesGetListRequest request = new(null, 1, 2);

    // act
    PaginatedList<CounterpartiesGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CheckResults(result, counterparties, 2, 2, 3, [2, 3]);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyCounterpartiesWithRequestedName_WithNameFilter()
  {
    // arrange
    List<Counterparty> counterparties = CreateTestCounterparties();

    await _dbContext.Counterparties.AddRangeAsync(counterparties);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartiesGetListRequest request = new("market");

    // act
    PaginatedList<CounterpartiesGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CheckResults(result, counterparties, 1, 1, 1, [1]);
  }
}
