using Calendary.Application.Common;
using Calendary.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Admin.Users;

public record ListUsersQuery(int Page, int PageSize) : IRequest<PagedList<AdminUserSummary>>;

public class ListUsersQueryHandler(IAppDbContext db) : IRequestHandler<ListUsersQuery, PagedList<AdminUserSummary>>
{
    public async Task<PagedList<AdminUserSummary>> Handle(ListUsersQuery request, CancellationToken ct)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var total = await db.Users.CountAsync(ct);
        var items = await db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserSummary(
                u.Id, u.Email, u.DisplayName, u.Role.ToString(), u.AuthProvider.ToString(),
                u.EmailConfirmed, u.CreatedAtUtc, u.Orders.Count))
            .ToListAsync(ct);

        return new PagedList<AdminUserSummary>(items, total, page, pageSize);
    }
}
