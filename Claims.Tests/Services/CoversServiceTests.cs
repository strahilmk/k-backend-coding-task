using Claims.Auditing;
using Claims.Dtos;
using Claims.Models;
using Claims.Services;
using Claims.Tests.Fakes;
using Xunit;

namespace Claims.Tests.Services;

// The premium formula itself is tested in PremiumTests.
public class CoversServiceTests
{
    private readonly FakeCoversRepository _covers = new();
    private readonly FakeAuditer _auditer = new();
    private readonly CoversService _service;

    public CoversServiceTests()
    {
        _service = new CoversService(_covers, _auditer);
    }

    [Fact]
    public async Task Created_cover_is_stored_with_its_computed_premium()
    {
        var request = Request();

        var cover = await _service.CreateAsync(request);

        var expected = _service.ComputePremium(request.StartDate, request.EndDate, request.Type);
        Assert.Equal(expected, cover.Premium);
        Assert.Equal(expected, _covers.Covers[cover.Id].Premium);
    }

    [Fact]
    public async Task Created_cover_is_audited_as_post()
    {
        var cover = await _service.CreateAsync(Request());

        Assert.Equal([(cover.Id, HttpRequestType.Post)], _auditer.CoverAudits);
    }

    [Fact]
    public async Task Deleted_cover_is_removed_and_audited_as_delete()
    {
        var cover = await _service.CreateAsync(Request());

        await _service.DeleteAsync(cover.Id);

        Assert.Empty(_covers.Covers);
        Assert.Equal((cover.Id, HttpRequestType.Delete), _auditer.CoverAudits.Last());
    }

    private static CreateCoverRequest Request() => new()
    {
        StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
        EndDate = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc),
        Type = CoverType.Tanker
    };
}
