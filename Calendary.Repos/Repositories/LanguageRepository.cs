using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public class LanguageRepository : IRepository<Language>
{
    private readonly ICalendaryDbContext _context;

    public LanguageRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Language>> GetAllAsync()
    {
        return await _context.Languages.ToListAsync();
    }

    public async Task<Language?> GetByIdAsync(int id)
    {
        return await _context.Languages.FindAsync(id);
    }

    public async Task AddAsync(Language entity)
    {
        await _context.Languages.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Language entity)
    {
        _context.Languages.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Languages.FindAsync(id);
        if (entity is not null)
        {
            _context.Languages.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
