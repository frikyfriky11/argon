using Argon.Application.Transactions.GetList;

namespace Argon.Application.Tests.Transactions.GetList;

[TestFixture]
public class TransactionsGetListHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _sut = new TransactionsGetListHandler(_dbContext);
  }

  private IApplicationDbContext _dbContext = null!;
  private TransactionsGetListHandler _sut = null!;

  [Test]
  public async Task Handle_ShouldRetrieveAllTransactions_WithoutFilters()
  {
    // arrange
    List<Transaction> sampleEntities = new()
    {
      new Transaction { Date = new DateOnly(2023, 9, 18), Description = "test transaction 1" },
      new Transaction { Date = new DateOnly(2023, 9, 19), Description = "test transaction 2" },
      new Transaction { Date = new DateOnly(2023, 9, 20), Description = "test transaction 3" },
    };

    await _dbContext.Transactions.AddRangeAsync(sampleEntities);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(null, null, null, null, 1, 2);

    List<TransactionsGetListResponse> expected = sampleEntities
      .OrderByDescending(transaction => transaction.Date)
      .ThenByDescending(transaction => transaction.Created)
      .ThenByDescending(transaction => transaction.Id)
      .Select(transaction => new TransactionsGetListResponse(
        transaction.Id,
        transaction.Date,
        transaction.Description,
        transaction.TransactionRows
          .Select(row => new TransactionRowsGetListResponse(
            row.Id,
            row.RowCounter,
            row.AccountId,
            row.Account.Name,
            row.Debit,
            row.Credit,
            row.Description
          ))
          .ToList()
      ))
      .Take(2)
      .ToList();

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Items.Should().BeEquivalentTo(expected);
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(2);
    result.TotalCount.Should().Be(3);
    result.HasNextPage.Should().Be(true);
    result.HasPreviousPage.Should().Be(false);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyTransactionsWithRequestedIds_WithAccountIdsFilter()
  {
    // arrange
    Account bankAccount = new() { Name = "Bank" };
    Account groceriesAccount = new() { Name = "Groceries" };
    Account carAccount = new() { Name = "Car" };
    Account rentAccount = new() { Name = "Rent" };

    List<Transaction> sampleEntities = new()
    {
      new Transaction
      {
        Description = "grocery shopping",
        TransactionRows = new List<TransactionRow>
        {
          new() { Account = groceriesAccount, Debit = 50 },
          new() { Account = bankAccount, Credit = 50 },
        },
      },
      new Transaction
      {
        Description = "gas filling",
        TransactionRows = new List<TransactionRow>
        {
          new() { Account = carAccount, Debit = 100 },
          new() { Account = bankAccount, Credit = 100 },
        },
      },
      new Transaction
      {
        Description = "rent paying",
        TransactionRows = new List<TransactionRow>
        {
          new() { Account = rentAccount, Debit = 600 },
          new() { Account = bankAccount, Credit = 600 },
        },
      },
    };

    await _dbContext.Transactions.AddRangeAsync(sampleEntities);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(new List<Guid> { groceriesAccount.Id, rentAccount.Id }, null, null, null);

    List<TransactionsGetListResponse> expected = sampleEntities
      .Where(x => x.TransactionRows.Any(row => request.AccountIds!.Contains(row.AccountId)))
      .OrderByDescending(transaction => transaction.Date)
      .ThenByDescending(transaction => transaction.Created)
      .ThenByDescending(transaction => transaction.Id)
      .Select(transaction => new TransactionsGetListResponse(
        transaction.Id,
        transaction.Date,
        transaction.Description,
        transaction.TransactionRows
          .Select(row => new TransactionRowsGetListResponse(
            row.Id,
            row.RowCounter,
            row.AccountId,
            row.Account.Name,
            row.Debit,
            row.Credit,
            row.Description
          ))
          .ToList()
      ))
      .ToList();

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Items.Should().BeEquivalentTo(expected);
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(1);
    result.TotalCount.Should().Be(2);
    result.HasNextPage.Should().Be(false);
    result.HasPreviousPage.Should().Be(false);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyTransactionsWithRequestedDescription_WithDescriptionFilter()
  {
    // arrange
    List<Transaction> sampleEntities = new()
    {
      new Transaction { Description = "grocery shopping 1" },
      new Transaction { Description = "grocery shopping 2" },
      new Transaction { Description = "gas filling" },
      new Transaction { Description = "rent paying" },
    };

    await _dbContext.Transactions.AddRangeAsync(sampleEntities);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(null, "shopping", null, null);

    List<TransactionsGetListResponse> expected = sampleEntities
      .Where(x => x.Description.Contains(request.Description!))
      .OrderByDescending(transaction => transaction.Date)
      .ThenByDescending(transaction => transaction.Created)
      .ThenByDescending(transaction => transaction.Id)
      .Select(transaction => new TransactionsGetListResponse(
        transaction.Id,
        transaction.Date,
        transaction.Description,
        transaction.TransactionRows
          .Select(row => new TransactionRowsGetListResponse(
            row.Id,
            row.RowCounter,
            row.AccountId,
            row.Account.Name,
            row.Debit,
            row.Credit,
            row.Description
          ))
          .ToList()
      ))
      .ToList();

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Items.Should().BeEquivalentTo(expected);
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(1);
    result.TotalCount.Should().Be(2);
    result.HasNextPage.Should().Be(false);
    result.HasPreviousPage.Should().Be(false);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyTransactionsWithRequestedDateFrom_WithDateFromFilter()
  {
    // arrange
    List<Transaction> sampleEntities = new()
    {
      new Transaction { Date = new DateOnly(2023, 8, 7), Description = "grocery shopping" },
      new Transaction { Date = new DateOnly(2023, 8, 6), Description = "gas filling" },
      new Transaction { Date = new DateOnly(2023, 8, 5), Description = "rent paying" },
    };

    await _dbContext.Transactions.AddRangeAsync(sampleEntities);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(null, null, new DateTimeOffset(new DateTime(2023, 8, 6)), null);

    List<TransactionsGetListResponse> expected = sampleEntities
      .Where(x => x.Date >= DateOnly.FromDateTime(request.DateFrom!.Value.Date))
      .OrderByDescending(transaction => transaction.Date)
      .ThenByDescending(transaction => transaction.Created)
      .ThenByDescending(transaction => transaction.Id)
      .Select(transaction => new TransactionsGetListResponse(
        transaction.Id,
        transaction.Date,
        transaction.Description,
        transaction.TransactionRows
          .Select(row => new TransactionRowsGetListResponse(
            row.Id,
            row.RowCounter,
            row.AccountId,
            row.Account.Name,
            row.Debit,
            row.Credit,
            row.Description
          ))
          .ToList()
      ))
      .ToList();

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Items.Should().BeEquivalentTo(expected);
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(1);
    result.TotalCount.Should().Be(2);
    result.HasNextPage.Should().Be(false);
    result.HasPreviousPage.Should().Be(false);
  }

  [Test]
  public async Task Handle_ShouldRetrieveOnlyTransactionsWithRequestedDateTo_WithDateToFilter()
  {
    // arrange
    List<Transaction> sampleEntities = new()
    {
      new Transaction { Date = new DateOnly(2023, 8, 7), Description = "grocery shopping" },
      new Transaction { Date = new DateOnly(2023, 8, 6), Description = "gas filling" },
      new Transaction { Date = new DateOnly(2023, 8, 5), Description = "rent paying" },
    };

    await _dbContext.Transactions.AddRangeAsync(sampleEntities);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(null, null, null, new DateTimeOffset(new DateTime(2023, 8, 6)));

    List<TransactionsGetListResponse> expected = sampleEntities
      .Where(x => x.Date <= DateOnly.FromDateTime(request.DateTo!.Value.Date))
      .OrderByDescending(transaction => transaction.Date)
      .ThenByDescending(transaction => transaction.Created)
      .ThenByDescending(transaction => transaction.Id)
      .Select(transaction => new TransactionsGetListResponse(
        transaction.Id,
        transaction.Date,
        transaction.Description,
        transaction.TransactionRows
          .Select(row => new TransactionRowsGetListResponse(
            row.Id,
            row.RowCounter,
            row.AccountId,
            row.Account.Name,
            row.Debit,
            row.Credit,
            row.Description
          ))
          .ToList()
      ))
      .ToList();

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Items.Should().BeEquivalentTo(expected);
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(1);
    result.TotalCount.Should().Be(2);
    result.HasNextPage.Should().Be(false);
    result.HasPreviousPage.Should().Be(false);
  }
}
