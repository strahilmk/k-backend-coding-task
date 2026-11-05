using Claims.Data;
using Claims.Models;
using Microsoft.EntityFrameworkCore;

namespace Claims.Repositories;

public class ClaimsRepository : IClaimsRepository
{
    private readonly ClaimsContext _context;

    public ClaimsRepository(ClaimsContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Claim>> GetAllAsync()
    {
        return await _context.Claims.ToListAsync();
    }

    public async Task<Claim?> GetByIdAsync(string id)
    {
        return await _context.Claims
            .Where(claim => claim.Id == id)
            .SingleOrDefaultAsync();
    }

    public async Task AddAsync(Claim claim)
    {
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var claim = await GetByIdAsync(id);
        if (claim is not null)
        {
            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
        }
    }
}
