using Claims.Models;

namespace Claims.Repositories;

public interface ICoversRepository
{
    Task<IEnumerable<Cover>> GetAllAsync();
    Task<Cover?> GetByIdAsync(string id);
    Task AddAsync(Cover cover);
    Task DeleteAsync(string id);
}
