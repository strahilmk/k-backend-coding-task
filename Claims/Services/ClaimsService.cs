using System.ComponentModel.DataAnnotations;
using Claims.Auditing;
using Claims.Dtos;
using Claims.Models;
using Claims.Repositories;

namespace Claims.Services;

public class ClaimsService : IClaimsService
{
    private readonly IClaimsRepository _claimsRepository;
    private readonly ICoversRepository _coversRepository;
    private readonly IAuditer _auditer;

    public ClaimsService(IClaimsRepository claimsRepository, ICoversRepository coversRepository, IAuditer auditer)
    {
        _claimsRepository = claimsRepository;
        _coversRepository = coversRepository;
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
        var cover = await _coversRepository.GetByIdAsync(request.CoverId);
        if (cover is null)
        {
            throw new ValidationException(
                new ValidationResult("The claim does not match any cover.", [nameof(request.CoverId)]), null, null);
        }

        // Compared by date, and both ends of the cover period are included.
        if (request.Created.Date < cover.StartDate.Date || request.Created.Date > cover.EndDate.Date)
        {
            throw new ValidationException(
                new ValidationResult(
                    $"The claim cannot be created outside its cover period ({cover.StartDate:yyyy-MM-dd} to {cover.EndDate:yyyy-MM-dd}).",
                    [nameof(request.Created)]),
                null, null);
        }

        var claim = new Claim
        {
            CoverId = request.CoverId,
            Created = request.Created,
            Name = request.Name,
            Type = request.Type,
            DamageCost = request.DamageCost
        };

        await _claimsRepository.AddAsync(claim);
        _auditer.AuditClaim(claim.Id, HttpRequestType.Post);
        return ClaimResponse.From(claim);
    }

    public async Task DeleteAsync(string id)
    {
        _auditer.AuditClaim(id, HttpRequestType.Delete);
        await _claimsRepository.DeleteAsync(id);
    }
}
