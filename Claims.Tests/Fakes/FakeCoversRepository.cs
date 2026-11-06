using Claims.Models;
using Claims.Repositories;

namespace Claims.Tests.Fakes;

/// <summary>Keeps covers in memory. Assigns an id on add, like EF does in ClaimsContext.</summary>
public class FakeCoversRepository : ICoversRepository
{
    public Dictionary<string, Cover> Covers { get; } = new();

    public Task<IEnumerable<Cover>> GetAllAsync() => Task.FromResult<IEnumerable<Cover>>(Covers.Values.ToList());

    public Task<Cover?> GetByIdAsync(string id) => Task.FromResult(Covers.GetValueOrDefault(id));

    public Task AddAsync(Cover cover)
    {
        cover.Id ??= Guid.NewGuid().ToString();
        Covers[cover.Id] = cover;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        Covers.Remove(id);
        return Task.CompletedTask;
    }
}
