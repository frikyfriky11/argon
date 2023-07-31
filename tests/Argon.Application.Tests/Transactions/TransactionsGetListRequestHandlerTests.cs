﻿using Argon.Application.Transactions;

namespace Argon.Application.Tests.Transactions;

[TestFixture]
public class TransactionsGetListRequestHandlerTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();

    _mapper = new MapperConfiguration(config => config.AddProfile<TransactionsProfile>()).CreateMapper();

    _sut = new TransactionsGetListRequestHandler(_dbContext, _mapper);
  }

  private IApplicationDbContext _dbContext = null!;
  private TransactionsGetListRequestHandler _sut = null!;
  private IMapper _mapper = null!;

  [Test]
  public async Task Handle_ShouldCompleteCorrectly_WithValidRequest()
  {
    // arrange
    List<Transaction> sampleEntities = new()
    {
      new Transaction { Description = "test transaction 1" },
      new Transaction { Description = "test transaction 2" },
      new Transaction { Description = "test transaction 3" },
    };

    await _dbContext.Transactions.AddRangeAsync(sampleEntities);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(null, 1, 2);

    List<TransactionsGetListResponse>? expected = _mapper.Map<List<TransactionsGetListResponse>>(sampleEntities
      .OrderBy(x => x.Date)
      .Take(2)
      .ToList());

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
  public async Task Handle_ShouldCompleteCorrectly_WithAccountIdsFilter()
  {
    // arrange
    Account bankAccount = new() { Name = "Bank" };
    Account groceriesAccount = new() { Name = "Groceries" };
    Account carAccount = new() { Name = "Car" };

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
        Description = "test transaction 2",
        TransactionRows = new List<TransactionRow>
        {
          new() { Account = carAccount, Debit = 100 },
          new() { Account = bankAccount, Credit = 100 },
        },
      },
    };

    await _dbContext.Transactions.AddRangeAsync(sampleEntities);
    await _dbContext.SaveChangesAsync(CancellationToken.None);

    TransactionsGetListRequest request = new(new List<Guid> { groceriesAccount.Id }, 1, 25);

    List<TransactionsGetListResponse>? expected = _mapper.Map<List<TransactionsGetListResponse>>(sampleEntities
      .OrderBy(x => x.Date)
      .Take(1)
      .ToList());

    // act
    PaginatedList<TransactionsGetListResponse> result = await _sut.Handle(request, CancellationToken.None);

    // assert
    result.Items.Should().BeEquivalentTo(expected);
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(1);
    result.TotalCount.Should().Be(1);
    result.HasNextPage.Should().Be(false);
    result.HasPreviousPage.Should().Be(false);
  }
}
