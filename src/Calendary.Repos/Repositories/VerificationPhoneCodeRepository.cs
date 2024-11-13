using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IVerificationPhoneCodeRepository : IRepository<VerificationPhoneCode>
{
    Task<VerificationPhoneCode?> GetValidByUserIdAndCodeAsync(int userId, string code);


    Task<VerificationPhoneCode?> GetLatestVerificationPhoneCodeAsync(int userId);

    Task MarkAsUsedAsync(int codeId);
}

public class VerificationPhoneCodeRepository : IVerificationPhoneCodeRepository
{
    private readonly ICalendaryDbContext _context;

    public VerificationPhoneCodeRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VerificationPhoneCode>> GetAllAsync()
    {
        return await _context.VerificationPhoneCodes.ToListAsync();
    }

    public async Task<VerificationPhoneCode?> GetByIdAsync(int id)
    {
        return await _context.VerificationPhoneCodes.FindAsync(id);
    }

    public async Task AddAsync(VerificationPhoneCode entity)
    {
        await _context.VerificationPhoneCodes.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(VerificationPhoneCode entity)
    {
        _context.VerificationPhoneCodes.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.VerificationPhoneCodes.FindAsync(id);
        if (entity is not null)
        {
            _context.VerificationPhoneCodes.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public Task<VerificationPhoneCode?> GetValidByUserIdAndCodeAsync(int userId, string code)
     => _context.VerificationPhoneCodes
         .FirstOrDefaultAsync(vc => vc.UserId == userId && vc.Code == code && !vc.IsUsed && vc.ExpiryDate > DateTime.UtcNow);

    public async Task MarkAsUsedAsync(int codeId)
    {
        var entity = await _context.VerificationPhoneCodes.FindAsync(codeId);

        if (entity is not null)
        {
            entity.IsUsed = true;
            _context.VerificationPhoneCodes.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<VerificationPhoneCode?> GetLatestVerificationPhoneCodeAsync(int userId)
    {
        return await _context.VerificationPhoneCodes
        .Where(vc => vc.UserId == userId)
        .OrderByDescending(vc => vc.ExpiryDate)
        .FirstOrDefaultAsync();
    }
}
