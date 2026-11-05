using Claims.Auditing;
using Claims.Dtos;
using Claims.Models;
using Claims.Repositories;

namespace Claims.Services;

public class ClaimsService : IClaimsService
{
    private readonly IClaimsRepository _claimsRepository;
    private readonly IAuditer _auditer;

    public ClaimsService(IClaimsRepository claimsRepository, IAuditer auditer)
    {
        _claimsRepository = claimsRepository;
        _auditer = auditer;
    }

    public async Task<IEnumerable<ClaimResponse>> GetAllAsync()
    {
        var claims = await _claimsRepository.GetAllAsync();
        return claims.Select(ClaimResponse.From);
    }

    public async Task<ClaimResponse?> GetByIdAsync(string id)
    {
        var claim = await _claimsRepository.GetByIdAsync(id);
        return claim is null ? null : ClaimResponse.From(claim);
    }

    public async Task<ClaimResponse> CreateAsync(CreateClaimRequest request)
    {
        var claim = new Claim
        {
            CoverId = request.CoverId,
            Created = request.Created,
            Name = request.Name,
            Type = request.Type,
            DamageCost = request.DamageCost
        };

        await _claimsRepository.AddAsync(claim);
        _auditer.AuditClaim(claim.Id, "POST");
        return ClaimResponse.From(claim);
    }

    public async Task DeleteAsync(string id)
    {
        _auditer.AuditClaim(id, "DELETE");
        await _claimsRepository.DeleteAsync(id);
    }
}
