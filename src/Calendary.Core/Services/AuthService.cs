using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Calendary.Core.Services;

public interface IAuthService
{
    Task<string> GenerateJwtTokenAsync(User user);
}

public class AuthService(IRoleRepository roleRepository, IConfiguration configuration) : IAuthService
{
    public async Task<string> GenerateJwtTokenAsync(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var roles = await roleRepository.GetRolesByUserIdAsync(user.Id);
        List<Claim> claims = [
 
            new Claim(JwtRegisteredClaimNames.Jti, user.Identity.ToString())
        ];

        if (user.Email is not null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
