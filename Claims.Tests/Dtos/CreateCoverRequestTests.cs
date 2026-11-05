using System.ComponentModel.DataAnnotations;
using Claims.Dtos;
using Claims.Models;
using Xunit;

namespace Claims.Tests.Dtos;

public class CreateCoverRequestTests
{
    private static readonly DateTime Today = DateTime.UtcNow.Date;

    [Fact]
    public void StartDate_yesterday_is_invalid()
    {
        var errors = Validate(Request(Today.AddDays(-1), Today.AddDays(30)));

        Assert.Contains(nameof(CreateCoverRequest.StartDate), errors);
    }

    [Fact]
    public void StartDate_today_is_valid()
    {
        var errors = Validate(Request(Today, Today.AddDays(30)));

        Assert.DoesNotContain(nameof(CreateCoverRequest.StartDate), errors);
    }

    [Fact]
    public void StartDate_in_the_future_is_valid()
    {
        var errors = Validate(Request(Today.AddDays(1), Today.AddDays(30)));

        Assert.DoesNotContain(nameof(CreateCoverRequest.StartDate), errors);
    }

    [Fact]
    public void Period_of_exactly_one_year_is_valid()
    {
        var start = Today.AddDays(1);

        var errors = Validate(Request(start, start.AddYears(1)));

        Assert.DoesNotContain(nameof(CreateCoverRequest.EndDate), errors);
    }

    [Fact]
    public void Period_longer_than_one_year_is_invalid()
    {
        var start = Today.AddDays(1);

        var errors = Validate(Request(start, start.AddYears(1).AddDays(1)));

        Assert.Contains(nameof(CreateCoverRequest.EndDate), errors);
    }

    [Fact]
    public void Period_of_one_year_crossing_29_February_is_valid()
    {
        // 366 days, because 2028 is a leap year.
        var start = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        var errors = Validate(Request(start, new DateTime(2028, 3, 1, 0, 0, 0, DateTimeKind.Utc)));

        Assert.DoesNotContain(nameof(CreateCoverRequest.EndDate), errors);
    }

    private static CreateCoverRequest Request(DateTime startDate, DateTime endDate) => new()
    {
        StartDate = startDate,
        EndDate = endDate,
        Type = CoverType.Yacht
    };

    // Returns the names of the fields that failed validation.
    private static List<string> Validate(CreateCoverRequest request) =>
        request.Validate(new ValidationContext(request))
            .SelectMany(result => result.MemberNames)
            .ToList();
}
