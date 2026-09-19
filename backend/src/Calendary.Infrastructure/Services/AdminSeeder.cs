using Calendary.Domain.Entities;
using Calendary.Domain.Enums;
using Calendary.Infrastructure.Data;
using Calendary.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Calendary.Infrastructure.Services;

/// Guarantees an admin account always exists — no environment should ever need a manual DB
/// UPDATE to get admin access. Config (AdminSeed:Password, i.e. the ADMIN_PASSWORD/
/// ADMIN_PASSWORD_STAGING env vars) is the source of truth: every startup re-hashes and writes
/// it, so rotating the secret and redeploying is enough to change the live password — no DB
/// access needed either way. Runs at startup (Program.cs), right after db.Database.Migrate().
public static class AdminSeeder
{
    public static async Task EnsureAdminUserAsync(
        AppDbContext db, AdminSeedOptions options, ILogger logger, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(options.Password))
        {
            logger.LogWarning("AdminSeed:Password is not configured — skipping admin user seed.");
            return;
        }

        var email = options.Email.Trim().ToLowerInvariant();
        var admin = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        var isNew = admin is null;
        admin ??= new User { Email = email, DisplayName = "Admin", AuthProvider = AuthProvider.Password };

        admin.Role = UserRole.Admin;
        admin.EmailConfirmed = true;
        admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, options.Password);

        if (isNew)
        {
            db.Users.Add(admin);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation(isNew ? "Seeded admin user {Email}" : "Synced admin user {Email}", email);
    }
}
