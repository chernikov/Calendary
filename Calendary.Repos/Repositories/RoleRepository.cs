using Calendary.Model;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Repos.Repositories;


public interface IRoleRepository 
{


    Task<IEnumerable<Role>> GetAllAsync();
}
public class RoleRepository : IRoleRepository
{
    private readonly ICalendaryDbContext _context;

    public RoleRepository(ICalendaryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Role>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }
}
