using Claims.Models;

namespace Claims.Repositories;

/// <summary>
/// Stores covers in the MongoDB "covers" collection.
/// </summary>
public interface ICoversRepository
{
    Task<IEnumerable<Cover>> GetAllAsync();

    /// <returns>The cover, or null if no cover has this id.</returns>
    Task<Cover?> GetByIdAsync(string id);

    Task AddAsync(Cover cover);

    /// <summary>Deletes the cover. Does nothing if no cover has this id.</summary>
    Task DeleteAsync(string id);
}
