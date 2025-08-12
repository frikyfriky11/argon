using Argon.Application.CounterpartyIdentifiers.GetList;

namespace Argon.Application.Tests.CounterpartyIdentifiers.GetList;

public class CounterpartyIdentifiersGetListHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new CounterpartyIdentifiersGetListHandler(_dbContext);
  }

  private IApplicationDbContext _dbContext = null!;
  private CounterpartyIdentifiersGetListHandler _sut = null!;

  private List<CounterpartyIdentifier> CreateTestCounterpartyIdentifiers()
  {
    CounterpartyIdentifier counterpartyIdentifier1 = new()
    {
      Counterparty = new Counterparty
      {
        Name = "test counterparty 1",
      },
      IdentifierText = "test counterparty identifier 1",
    };
    
    CounterpartyIdentifier counterpartyIdentifier2 = new()
    {
      Counterparty = new Counterparty
      {
        Name = "test counterparty 2",
      },
      IdentifierText = "test counterparty identifier 2",
    };
    
    CounterpartyIdentifier counterpartyIdentifier3 = new()
    {
      Counterparty = new Counterparty
      {
        Name = "test counterparty 3",
      },
      IdentifierText = "test counterparty identifier 3",
    };

    return new List<CounterpartyIdentifier>
    {
      counterpartyIdentifier1,
      counterpartyIdentifier2,
      counterpartyIdentifier3,
    };
  }

  private static void CheckResults(
    PaginatedList<CounterpartyIdentifiersGetListResponse> result,
    List<CounterpartyIdentifier> counterpartyIdentifiers,
    int count,
    int totalPages,
    int totalCount,
    int[] expectedItems
    )
  {
    result.Items.Should().HaveCount(count);

    for (int i = 0; i < result.Items.Count; i++)
    {
      CounterpartyIdentifiersGetListResponse resultItem = result.Items[i];
      CounterpartyIdentifier expectedItem = counterpartyIdentifiers[expectedItems[i] - 1];

      resultItem.Id.Should().Be(expectedItem.Id);
      resultItem.CounterpartyId.Should().Be(expectedItem.CounterpartyId);
      resultItem.IdentifierText.Should().Be(expectedItem.IdentifierText);
    }
    
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(totalPages);
    result.TotalCount.Should().Be(totalCount);
    result.HasNextPage.Should().Be(totalPages > 1);
    result.HasPreviousPage.Should().BeFalse();
  }

  [Test]
  public async Task Handle_ShouldRetrieveAllCounterpartyIdentifiers_WithoutFilters()
  {
    // arrange
    List<CounterpartyIdentifier> counterpartyIdentifiers = CreateTestCounterpartyIdentifiers();

    await _dbContext.CounterpartyIdentifiers.AddRangeAsync(counterpartyIdentifiers);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartyIdentifiersGetListRequest request = new(null, null, 1, 2);

    // act
    PaginatedList<CounterpartyIdentifiersGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CheckResults(result, counterpartyIdentifiers, 2, 2, 3, [1, 2]);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyCounterpartyIdentifiersWithRequestedName_WithNameFilter()
  {
    // arrange
    List<CounterpartyIdentifier> counterpartyIdentifiers = CreateTestCounterpartyIdentifiers();

    await _dbContext.CounterpartyIdentifiers.AddRangeAsync(counterpartyIdentifiers);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    CounterpartyIdentifiersGetListRequest request = new(null, "2");

    // act
    PaginatedList<CounterpartyIdentifiersGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CheckResults(result, counterpartyIdentifiers, 1, 1, 1, [2]);
  }
}
