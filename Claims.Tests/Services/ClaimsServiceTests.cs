using System.ComponentModel.DataAnnotations;
using Claims.Auditing;
using Claims.Dtos;
using Claims.Models;
using Claims.Services;
using Claims.Tests.Fakes;
using Xunit;

namespace Claims.Tests.Services;

public class ClaimsServiceTests
{
    private static readonly DateTime CoverStart = new(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CoverEnd = new(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    private readonly FakeClaimsRepository _claims = new();
    private readonly FakeCoversRepository _covers = new();
    private readonly FakeAuditer _auditer = new();
    private readonly ClaimsService _service;
    private readonly string _coverId = "cover-id";

    public ClaimsServiceTests()
    {
        _service = new ClaimsService(_claims, _covers, _auditer);
        _covers.Covers[_coverId] = new Cover { Id = _coverId, StartDate = CoverStart, EndDate = CoverEnd, Type = CoverType.Yacht };
    }

    [Fact]
    public async Task Claim_for_an_unknown_cover_is_rejected()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(Request("unknown-cover", CoverStart)));

        Assert.Contains(nameof(CreateClaimRequest.CoverId), exception.ValidationResult.MemberNames);
    }

    [Fact]
    public async Task Claim_created_the_day_before_the_cover_starts_is_rejected()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(Request(_coverId, CoverStart.AddDays(-1))));

        Assert.Contains(nameof(CreateClaimRequest.Created), exception.ValidationResult.MemberNames);
    }

    [Fact]
    public async Task Claim_created_the_day_after_the_cover_ends_is_rejected()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(Request(_coverId, CoverEnd.AddDays(1))));

        Assert.Contains(nameof(CreateClaimRequest.Created), exception.ValidationResult.MemberNames);
    }

    [Fact]
    public async Task Claim_created_on_the_cover_start_date_is_accepted()
    {
        var claim = await _service.CreateAsync(Request(_coverId, CoverStart));

        Assert.Equal(CoverStart, claim.Created);
    }

    [Fact]
    public async Task Claim_created_late_on_the_cover_end_date_is_accepted()
    {
        // Compared by date, so the time of day on the last day doesn't matter.
        var created = CoverEnd.AddHours(23);

        var claim = await _service.CreateAsync(Request(_coverId, created));

        Assert.Equal(created, claim.Created);
    }

    [Fact]
    public async Task Rejected_claim_is_not_stored_or_audited()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(Request(_coverId, CoverEnd.AddDays(1))));

        Assert.Empty(_claims.Claims);
        Assert.Empty(_auditer.ClaimAudits);
    }

    [Fact]
    public async Task Created_claim_is_stored_and_audited_as_post()
    {
        var claim = await _service.CreateAsync(Request(_coverId, CoverStart));

        Assert.True(_claims.Claims.ContainsKey(claim.Id));
        Assert.Equal([(claim.Id, HttpRequestType.Post)], _auditer.ClaimAudits);
    }

    [Fact]
    public async Task Deleted_claim_is_removed_and_audited_as_delete()
    {
        var claim = await _service.CreateAsync(Request(_coverId, CoverStart));

        await _service.DeleteAsync(claim.Id);

        Assert.Empty(_claims.Claims);
        Assert.Equal((claim.Id, HttpRequestType.Delete), _auditer.ClaimAudits.Last());
    }

    private static CreateClaimRequest Request(string coverId, DateTime created) => new()
    {
        CoverId = coverId,
        Created = created,
        Name = "name",
        Type = ClaimType.Fire,
        DamageCost = 1000m
    };
}
