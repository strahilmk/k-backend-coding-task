using System.Threading.Channels;
using Claims.Auditing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Claims.Tests.Auditing;

public class AuditerTests
{
    [Fact]
    public void AuditClaim_queues_a_claim_audit()
    {
        var channel = Channel.CreateUnbounded<IAuditEntry>();
        var auditer = new Auditer(channel, NullLogger<Auditer>.Instance);

        auditer.AuditClaim("claim-id", HttpRequestType.Post);

        Assert.True(channel.Reader.TryRead(out var entry));
        var audit = Assert.IsType<ClaimAudit>(entry);
        Assert.Equal("claim-id", audit.ClaimId);
        Assert.Equal(HttpRequestType.Post, audit.HttpRequestType);
        Assert.InRange(audit.Created, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
    }

    [Fact]
    public void AuditCover_queues_a_cover_audit()
    {
        var channel = Channel.CreateUnbounded<IAuditEntry>();
        var auditer = new Auditer(channel, NullLogger<Auditer>.Instance);

        auditer.AuditCover("cover-id", HttpRequestType.Delete);

        Assert.True(channel.Reader.TryRead(out var entry));
        var audit = Assert.IsType<CoverAudit>(entry);
        Assert.Equal("cover-id", audit.CoverId);
        Assert.Equal(HttpRequestType.Delete, audit.HttpRequestType);
        Assert.InRange(audit.Created, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow);
    }

    [Fact]
    public void Audit_is_dropped_without_throwing_when_the_queue_is_full()
    {
        var channel = Channel.CreateBounded<IAuditEntry>(1);
        var auditer = new Auditer(channel, NullLogger<Auditer>.Instance);
        auditer.AuditClaim("first", HttpRequestType.Post);

        auditer.AuditClaim("second", HttpRequestType.Post);

        Assert.Equal(1, channel.Reader.Count);
        Assert.True(channel.Reader.TryRead(out var entry));
        Assert.Equal("first", Assert.IsType<ClaimAudit>(entry).ClaimId);
    }
}
