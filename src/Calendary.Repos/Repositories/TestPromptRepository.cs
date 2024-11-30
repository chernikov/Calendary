using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;


public interface ITestPromptRepository : IRepository<TestPrompt>
{
    Task<IEnumerable<TestPrompt>> GetByPromptIdAsync(int idPrompt);
}

public class TestPromptRepository : ITestPromptRepository
{
    private readonly ICalendaryDbContext _context;

    public TestPromptRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TestPrompt entity)
    {
        await _context.TestPrompts.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.TestPrompts.FindAsync(id);
        if (entity != null)
        {
            _context.TestPrompts.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<TestPrompt>> GetAllAsync()
    {
        return await _context.TestPrompts.ToListAsync();
    }

    public async Task<TestPrompt?> GetByIdAsync(int id)
    {
        return await _context.TestPrompts.FindAsync(id);
    }

    public async Task<IEnumerable<TestPrompt>> GetByPromptIdAsync(int idPrompt)
    {
        return await _context.TestPrompts.Where(p => p.PromptId == idPrompt).ToListAsync();
    }

    public async Task UpdateAsync(TestPrompt entity)
    {
        _context.TestPrompts.Update(entity);
        await _context.SaveChangesAsync();
    }
}
