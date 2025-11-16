using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface ITemplateRepository : IRepository<Template>
{
    Task<(IEnumerable<Template> Items, int TotalCount)> GetPagedAsync(
        string? category = null,
        string? searchQuery = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "popularity"
    );
    Task<IEnumerable<string>> GetCategoriesAsync();
    Task<IEnumerable<Template>> GetFeaturedAsync(int count = 5);
}

public class TemplateRepository : ITemplateRepository
{
    private readonly ICalendaryDbContext _context;

    public TemplateRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Template>> GetAllAsync()
    {
        return await _context.Templates
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<Template?> GetByIdAsync(int id)
    {
        return await _context.Templates.FindAsync(id);
    }

    public async Task AddAsync(Template entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.Templates.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Template entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Templates.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Templates.FindAsync(id);
        if (entity is not null)
        {
            _context.Templates.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<(IEnumerable<Template> Items, int TotalCount)> GetPagedAsync(
        string? category = null,
        string? searchQuery = null,
        int page = 1,
        int pageSize = 20,
        string sortBy = "popularity"
    )
    {
        var query = _context.Templates.Where(t => t.IsActive);

        // Filter by category
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(t => t.Category == category);
        }

        // Search in Name and Description
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(t =>
                t.Name.Contains(searchQuery) ||
                t.Description.Contains(searchQuery)
            );
        }

        // Apply sorting
        query = sortBy.ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(t => t.Price),
            "price_desc" => query.OrderByDescending(t => t.Price),
            "newest" => query.OrderByDescending(t => t.CreatedAt),
            "name" => query.OrderBy(t => t.Name),
            _ => query.OrderBy(t => t.SortOrder) // default: popularity
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.Templates
            .Where(t => t.IsActive)
            .Select(t => t.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<IEnumerable<Template>> GetFeaturedAsync(int count = 5)
    {
        return await _context.Templates
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .Take(count)
            .ToListAsync();
    }
}
