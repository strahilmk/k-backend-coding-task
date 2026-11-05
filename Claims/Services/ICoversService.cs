using Claims.Dtos;
using Claims.Models;

namespace Claims.Services;

/// <summary>
/// Business logic for covers. Every create and delete is audited.
/// </summary>
public interface ICoversService
{
    Task<IEnumerable<CoverResponse>> GetAllAsync();

    /// <returns>The cover, or null if no cover has this id.</returns>
    Task<CoverResponse?> GetByIdAsync(string id);

    /// <summary>Computes the premium for the new cover and stores it.</summary>
    Task<CoverResponse> CreateAsync(CreateCoverRequest request);

    /// <summary>Deletes the cover. Does nothing if no cover has this id.</summary>
    Task DeleteAsync(string id);

    /// <summary>Computes the premium for a cover of this type over this period.</summary>
    decimal ComputePremium(DateTime startDate, DateTime endDate, CoverType coverType);
}
