using Calendary.Application.Common;
using Calendary.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Calendary.Application.Auth;

public record GetCurrentUserQuery(Guid UserId) : IRequest<User?>;

public class GetCurrentUserQueryHandler(IAppDbContext db) : IRequestHandler<GetCurrentUserQuery, User?>
{
    public Task<User?> Handle(GetCurrentUserQuery request, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, ct);
}
