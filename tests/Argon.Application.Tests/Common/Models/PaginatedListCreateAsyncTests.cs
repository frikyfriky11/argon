using Argon.Application.Extensions;

namespace Argon.Application.Tests.Common.Models;

public class PaginatedListCreateAsyncTests
{
  [SetUp]
  public void SetUp()
  {
    _dbContext = DatabaseTestHelpers.GetInMemoryDbContext();
  }

  private IApplicationDbContext _dbContext = null!;

  private async Task SeedAccountsAsync(params string[] names)
  {
    foreach (string name in names)
    {
      await _dbContext.Accounts.AddAsync(new Account { Name = name });
    }

    await _dbContext.SaveChangesAsync(CancellationToken.None);
  }

  [Test]
  public async Task CreateAsync_ShouldReturnRequestedPage_WithCorrectSkipAndFlags()
  {
    // arrange — five accounts, ordered by name: A, B, C, D, E
    await SeedAccountsAsync("A", "B", "C", "D", "E");

    // act — page 2 of size 2 should skip A,B and return C,D
    PaginatedList<Account> result = await _dbContext.Accounts
      .OrderBy(account => account.Name)
      .PaginatedListAsync(pageNumber: 2, pageSize: 2, CancellationToken.None);

    // assert
    result.Items.Select(a => a.Name).Should().Equal("C", "D");
    result.PageNumber.Should().Be(2);
    result.TotalCount.Should().Be(5);
    result.TotalPages.Should().Be(3);
    result.HasPreviousPage.Should().BeTrue();
    result.HasNextPage.Should().BeTrue();
  }

  [Test]
  public async Task CreateAsync_ShouldReturnPartialLastPage_WithoutNextPage()
  {
    // arrange
    await SeedAccountsAsync("A", "B", "C", "D", "E");

    // act — page 3 of size 2 returns the single trailing item
    PaginatedList<Account> result = await _dbContext.Accounts
      .OrderBy(account => account.Name)
      .PaginatedListAsync(pageNumber: 3, pageSize: 2, CancellationToken.None);

    // assert
    result.Items.Select(a => a.Name).Should().Equal("E");
    result.HasNextPage.Should().BeFalse();
    result.HasPreviousPage.Should().BeTrue();
  }

  [Test]
  public async Task CreateAsync_ShouldReturnAllItemsOnASinglePage_WhenPageSizeIsMinusOne()
  {
    // arrange
    await SeedAccountsAsync("A", "B", "C", "D", "E");

    // act
    PaginatedList<Account> result = await _dbContext.Accounts
      .OrderBy(account => account.Name)
      .PaginatedListAsync(pageNumber: 1, pageSize: -1, CancellationToken.None);

    // assert
    result.Items.Should().HaveCount(5);
    result.TotalCount.Should().Be(5);
    result.PageNumber.Should().Be(1);
    result.TotalPages.Should().Be(1);
    result.HasNextPage.Should().BeFalse();
    result.HasPreviousPage.Should().BeFalse();
  }
}
