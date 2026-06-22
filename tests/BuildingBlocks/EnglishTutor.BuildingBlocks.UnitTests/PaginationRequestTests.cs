using EnglishTutor.BuildingBlocks.Application.Pagination;
using FluentAssertions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class PaginationRequestTests
{
    // ===== Fix #3: divide-by-zero & negative inputs =====

    [Fact]
    public void SafePageSize_Should_Clamp_To_Minimum_One()
    {
        var req = new PaginationRequest { PageSize = 0 };
        req.SafePageSize.Should().Be(1);
    }

    [Fact]
    public void SafePageSize_Should_Clamp_To_Maximum_100()
    {
        var req = new PaginationRequest { PageSize = 5_000 };
        req.SafePageSize.Should().Be(100);
    }

    [Fact]
    public void SafePageSize_Should_Pass_Through_Valid_Value()
    {
        var req = new PaginationRequest { PageSize = 25 };
        req.SafePageSize.Should().Be(25);
    }

    [Fact]
    public void SafePageSize_Should_Handle_Negative_Input()
    {
        var req = new PaginationRequest { PageSize = -10 };
        req.SafePageSize.Should().Be(1);
    }

    [Fact]
    public void SafePage_Should_Clamp_To_Minimum_One()
    {
        var req = new PaginationRequest { Page = 0 };
        req.SafePage.Should().Be(1);
    }

    [Fact]
    public void SafePage_Should_Handle_Negative_Input()
    {
        var req = new PaginationRequest { Page = -5 };
        req.SafePage.Should().Be(1);
    }

    [Fact]
    public void Defaults_Should_Be_Safe()
    {
        var req = new PaginationRequest();
        req.SafePage.Should().Be(1);
        req.SafePageSize.Should().Be(20);
    }
}

public class PagedResultTests
{
    [Fact]
    public void TotalPages_Should_Not_Throw_When_PageSize_Is_Zero()
    {
        // Regression: PageSize=0 previously caused divide-by-zero.
        var result = new PagedResult<string> { Items = [], TotalCount = 100, Page = 1, PageSize = 0 };
        var act = () => _ = result.TotalPages;
        act.Should().NotThrow();
    }

    [Fact]
    public void TotalPages_Should_Return_Count_When_SafePageSize_Is_One()
    {
        // SafePageSize clamps PageSize=0 up to 1, so TotalPages = ceil(TotalCount / 1) = TotalCount.
        var result = new PagedResult<string> { Items = [], TotalCount = 100, Page = 1, PageSize = 0 };
        result.TotalPages.Should().Be(100);
    }

    [Fact]
    public void TotalPages_Should_Calculate_Correctly()
    {
        var result = new PagedResult<string> { Items = [], TotalCount = 95, Page = 1, PageSize = 20 };
        result.TotalPages.Should().Be(5); // ceil(95 / 20) = 5
    }

    [Fact]
    public void SafePageSize_Should_Guarantee_At_Least_One()
    {
        var result = new PagedResult<string> { Items = [], TotalCount = 0, Page = 1, PageSize = 0 };
        result.SafePageSize.Should().Be(1);
    }

    // ===== Additional hardening: max clamp + Page normalize in PagedResult =====

    [Fact]
    public void SafePageSize_Should_Clamp_To_Maximum_100()
    {
        // Guards against callers constructing PagedResult directly with a too-large size.
        var result = new PagedResult<string> { Items = [], TotalCount = 100, Page = 1, PageSize = 5_000 };
        result.SafePageSize.Should().Be(100);
    }

    [Fact]
    public void SafePage_Should_Clamp_Negative_Input_To_One()
    {
        var result = new PagedResult<string> { Items = [], TotalCount = 100, Page = -5, PageSize = 20 };
        result.SafePage.Should().Be(1);
    }

    [Fact]
    public void HasPreviousPage_Should_Be_False_When_SafePage_Is_One()
    {
        var result = new PagedResult<string> { Items = [], TotalCount = 100, Page = 0, PageSize = 20 };
        result.SafePage.Should().Be(1);
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_Should_Be_False_When_SafePage_Exceeds_TotalPages()
    {
        var result = new PagedResult<string> { Items = [], TotalCount = 10, Page = 999, PageSize = 20 };
        result.SafePage.Should().Be(999);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
    }
}
