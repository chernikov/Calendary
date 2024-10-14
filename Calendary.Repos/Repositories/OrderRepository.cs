using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetOrderByStatusAsync(int userId, string status);
}

public class OrderRepository : IOrderRepository
{
    private readonly CalendaryDbContext _context;

    public OrderRepository(CalendaryDbContext context)
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
}
