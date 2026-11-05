using Claims.Auditing;
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

    public Task<IEnumerable<Claim>> GetAllAsync()
    {
        return _claimsRepository.GetAllAsync();
    }

    public Task<Claim?> GetByIdAsync(string id)
    {
        return _claimsRepository.GetByIdAsync(id);
    }

    public async Task<Claim> CreateAsync(Claim claim)
    {
        claim.Id = Guid.NewGuid().ToString();
        await _claimsRepository.AddAsync(claim);
        _auditer.AuditClaim(claim.Id, "POST");
        return claim;
    }

    public async Task DeleteAsync(string id)
    {
        _auditer.AuditClaim(id, "DELETE");
        await _claimsRepository.DeleteAsync(id);
    }
}
