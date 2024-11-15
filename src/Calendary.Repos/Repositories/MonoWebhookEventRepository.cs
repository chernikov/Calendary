using Calendary.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Repos.Repositories;

public interface IMonoWebhookEventRepository : IRepository<MonoWebhookEvent>
{
}

public class MonoWebhookEventRepository : IMonoWebhookEventRepository
{
    private readonly ICalendaryDbContext _context;

    public MonoWebhookEventRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MonoWebhookEvent>> GetAllAsync()
    {
        return await _context.MonoWebhookEvents.ToListAsync();
    }

    public async Task<MonoWebhookEvent?> GetByIdAsync(int id)
    {
        return await _context.MonoWebhookEvents.FindAsync(id);
    }

    public async Task AddAsync(MonoWebhookEvent entity)
    {
        await _context.MonoWebhookEvents.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(MonoWebhookEvent entity)
    {
        _context.MonoWebhookEvents.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.MonoWebhookEvents.FindAsync(id);
        if (entity != null)
        {
            _context.MonoWebhookEvents.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
