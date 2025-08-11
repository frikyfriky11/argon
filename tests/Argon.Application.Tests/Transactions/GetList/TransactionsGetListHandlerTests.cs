using Argon.Application.Transactions.GetList;

namespace Argon.Application.Tests.Transactions.GetList;

public class TransactionsGetListHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _bankAccountId = Guid.NewGuid();
    _groceriesAccountId = Guid.NewGuid();
    _carAccountId = Guid.NewGuid();
    _rentAccountId = Guid.NewGuid();
    _marketCounterpartyId  = Guid.NewGuid();
    _gasStationCounterpartyId = Guid.NewGuid();
    _homeownerCounterpartyId = Guid.NewGuid();

    _sut = new TransactionsGetListHandler(_dbContext);
  }

  private IApplicationDbContext _dbContext = null!;
  private TransactionsGetListHandler _sut = null!;
  private Guid _bankAccountId;
  private Guid _groceriesAccountId;
  private Guid _carAccountId;
  private Guid _rentAccountId;
  private Guid _marketCounterpartyId;
  private Guid _gasStationCounterpartyId;
  private Guid _homeownerCounterpartyId;

  private List<Transaction> CreateTestTransactions()
  {
    Account bankAccount = new() { Id = _bankAccountId, Name = "Bank" };
    Account groceriesAccount = new() { Id = _groceriesAccountId, Name = "Groceries" };
    Account carAccount = new() { Id = _carAccountId, Name = "Car" };
    Account rentAccount = new() { Id = _rentAccountId, Name = "Rent" };
    Counterparty marketCounterparty = new() { Id = _marketCounterpartyId, Name = "Market" };
    Counterparty gasStationCounterparty = new() { Id = _gasStationCounterpartyId, Name = "Gas station" };
    Counterparty homeownerCounterparty = new() { Id = _homeownerCounterpartyId, Name = "Home owner" };

    TransactionRow transaction1Row1 = new() { RowCounter = 1, Account = groceriesAccount, Debit = 50 };
    TransactionRow transaction1Row2 = new() { RowCounter = 2, Account = bankAccount, Credit = 50 };
    Transaction transaction1 = new()
    {
      Counterparty = marketCounterparty,
      Date = new DateOnly(2024, 9, 12),
      TransactionRows = new List<TransactionRow>
      {
        transaction1Row1,
        transaction1Row2,
      },
    };
    
    TransactionRow transaction2Row1 = new() { RowCounter = 1, Account = carAccount, Debit = 100 };
    TransactionRow transaction2Row2 = new() { RowCounter = 2, Account = bankAccount, Credit = 100 };
    Transaction transaction2 = new()
    {
      Counterparty = gasStationCounterparty,
      Date = new DateOnly(2024, 9, 13),
      TransactionRows = new List<TransactionRow>
      {
        transaction2Row1,
        transaction2Row2,
      },
    };
    
    TransactionRow transaction3Row1 = new() { RowCounter = 1, Account = rentAccount, Debit = 600 };
    TransactionRow transaction3Row2 = new() { RowCounter = 2, Account = bankAccount, Credit = 600 };
    Transaction transaction3 = new()
    {
      Counterparty = homeownerCounterparty,
      Date = new DateOnly(2024, 9, 14),
      TransactionRows = new List<TransactionRow>
      {
        transaction3Row1,
        transaction3Row2,
      },
    };

    return new List<Transaction>
    {
      transaction1,
      transaction2,
      transaction3,
    };
  }

  private static void CheckResults(
    PaginatedList<TransactionsGetListResponse> result,
    List<Transaction> transactions,
    int count,
    int totalPages,
    int totalCount,
    int[] expectedItems
    )
  {
    result.Items.Should().HaveCount(count);

    for (int i = 0; i < result.Items.Count; i++)
    {
      TransactionsGetListResponse resultItem = result.Items[i];
      Transaction expectedItem = transactions[expectedItems[i] - 1];

      resultItem.Id.Should().Be(expectedItem.Id);
      resultItem.Date.Should().Be(expectedItem.Date);
      resultItem.CounterpartyId.Should().Be(expectedItem.CounterpartyId);
      resultItem.TransactionRows.Should().HaveCount(expectedItem.TransactionRows.Count);
      resultItem.TransactionRows[0].RowCounter.Should().Be(expectedItem.TransactionRows.First().RowCounter);
      resultItem.TransactionRows[0].Description.Should().Be(expectedItem.TransactionRows.First().Description);
      resultItem.TransactionRows[0].AccountId.Should().Be(expectedItem.TransactionRows.First().AccountId);
      resultItem.TransactionRows[0].Debit.Should().Be(expectedItem.TransactionRows.First().Debit);
      resultItem.TransactionRows[0].Credit.Should().Be(expectedItem.TransactionRows.First().Credit);
      resultItem.TransactionRows[1].RowCounter.Should().Be(expectedItem.TransactionRows.Last().RowCounter);
      resultItem.TransactionRows[1].Description.Should().Be(expectedItem.TransactionRows.Last().Description);
      resultItem.TransactionRows[1].AccountId.Should().Be(expectedItem.TransactionRows.Last().AccountId);
      resultItem.TransactionRows[1].Debit.Should().Be(expectedItem.TransactionRows.Last().Debit);
      resultItem.TransactionRows[1].Credit.Should().Be(expectedItem.TransactionRows.Last().Credit);
    }
    
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(totalPages);
    result.TotalCount.Should().Be(totalCount);
    result.HasNextPage.Should().Be(totalPages > 1);
    result.HasPreviousPage.Should().BeFalse();
  }

  [Test]
  public async Task Handle_ShouldRetrieveAllTransactions_WithoutFilters()
  {
    // arrange
    List<Transaction> transactions = CreateTestTransactions();

    await _dbContext.Transactions.AddRangeAsync(transactions);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(null, null, null, null, 1, 2);

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CheckResults(result, transactions, 2, 2, 3, [3, 2]);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyTransactionsWithRequestedIds_WithAccountIdsFilter()
  {
    // arrange
    List<Transaction> transactions = CreateTestTransactions();

    await _dbContext.Transactions.AddRangeAsync(transactions);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(new List<Guid> { _groceriesAccountId, _rentAccountId }, null, null, null);

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CheckResults(result, transactions, 2, 1, 2, [3, 1]);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyTransactionsWithRequestedIds_WithCounterpartyIdsFilter()
  {
    // arrange
    List<Transaction> transactions = CreateTestTransactions();

    await _dbContext.Transactions.AddRangeAsync(transactions);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(null, new List<Guid> { _marketCounterpartyId, _homeownerCounterpartyId }, null, null);

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CheckResults(result, transactions, 2, 1, 2, [3, 1]);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyTransactionsWithRequestedDateFrom_WithDateFromFilter()
  {
    // arrange
    List<Transaction> transactions = CreateTestTransactions();

    await _dbContext.Transactions.AddRangeAsync(transactions);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(null, null, new DateTimeOffset(new DateTime(2024, 9, 13)), null);

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CheckResults(result, transactions, 2, 1, 2, [3, 2]);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyTransactionsWithRequestedDateTo_WithDateToFilter()
  {
    // arrange
    List<Transaction> transactions = CreateTestTransactions();

    await _dbContext.Transactions.AddRangeAsync(transactions);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(null, null, null, new DateTimeOffset(new DateTime(2024, 9, 13)));

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    CheckResults(result, transactions, 2, 1, 2, [2, 1]);
  }
}
