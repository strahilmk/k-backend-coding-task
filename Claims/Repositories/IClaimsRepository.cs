using Claims.Models;

namespace Claims.Repositories;

public interface IClaimsRepository
{
    Task<IEnumerable<Claim>> GetAllAsync();
    Task<Claim?> GetByIdAsync(string id);
    Task AddAsync(Claim claim);
    Task DeleteAsync(string id);
}
