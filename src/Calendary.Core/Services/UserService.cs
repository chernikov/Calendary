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
    Task<VerificationEmailCode?> GetLatestVerificationEmailCodeAsync(int userId);
    Task ConfirmUserEmailAsync(int userId);
    Task<VerificationPhoneCode?> GetLatestVerificationPhoneCodeAsync(int userId);
    Task ConfirmUserPhoneAsync(int userId);
    Task<bool> ChangePasswordAsync(int id, string currentPassword, string newPassword);
    Task<bool> NewPasswordAsync(int id, string newPassword);
    Task<ResetToken?> CreateResetTokenAsync(int userId);
    Task<User?> FindAndDeleteResetTokenAsync(string token);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetAllForAdminAsync();
}

public class UserService(IUserRepository userRepository,
    IUserSettingRepository userSettingRepository,
    IVerificationEmailCodeRepository verificationEmailCodeRepository,
    IVerificationPhoneCodeRepository verificationPhoneCodeRepository,
    IResetTokenRepository resetTokenRepository) : IUserService
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

        var newUser = new User
        {
            Email = user.Email,
            UserName = user.UserName,
            PasswordHash = hashPassword,
            IsEmailConfirmed = false,
            IsPhoneNumberConfirmed = false,
            IsTemporary = false,
            CreatedByAdmin = user.CreatedByAdmin   
        };

        await userRepository.AddAsync(newUser);
        await userRepository.AddRole(newUser.Id, Role.UserRole.Id);
        await userSettingRepository.AddAsync(new UserSetting { UserId = newUser.Id });
        return newUser;
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

    public Task<VerificationPhoneCode?> GetLatestVerificationPhoneCodeAsync(int userId)
         => verificationPhoneCodeRepository.GetLatestVerificationPhoneCodeAsync(userId);

    public async Task ConfirmUserPhoneAsync(int userId)
    {
        var entity = await userRepository.GetByIdAsync(userId);
        if (entity is not null)
        {
            entity.IsPhoneNumberConfirmed = true;
            await userRepository.UpdateAsync(entity);
        }
    }

    public async Task<bool> ChangePasswordAsync(int id, string currentPassword, string newPassword)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return false;
        }

        var hashPassword = GetMd5Hash(currentPassword);
        if (user.PasswordHash != hashPassword)
        {
            return false;
        }
        var newHashPassword = GetMd5Hash(newPassword);

        user.PasswordHash = newHashPassword;
        await userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> NewPasswordAsync(int id, string newPassword)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return false;
        }
        var newHashPassword = GetMd5Hash(newPassword);
        user.PasswordHash = newHashPassword;
        await userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<ResetToken?> CreateResetTokenAsync(int userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return null;
        }
        var token = GenerateToken();

        var resetToken = new ResetToken
        {
            Token = token,
            UserId = user.Id
        };

        await resetTokenRepository.AddAsync(resetToken);
        return resetToken;
    }

    public async Task<User?> FindAndDeleteResetTokenAsync(string token)
    {
        var resetToken = await resetTokenRepository.GetByTokenAsync(token);
        if (resetToken is null)
        {
            return null;
        }
        var user = await userRepository.GetByIdAsync(resetToken.UserId);

        await resetTokenRepository.DeleteAsync(resetToken.Id);
        return user;
    }

    private string GenerateToken()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 20)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

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

    public Task<IEnumerable<User>> GetAllAsync()
        => userRepository.GetAllAsync();

    public Task<IEnumerable<User>> GetAllForAdminAsync()
           => userRepository.GetAllForAdminAsync();
}
