using System.Text.Json;
using Claims.Json;
using Xunit;

namespace Claims.Tests.Json;

public class UtcDateTimeConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new UtcDateTimeConverter() } };

    [Fact]
    public void Date_without_a_time_zone_is_read_as_utc()
    {
        var date = JsonSerializer.Deserialize<DateTime>("\"2026-11-06T00:00:00\"", Options);

        Assert.Equal(new DateTime(2026, 11, 6, 0, 0, 0, DateTimeKind.Utc), date);
        Assert.Equal(DateTimeKind.Utc, date.Kind);
    }

    [Fact]
    public void Date_with_an_offset_is_converted_to_utc()
    {
        var date = JsonSerializer.Deserialize<DateTime>("\"2026-11-06T00:00:00+02:00\"", Options);

        Assert.Equal(new DateTime(2026, 11, 5, 22, 0, 0, DateTimeKind.Utc), date);
        Assert.Equal(DateTimeKind.Utc, date.Kind);
    }

    [Fact]
    public void Written_date_is_utc()
    {
        var json = JsonSerializer.Serialize(new DateTime(2026, 11, 6, 0, 0, 0, DateTimeKind.Unspecified), Options);

        Assert.Equal("\"2026-11-06T00:00:00Z\"", json);
    }
}
