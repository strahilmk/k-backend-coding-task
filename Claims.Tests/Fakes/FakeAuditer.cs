using Claims.Auditing;

namespace Claims.Tests.Fakes;

/// <summary>Records each audit call instead of queuing it.</summary>
public class FakeAuditer : IAuditer
{
    public List<(string Id, HttpRequestType Type)> ClaimAudits { get; } = new();
    public List<(string Id, HttpRequestType Type)> CoverAudits { get; } = new();

    public void AuditClaim(string id, HttpRequestType httpRequestType) => ClaimAudits.Add((id, httpRequestType));

    public void AuditCover(string id, HttpRequestType httpRequestType) => CoverAudits.Add((id, httpRequestType));
}
