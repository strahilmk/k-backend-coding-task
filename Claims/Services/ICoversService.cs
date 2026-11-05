using Claims.Models;

namespace Claims.Services;

/// <summary>
/// Business logic for covers. Every create and delete is audited.
/// </summary>
public interface ICoversService
{
    Task<IEnumerable<Cover>> GetAllAsync();

    /// <returns>The cover, or null if no cover has this id.</returns>
    Task<Cover?> GetByIdAsync(string id);

    /// <summary>Assigns a new id to the cover, computes its premium and stores it.</summary>
    Task<Cover> CreateAsync(Cover cover);

    /// <summary>Deletes the cover. Does nothing if no cover has this id.</summary>
    Task DeleteAsync(string id);

    /// <summary>Computes the premium for a cover of this type over this period.</summary>
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}
