using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IVerificationEmailCodeRepository : IRepository<VerificationEmailCode>
{
    Task<VerificationEmailCode?> GetLatestVerificationEmailCodeAsync(int userId);
    Task<VerificationEmailCode?> GetValidByUserIdAndCodeAsync(int userId, string code);
    Task MarkAsUsedAsync(int codeId);
}
public class VerificationEmailCodeRepository : IVerificationEmailCodeRepository
{
    private readonly ICalendaryDbContext _context;

    public VerificationEmailCodeRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VerificationEmailCode>> GetAllAsync()
    {
        return await _context.VerificationEmailCodes.ToListAsync();
    }

    public async Task<VerificationEmailCode?> GetByIdAsync(int id)
    {
        return await _context.VerificationEmailCodes.FindAsync(id);
    }

    public async Task AddAsync(VerificationEmailCode entity)
    {
        await _context.VerificationEmailCodes.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(VerificationEmailCode entity)
    {
        _context.VerificationEmailCodes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.VerificationEmailCodes.FindAsync(id);
        if (entity is not null)
        {
            _context.VerificationEmailCodes.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public Task<VerificationEmailCode?> GetValidByUserIdAndCodeAsync(int userId, string code)
        => _context.VerificationEmailCodes
            .FirstOrDefaultAsync(vc => vc.UserId == userId && vc.Code == code && !vc.IsUsed && vc.ExpiryDate > DateTime.UtcNow);

    public async Task MarkAsUsedAsync(int codeId)
    {
        var entity = await _context.VerificationEmailCodes.FindAsync(codeId);

        if (entity is not null)
        {
            entity.IsUsed = true;
            _context.VerificationEmailCodes.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<VerificationEmailCode?> GetLatestVerificationEmailCodeAsync(int userId)
    {
        return await _context.VerificationEmailCodes
        .Where(vc => vc.UserId == userId)
        .OrderByDescending(vc => vc.ExpiryDate)
        .FirstOrDefaultAsync();
    }
}
