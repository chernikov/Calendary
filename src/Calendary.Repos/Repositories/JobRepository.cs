using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IJobRepository : IRepository<Job>
{
}

public class JobRepository : IJobRepository
{
    private readonly ICalendaryDbContext _context;

    public JobRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Job>> GetAllAsync()
    {
        return await _context.Jobs.ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await _context.Jobs.FindAsync(id);
    }

    public async Task AddAsync(Job entity)
    {
        await _context.Jobs.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Job entity)
    {
        _context.Jobs.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Jobs.FindAsync(id);
        if (entity is not null)
        {
            _context.Jobs.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}