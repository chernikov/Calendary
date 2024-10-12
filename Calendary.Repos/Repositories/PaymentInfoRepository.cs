using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public class PaymentInfoRepository : IRepository<PaymentInfo>
{
    private readonly ICalendaryDbContext _context;

    public PaymentInfoRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PaymentInfo>> GetAllAsync()
    {
        return await _context.PaymentInfos.ToListAsync();
    }

    public async Task<PaymentInfo?> GetByIdAsync(int id)
    {
        return await _context.PaymentInfos.FindAsync(id);
    }

    public async Task AddAsync(PaymentInfo entity)
    {
        await _context.PaymentInfos.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PaymentInfo entity)
    {
        _context.PaymentInfos.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.PaymentInfos.FindAsync(id);
        if (entity is not null)
        {
            _context.PaymentInfos.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
