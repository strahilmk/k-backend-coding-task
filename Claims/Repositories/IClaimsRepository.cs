using Claims.Models;

namespace Claims.Repositories;

/// <summary>
/// Stores claims in the MongoDB "claims" collection.
/// </summary>
public interface IClaimsRepository
{
    Task<IEnumerable<Claim>> GetAllAsync();

    /// <returns>The claim, or null if no claim has this id.</returns>
    Task<Claim?> GetByIdAsync(string id);

    Task AddAsync(Claim claim);

    /// <summary>Deletes the claim. Does nothing if no claim has this id.</summary>
    Task DeleteAsync(string id);
}
