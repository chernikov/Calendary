using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IJobTaskRepository : IRepository<JobTask>
{
    Task AddRangeAsync(List<JobTask> jobTasks);
    Task<JobTask?> GetByIdWithPromptAsync(int jobTaskId);
}

public class JobTaskRepository : IJobTaskRepository
{
    private readonly ICalendaryDbContext _context;

    public JobTaskRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<JobTask>> GetAllAsync()
    {
        return await _context.JobTasks.ToListAsync();
    }

    public async Task<JobTask?> GetByIdAsync(int id)
    {
        return await _context.JobTasks.FindAsync(id);
    }

    public async Task AddAsync(JobTask entity)
    {
        await _context.JobTasks.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(JobTask entity)
    {
        _context.JobTasks.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.JobTasks.FindAsync(id);
        if (entity is not null)
        {
            _context.JobTasks.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddRangeAsync(List<JobTask> jobTasks)
    {
        await _context.JobTasks.AddRangeAsync(jobTasks);
        await _context.SaveChangesAsync();
    }

    public Task<JobTask?> GetByIdWithPromptAsync(int jobTaskId)
        => _context.JobTasks
            .Include(x => x.Prompt)
            .FirstOrDefaultAsync(x => x.Id == jobTaskId);
}
