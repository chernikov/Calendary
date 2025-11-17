using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);

    Task<User?> GetUserByIdentityAsync(Guid identity);

    Task AddRole(int userId, int roleId);
    Task<bool> ValidateEmailAsync(int id, string email);

    Task<IEnumerable<User>> GetAllForAdminAsync();
}

public class UserRepository : IUserRepository
{
    private readonly ICalendaryDbContext _context;

    public UserRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetUserByIdentityAsync(Guid identity)
    {
        return await _context.Users.FirstOrDefaultAsync(p => p.Identity == identity);    
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User entity)
    {
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User entity)
    {
        _context.Users.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Users.FindAsync(id);
        if (entity is not null)
        {
            _context.Users.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddRole(int userId, int roleId)
    {
        var userRole = new UserRole { RoleId = roleId, UserId = userId };
        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateEmailAsync(int id, string email)
    {
        return !(await _context.Users.AnyAsync(u => u.Id != id && u.Email == email && !u.IsTemporary));
    }

    public async Task<IEnumerable<User>> GetAllForAdminAsync()
    {
        return await _context.Users
            .Where(u => !u.IsTemporary && _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == Role.UserRole.Id))
            .ToListAsync();
    }

    
}
