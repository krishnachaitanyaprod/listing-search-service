using ListingSearch.Core.Search;

namespace ListingSearch.UnitTests.Search;

public class PagerTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 5, 2)]
    [InlineData(12, 5, 3)]
    [InlineData(12, 50, 1)]
    public void TotalPages_IsTotalDividedByPageSize_RoundedUp(int totalCount, int pageSize, int expected)
    {
        Assert.Equal(expected, Pager.TotalPages(totalCount, pageSize));
    }

    [Theory]
    [InlineData(1, 0, true)]   // page 1 always exists, even with no results
    [InlineData(2, 0, false)]
    [InlineData(3, 12, true)]  // 12 results at 5 per page: page 3 is the last
    [InlineData(4, 12, false)]
    public void PageExists_OnlyUpToTheLastPage_ExceptPageOne(int page, int totalCount, bool expected)
    {
        Assert.Equal(expected, Pager.PageExists(page, totalCount, pageSize: 5));
    }

    [Fact]
    public void Page_ReturnsTheItemsForThatPage_AndTheTotals()
    {
        var items = Enumerable.Range(1, 12).ToList();

        var page = Pager.Page(items, page: 3, pageSize: 5);

        Assert.Equal(new[] { 11, 12 }, page.Items);
        Assert.Equal(3, page.Page);
        Assert.Equal(5, page.PageSize);
        Assert.Equal(12, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
    }
}
