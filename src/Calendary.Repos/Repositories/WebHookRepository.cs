using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IWebHookRepository : IRepository<WebHook>
{ 
}

internal class WebHookRepository : IWebHookRepository
{
    private readonly ICalendaryDbContext _context;

    public WebHookRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<WebHook> GetByIdAsync(int id)
    {
        return await _context.WebHooks.FindAsync(id);
    }

    public async Task<IEnumerable<WebHook>> GetAllAsync()
    {
        return await _context.WebHooks.ToListAsync();
    }

    public async Task AddAsync(WebHook webHook)
    {
        await _context.WebHooks.AddAsync(webHook);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(WebHook webHook)
    {
        _context.WebHooks.Update(webHook);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var webHook = await _context.WebHooks.FindAsync(id);
        if (webHook != null)
        {
            _context.WebHooks.Remove(webHook);
            await _context.SaveChangesAsync();
        }
    }
}
