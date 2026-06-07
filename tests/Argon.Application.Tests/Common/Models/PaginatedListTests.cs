namespace Argon.Application.Tests.Common.Models;

public class PaginatedListTests
{
  [Test]
  public void Constructor_ShouldReportZeroTotalPages_WhenTotalCountIsZero()
  {
    PaginatedList<int> list = new(new List<int>(), totalCount: 0, pageNumber: 1, pageSize: 25);

    list.TotalPages.Should().Be(0);
    list.HasNextPage.Should().BeFalse();
    list.HasPreviousPage.Should().BeFalse();
  }

  [Test]
  public void Constructor_ShouldReportZeroTotalPages_WhenPageSizeIsZero()
  {
    // The `pageSize == -1 fetch all` branch in CreateAsync passes count back as pageSize,
    // which becomes 0 when there are no items. The constructor must not divide by zero
    // (cast of NaN to int is int.MinValue).
    PaginatedList<int> list = new(new List<int>(), totalCount: 0, pageNumber: 1, pageSize: 0);

    list.TotalPages.Should().Be(0);
  }

  [Test]
  public void Constructor_ShouldRoundUpTotalPages_WithPartialLastPage()
  {
    PaginatedList<int> list = new(new List<int> { 1 }, totalCount: 51, pageNumber: 1, pageSize: 25);

    list.TotalPages.Should().Be(3);
  }
}
