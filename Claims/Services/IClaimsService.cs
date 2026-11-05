using Claims.Models;

namespace Claims.Services;

/// <summary>
/// Business logic for claims. Every create and delete is audited.
/// </summary>
public interface IClaimsService
{
    Task<IEnumerable<Claim>> GetAllAsync();

    /// <returns>The claim, or null if no claim has this id.</returns>
    Task<Claim?> GetByIdAsync(string id);

    /// <summary>Assigns a new id to the claim and stores it.</summary>
    Task<Claim> CreateAsync(Claim claim);

    /// <summary>Deletes the claim. Does nothing if no claim has this id.</summary>
    Task DeleteAsync(string id);
}
