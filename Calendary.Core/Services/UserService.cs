using Calendary.Model;
using Calendary.Repos.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace Calendary.Core.Services;
public interface IUserService
{
    Task<User> RegisterUserAsync(User user, string password);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> LoginAsync(string email, string password);
    Task<User?> UpdateAsync(int userId, User entity);

    Task<User?> GetUserByIdentityAsync(Guid userIdentity);

    Task CreateVerificationEmailCodeAsync(int userId, string code, DateTime expiryDate);

    Task CreateVerificationPhoneCodeAsync(int userId, string code, DateTime expiryDate);

    Task<VerificationEmailCode?> GetVerificationEmailCodeAsync(int userId, string code);

    Task<VerificationPhoneCode?> GetVerificationPhoneCodeAsync(int userId, string code);

    Task MarkEmailCodeAsUsedAsync(int codeId);

    Task MarkPhoneCodeAsUsedAsync(int codeId);

    Task<bool> ValidateEmailAsync(int id, string email);
    Task<VerificationEmailCode?> GetLatestVerificationEmailCodeAsync(int id);
    Task ConfirmUserEmailAsync(int userId);
}

public class UserService(IUserRepository userRepository,
    IUserSettingRepository userSettingRepository,
    IVerificationEmailCodeRepository verificationEmailCodeRepository,
    IVerificationPhoneCodeRepository verificationPhoneCodeRepository) : IUserService
{
    public async Task<User> RegisterUserAsync(User user, string password)
    {
        if (user.Email is not null)
        {
            var entity = await userRepository.GetUserByEmailAsync(user.Email);
            if (entity is not null)
            {
                throw new Exception("User with this email already exists.");
            }
        }

        var hashPassword = GetMd5Hash(password);

        user = new User
        {
            Email = user.Email,
            UserName = user.UserName,
            PasswordHash = hashPassword,
            IsEmailConfirmed = false,
            IsPhoneNumberConfirmed = false,
            IsTemporary = user.IsTemporary

        };

        await userRepository.AddAsync(user);
        await userRepository.AddRole(user.Id, Role.UserRole.Id);
        await userSettingRepository.AddAsync(new UserSetting { UserId = user.Id });
        return user;
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await userRepository.GetUserByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var hashPassword = GetMd5Hash(password);
        if (user.PasswordHash != hashPassword)
        {
            return null;
        }
        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
        => await userRepository.GetUserByEmailAsync(email);

    public async Task CreateVerificationEmailCodeAsync(int userId, string code, DateTime expiryDate)
    {
        var verificationCode = new VerificationEmailCode
        {
            UserId = userId,
            Code = code,
            ExpiryDate = expiryDate,
            IsUsed = false
        };

        await verificationEmailCodeRepository.AddAsync(verificationCode);
    }

    public async Task CreateVerificationPhoneCodeAsync(int userId, string code, DateTime expiryDate)
    {
        var verificationCode = new VerificationPhoneCode
        {
            UserId = userId,
            Code = code,
            ExpiryDate = expiryDate,
            IsUsed = false
        };

        await verificationPhoneCodeRepository.AddAsync(verificationCode);
    }

    public Task<VerificationEmailCode?> GetVerificationEmailCodeAsync(int userId, string code)
        => verificationEmailCodeRepository.GetValidByUserIdAndCodeAsync(userId, code);

    public Task MarkEmailCodeAsUsedAsync(int codeId)
        => verificationEmailCodeRepository.MarkAsUsedAsync(codeId);

    public Task<VerificationPhoneCode?> GetVerificationPhoneCodeAsync(int userId, string code)
        => verificationPhoneCodeRepository.GetValidByUserIdAndCodeAsync(userId, code);

    public Task MarkPhoneCodeAsUsedAsync(int codeId)
        => verificationPhoneCodeRepository.MarkAsUsedAsync(codeId);


    public async Task<User?> UpdateAsync(int userId, User entity)
    {
        var dbUser = await userRepository.GetByIdAsync(userId);
        if (dbUser is null)
        {
            return null;
        }
        dbUser.UserName = entity.UserName;
        dbUser.Email = entity.Email;
        dbUser.PhoneNumber = entity.PhoneNumber;
        await userRepository.UpdateAsync(dbUser);
        return dbUser;
    }

    public Task<User?> GetUserByIdentityAsync(Guid userIdentity)
       => userRepository.GetByIdentityAsync(userIdentity);


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

    public Task<bool> ValidateEmailAsync(int id, string email)
        => userRepository.ValidateEmailAsync(id, email);

    public Task<VerificationEmailCode?> GetLatestVerificationEmailCodeAsync(int userId)
        => verificationEmailCodeRepository.GetLatestVerificationEmailCodeAsync(userId);

    public async Task ConfirmUserEmailAsync(int userId)
    {
        var entity = await userRepository.GetByIdAsync(userId);
        if (entity is not null)
        {
            entity.IsEmailConfirmed = true;
            await userRepository.UpdateAsync(entity);
        }
    }
}
