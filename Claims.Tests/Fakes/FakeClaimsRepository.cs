using Claims.Models;
using Claims.Repositories;

namespace Claims.Tests.Fakes;

/// <summary>Keeps claims in memory. Assigns an id on add, like EF does in ClaimsContext.</summary>
public class FakeClaimsRepository : IClaimsRepository
{
    public Dictionary<string, Claim> Claims { get; } = new();

    public Task<IEnumerable<Claim>> GetAllAsync() => Task.FromResult<IEnumerable<Claim>>(Claims.Values.ToList());

    public Task<Claim?> GetByIdAsync(string id) => Task.FromResult(Claims.GetValueOrDefault(id));

    public Task AddAsync(Claim claim)
    {
        claim.Id ??= Guid.NewGuid().ToString();
        Claims[claim.Id] = claim;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        Claims.Remove(id);
        return Task.CompletedTask;
    }
}
