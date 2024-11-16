using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderByStatusAsync(int userId, string status);
    Task<Order?> GetFullOrderByStatusAsync(int userId, string status);
    Task<Order?> GetFullOrderAsync(int orderId);
    Task<Order?> GetOrderWithItemsAsync(int userId, string status);

    Task<(List<Order>, int)> GetOrdersWithPagingAsync(int userId, int page, int pageSize);
}

public class OrderRepository : IOrderRepository
{
    private readonly ICalendaryDbContext _context;

    public OrderRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders.ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders.FindAsync(id);
    }

    public async Task AddAsync(Order entity)
    {
        await _context.Orders.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order entity)
    {
        _context.Orders.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Orders.FindAsync(id);
        if (entity != null)
        {
            _context.Orders.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public Task<Order?> GetOrderByStatusAsync(int userId, string status) 
        => _context.Orders.FirstOrDefaultAsync(o => o.UserId == userId && o.Status == status);


    private IQueryable<Order> IncludeOrderDetails(IQueryable<Order> query)
        => query
            .Include(p => p.OrderItems)
                .ThenInclude(p => p.Calendar)
                    .ThenInclude(p => p.Language)
            .Include(p => p.OrderItems)
                .ThenInclude(p => p.Calendar)
                    .ThenInclude(p => p.CalendarHolidays)
                        .ThenInclude(p => p.Holiday)
            .Include(p => p.OrderItems)
                .ThenInclude(p => p.Calendar)
                    .ThenInclude(p => p.EventDates)
            .Include(p => p.OrderItems)
                .ThenInclude(p => p.Calendar)
                    .ThenInclude(p => p.Images);

    public Task<Order?> GetFullOrderByStatusAsync(int userId, string status)
        => IncludeOrderDetails(_context.Orders)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == status);

    public Task<Order?> GetOrderWithItemsAsync(int userId, string status)
        => _context.Orders
            .Include(p => p.OrderItems)
                .ThenInclude(p => p.Calendar)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == status);

    public Task<Order?> GetFullOrderAsync(int orderId)
        => IncludeOrderDetails(_context.Orders)
            .FirstOrDefaultAsync(o => o.Id == orderId);

    public async Task<(List<Order>, int)> GetOrdersWithPagingAsync(int userId, int page, int pageSize)
    {
        var query = _context.Orders
            .Where(o => o.UserId == userId);
        var fullQuery = IncludeOrderDetails(query);

        var totalOrders = await query.CountAsync();
        var orders = await fullQuery
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (orders, totalOrders);
    }
}
