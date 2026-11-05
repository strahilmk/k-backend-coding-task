using Claims.Dtos;

namespace Claims.Services;

/// <summary>
/// Business logic for claims. Every create and delete is audited.
/// </summary>
public interface IClaimsService
{
    Task<IEnumerable<ClaimResponse>> GetAllAsync();

    /// <returns>The claim, or null if no claim has this id.</returns>
    Task<ClaimResponse?> GetByIdAsync(string id);

    Task<ClaimResponse> CreateAsync(CreateClaimRequest request);

    /// <summary>Deletes the claim. Does nothing if no claim has this id.</summary>
    Task DeleteAsync(string id);
}
