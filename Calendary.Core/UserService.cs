using Calendary.Model;
using Calendary.Repos.Repositories;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Cryptography;
using System.Text;

namespace Calendary.Core;
public interface IUserService
{
    Task RegisterUserAsync(User user, string password);
    Task<User?> GetUserByEmailAsync(string email);
}

public class UserService(IUserRepository userRepository, IRoleRepository roleRepository) : IUserService
{
    

    public async Task RegisterUserAsync(User user, string password)
    {
        var entity = await userRepository.GetUserByEmailAsync(user.Email);
        if (entity is not null)
        {
            throw new Exception("User with this email already exists.");
        }

        var hashPassword = GetMd5Hash(password);

        user = new User
        {
            Email = user.Email,
            UserName = user.UserName,
            PasswordHash = hashPassword,
            IsEmailConfirmed = false,
            IsPhoneNumberConfirmed = false,

        };

        await userRepository.AddAsync(user);
        await userRepository.AddRole(user.Id, Role.UserRole.Id);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
        => await userRepository.GetUserByEmailAsync(email);

    private static string GetMd5Hash(string password)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashBytes = md5.ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("x2"));
        }
        return sb.ToString();
    }
}
