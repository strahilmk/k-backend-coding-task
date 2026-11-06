using Claims.Models;
using Claims.Services;
using Claims.Tests.Fakes;
using Xunit;

namespace Claims.Tests.Services;

// Expected values are worked out by hand from the rules in docs/README.md (Task 5):
// - base day rate 1250, times 1.1 for Yacht, 1.2 for PassengerShip, 1.5 for Tanker and 1.3 for other types
// - the first 30 days are not discounted
// - the next 150 days are discounted by 5% for Yacht and 2% for other types
// - the remaining days by an additional 3% for Yacht (8% in total) and 1% for other types (3% in total)
// - only whole days are charged (our decision, see docs/pr-notes.md)
public class PremiumTests
{
    private static readonly DateTime Start = new(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly CoversService _service = new(new FakeCoversRepository(), new FakeAuditer());

    public static TheoryData<CoverType, decimal> DayRates => new()
    {
        { CoverType.Yacht, 1375m },         // 1250 * 1.1
        { CoverType.PassengerShip, 1500m }, // 1250 * 1.2
        { CoverType.Tanker, 1875m },        // 1250 * 1.5
        { CoverType.ContainerShip, 1625m }, // 1250 * 1.3
        { CoverType.BulkCarrier, 1625m }    // 1250 * 1.3
    };

    [Theory]
    [MemberData(nameof(DayRates))]
    public void One_day_costs_the_day_rate_of_the_cover_type(CoverType type, decimal dayRate)
    {
        Assert.Equal(dayRate, Premium(type, days: 1));
    }

    // Yacht: 1375 a day, 1306.25 with 5% off, 1265 with 8% off.
    // Tanker: 1875 a day, 1837.50 with 2% off, 1818.75 with 3% off.
    public static TheoryData<CoverType, int, decimal> Periods => new()
    {
        { CoverType.Yacht, 30, 41_250m },        // 30 * 1375
        { CoverType.Yacht, 31, 42_556.25m },     // 41250 + 1306.25
        { CoverType.Yacht, 180, 237_187.50m },   // 41250 + 150 * 1306.25
        { CoverType.Yacht, 181, 238_452.50m },   // 237187.50 + 1265
        { CoverType.Yacht, 365, 471_212.50m },   // 237187.50 + 185 * 1265
        { CoverType.Yacht, 366, 472_477.50m },   // 471212.50 + 1265, a leap-year period
        { CoverType.Tanker, 30, 56_250m },       // 30 * 1875
        { CoverType.Tanker, 31, 58_087.50m },    // 56250 + 1837.50
        { CoverType.Tanker, 180, 331_875m },     // 56250 + 150 * 1837.50
        { CoverType.Tanker, 181, 333_693.75m },  // 331875 + 1818.75
        { CoverType.Tanker, 365, 668_343.75m }   // 331875 + 185 * 1818.75
    };

    [Theory]
    [MemberData(nameof(Periods))]
    public void Premium_is_discounted_progressively_over_the_period(CoverType type, int days, decimal expected)
    {
        Assert.Equal(expected, Premium(type, days));
    }

    [Fact]
    public void Partial_last_day_is_not_charged()
    {
        var premium = _service.ComputePremium(Start, Start.AddDays(30).AddHours(12), CoverType.Yacht);

        Assert.Equal(41_250m, premium);
    }

    [Fact]
    public void Period_shorter_than_one_day_costs_nothing()
    {
        var premium = _service.ComputePremium(Start, Start.AddHours(12), CoverType.Yacht);

        Assert.Equal(0m, premium);
    }

    private decimal Premium(CoverType type, int days) =>
        _service.ComputePremium(Start, Start.AddDays(days), type);
}
