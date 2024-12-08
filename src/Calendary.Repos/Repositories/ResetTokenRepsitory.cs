using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IResetTokenRepository : IRepository<ResetToken>
{
    Task<ResetToken?> GetByTokenAsync(string token);
}

public class ResetTokenRepository : IResetTokenRepository
{
    private readonly ICalendaryDbContext _context;

    public ResetTokenRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ResetToken>> GetAllAsync()
    {
        return await _context.ResetTokens.ToListAsync();
    }

    public async Task<ResetToken?> GetByIdAsync(int id)
    {
        return await _context.ResetTokens.FindAsync(id);
    }

    public async Task AddAsync(ResetToken entity)
    {
        await _context.ResetTokens.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ResetToken entity)
    {
        _context.ResetTokens.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.ResetTokens.FindAsync(id);
        if (entity is not null)
        {
            _context.ResetTokens.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ResetToken?> GetByTokenAsync(string token)
    {
        return await _context.ResetTokens.FirstOrDefaultAsync(rt => rt.Token == token);
    }
}