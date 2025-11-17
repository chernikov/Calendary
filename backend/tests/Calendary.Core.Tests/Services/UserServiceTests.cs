using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Moq;

namespace Calendary.Core.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserSettingRepository> _mockUserSettingRepository;
    private readonly Mock<IVerificationEmailCodeRepository> _mockVerificationEmailCodeRepository;
    private readonly Mock<IVerificationPhoneCodeRepository> _mockVerificationPhoneCodeRepository;
    private readonly Mock<IResetTokenRepository> _mockResetTokenRepository;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserSettingRepository = new Mock<IUserSettingRepository>();
        _mockVerificationEmailCodeRepository = new Mock<IVerificationEmailCodeRepository>();
        _mockVerificationPhoneCodeRepository = new Mock<IVerificationPhoneCodeRepository>();
        _mockResetTokenRepository = new Mock<IResetTokenRepository>();
    }

    private UserService CreateService()
    {
        return new UserService(
            _mockUserRepository.Object,
            _mockUserSettingRepository.Object,
            _mockVerificationEmailCodeRepository.Object,
            _mockVerificationPhoneCodeRepository.Object,
            _mockResetTokenRepository.Object);
    }

    private User CreateTestUser(int id = 1, string email = "test@example.com", string userName = "testuser")
    {
        return new User
        {
            Id = id,
            Email = email,
            UserName = userName,
            PasswordHash = "5f4dcc3b5aa765d61d8327deb882cf99", // MD5 hash of "password"
            IsEmailConfirmed = false,
            IsPhoneNumberConfirmed = false,
            IsTemporary = false,
            Identity = Guid.NewGuid(),
            CreatedByAdmin = false
        };
    }

    #region RegisterUserAsync Tests

    [Fact]
    public async Task RegisterUserAsync_NewUser_CreatesUserSuccessfully()
    {
        // Arrange
        var user = new User { Email = "new@example.com", UserName = "newuser" };
        var password = "password123";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(user.Email))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.AddRole(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        _mockUserSettingRepository.Setup(x => x.AddAsync(It.IsAny<UserSetting>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.RegisterUserAsync(user, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.UserName, result.UserName);
        Assert.False(result.IsEmailConfirmed);
        Assert.False(result.IsPhoneNumberConfirmed);
        Assert.NotNull(result.PasswordHash);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUserRepository.Verify(x => x.AddRole(It.IsAny<int>(), Role.UserRole.Id), Times.Once);
        _mockUserSettingRepository.Verify(x => x.AddAsync(It.IsAny<UserSetting>()), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_ExistingEmail_ThrowsException()
    {
        // Arrange
        var existingUser = CreateTestUser(email: "existing@example.com");
        var user = new User { Email = "existing@example.com", UserName = "newuser" };
        var password = "password123";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(user.Email))
            .ReturnsAsync(existingUser);

        var service = CreateService();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => service.RegisterUserAsync(user, password));
        Assert.Contains("already exists", exception.Message);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterUserAsync_CreatedByAdmin_SetsCorrectFlag()
    {
        // Arrange
        var user = new User
        {
            Email = "admin@example.com",
            UserName = "adminuser",
            CreatedByAdmin = true
        };
        var password = "password123";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(user.Email))
            .ReturnsAsync((User?)null);
        _mockUserRepository.Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockUserRepository.Setup(x => x.AddRole(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(Task.CompletedTask);
        _mockUserSettingRepository.Setup(x => x.AddAsync(It.IsAny<UserSetting>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.RegisterUserAsync(user, password);

        // Assert
        Assert.True(result.CreatedByAdmin);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";
        var user = CreateTestUser(email: email);

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(email, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "password";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(email, password);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrongpassword";
        var user = CreateTestUser(email: email);

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.LoginAsync(email, password);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetUserByEmailAsync Tests

    [Fact]
    public async Task GetUserByEmailAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = CreateTestUser(email: email);

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.GetUserByEmailAsync(email);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(email, result.Email);
    }

    [Fact]
    public async Task GetUserByEmailAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockUserRepository.Setup(x => x.GetUserByEmailAsync(email))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var result = await service.GetUserByEmailAsync(email);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var userId = 1;
        var user = CreateTestUser(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.GetUserByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = 999;

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var result = await service.GetUserByIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetUserByIdentityAsync Tests

    [Fact]
    public async Task GetUserByIdentityAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var identity = Guid.NewGuid();
        var user = CreateTestUser();
        user.Identity = identity;

        _mockUserRepository.Setup(x => x.GetUserByIdentityAsync(identity))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.GetUserByIdentityAsync(identity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(identity, result.Identity);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_UserExists_UpdatesUser()
    {
        // Arrange
        var userId = 1;
        var existingUser = CreateTestUser(userId);
        var updatedUser = new User
        {
            UserName = "updatedname",
            Email = "updated@example.com",
            PhoneNumber = "+380501234567"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(existingUser);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(userId, updatedUser);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updatedUser.UserName, result.UserName);
        Assert.Equal(updatedUser.Email, result.Email);
        Assert.Equal(updatedUser.PhoneNumber, result.PhoneNumber);
        _mockUserRepository.Verify(x => x.UpdateAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = 999;
        var updatedUser = new User { UserName = "updatedname" };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var result = await service.UpdateAsync(userId, updatedUser);

        // Assert
        Assert.Null(result);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region Verification Email Code Tests

    [Fact]
    public async Task CreateVerificationEmailCodeAsync_CreatesCode()
    {
        // Arrange
        var userId = 1;
        var code = "123456";
        var expiryDate = DateTime.UtcNow.AddHours(24);

        _mockVerificationEmailCodeRepository.Setup(x => x.AddAsync(It.IsAny<VerificationEmailCode>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.CreateVerificationEmailCodeAsync(userId, code, expiryDate);

        // Assert
        _mockVerificationEmailCodeRepository.Verify(
            x => x.AddAsync(It.Is<VerificationEmailCode>(
                v => v.UserId == userId &&
                     v.Code == code &&
                     v.ExpiryDate == expiryDate &&
                     v.IsUsed == false)),
            Times.Once);
    }

    [Fact]
    public async Task GetVerificationEmailCodeAsync_CodeExists_ReturnsCode()
    {
        // Arrange
        var userId = 1;
        var code = "123456";
        var verificationCode = new VerificationEmailCode
        {
            Id = 1,
            UserId = userId,
            Code = code,
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };

        _mockVerificationEmailCodeRepository
            .Setup(x => x.GetValidByUserIdAndCodeAsync(userId, code))
            .ReturnsAsync(verificationCode);

        var service = CreateService();

        // Act
        var result = await service.GetVerificationEmailCodeAsync(userId, code);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(code, result.Code);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task MarkEmailCodeAsUsedAsync_CallsRepository()
    {
        // Arrange
        var codeId = 1;

        _mockVerificationEmailCodeRepository.Setup(x => x.MarkAsUsedAsync(codeId))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.MarkEmailCodeAsUsedAsync(codeId);

        // Assert
        _mockVerificationEmailCodeRepository.Verify(x => x.MarkAsUsedAsync(codeId), Times.Once);
    }

    [Fact]
    public async Task GetLatestVerificationEmailCodeAsync_ReturnsLatestCode()
    {
        // Arrange
        var userId = 1;
        var latestCode = new VerificationEmailCode
        {
            Id = 2,
            UserId = userId,
            Code = "654321",
            ExpiryDate = DateTime.UtcNow.AddHours(24),
            IsUsed = false
        };

        _mockVerificationEmailCodeRepository
            .Setup(x => x.GetLatestVerificationEmailCodeAsync(userId))
            .ReturnsAsync(latestCode);

        var service = CreateService();

        // Act
        var result = await service.GetLatestVerificationEmailCodeAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("654321", result.Code);
    }

    #endregion

    #region Verification Phone Code Tests

    [Fact]
    public async Task CreateVerificationPhoneCodeAsync_CreatesCode()
    {
        // Arrange
        var userId = 1;
        var code = "123456";
        var expiryDate = DateTime.UtcNow.AddHours(1);

        _mockVerificationPhoneCodeRepository.Setup(x => x.AddAsync(It.IsAny<VerificationPhoneCode>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.CreateVerificationPhoneCodeAsync(userId, code, expiryDate);

        // Assert
        _mockVerificationPhoneCodeRepository.Verify(
            x => x.AddAsync(It.Is<VerificationPhoneCode>(
                v => v.UserId == userId &&
                     v.Code == code &&
                     v.ExpiryDate == expiryDate &&
                     v.IsUsed == false)),
            Times.Once);
    }

    [Fact]
    public async Task GetVerificationPhoneCodeAsync_CodeExists_ReturnsCode()
    {
        // Arrange
        var userId = 1;
        var code = "123456";
        var verificationCode = new VerificationPhoneCode
        {
            Id = 1,
            UserId = userId,
            Code = code,
            ExpiryDate = DateTime.UtcNow.AddHours(1),
            IsUsed = false
        };

        _mockVerificationPhoneCodeRepository
            .Setup(x => x.GetValidByUserIdAndCodeAsync(userId, code))
            .ReturnsAsync(verificationCode);

        var service = CreateService();

        // Act
        var result = await service.GetVerificationPhoneCodeAsync(userId, code);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(code, result.Code);
        Assert.Equal(userId, result.UserId);
    }

    [Fact]
    public async Task MarkPhoneCodeAsUsedAsync_CallsRepository()
    {
        // Arrange
        var codeId = 1;

        _mockVerificationPhoneCodeRepository.Setup(x => x.MarkAsUsedAsync(codeId))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.MarkPhoneCodeAsUsedAsync(codeId);

        // Assert
        _mockVerificationPhoneCodeRepository.Verify(x => x.MarkAsUsedAsync(codeId), Times.Once);
    }

    #endregion

    #region ConfirmUserEmailAsync Tests

    [Fact]
    public async Task ConfirmUserEmailAsync_UserExists_ConfirmsEmail()
    {
        // Arrange
        var userId = 1;
        var user = CreateTestUser(userId);
        user.IsEmailConfirmed = false;

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.ConfirmUserEmailAsync(userId);

        // Assert
        Assert.True(user.IsEmailConfirmed);
        _mockUserRepository.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ConfirmUserEmailAsync_UserDoesNotExist_DoesNothing()
    {
        // Arrange
        var userId = 999;

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        await service.ConfirmUserEmailAsync(userId);

        // Assert
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region ConfirmUserPhoneAsync Tests

    [Fact]
    public async Task ConfirmUserPhoneAsync_UserExists_ConfirmsPhone()
    {
        // Arrange
        var userId = 1;
        var user = CreateTestUser(userId);
        user.IsPhoneNumberConfirmed = false;

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        await service.ConfirmUserPhoneAsync(userId);

        // Assert
        Assert.True(user.IsPhoneNumberConfirmed);
        _mockUserRepository.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    #endregion

    #region ChangePasswordAsync Tests

    [Fact]
    public async Task ChangePasswordAsync_ValidCurrentPassword_ChangesPassword()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "password";
        var newPassword = "newpassword123";
        var user = CreateTestUser(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        Assert.True(result);
        Assert.NotEqual("5f4dcc3b5aa765d61d8327deb882cf99", user.PasswordHash); // Old hash
        _mockUserRepository.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_InvalidCurrentPassword_ReturnsFalse()
    {
        // Arrange
        var userId = 1;
        var currentPassword = "wrongpassword";
        var newPassword = "newpassword123";
        var user = CreateTestUser(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        var service = CreateService();

        // Act
        var result = await service.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        Assert.False(result);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        var currentPassword = "password";
        var newPassword = "newpassword123";

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var result = await service.ChangePasswordAsync(userId, currentPassword, newPassword);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region NewPasswordAsync Tests

    [Fact]
    public async Task NewPasswordAsync_UserExists_SetsNewPassword()
    {
        // Arrange
        var userId = 1;
        var newPassword = "newpassword123";
        var user = CreateTestUser(userId);
        var oldHash = user.PasswordHash;

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.NewPasswordAsync(userId, newPassword);

        // Assert
        Assert.True(result);
        Assert.NotEqual(oldHash, user.PasswordHash);
        _mockUserRepository.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task NewPasswordAsync_UserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var userId = 999;
        var newPassword = "newpassword123";

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var result = await service.NewPasswordAsync(userId, newPassword);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Reset Token Tests

    [Fact]
    public async Task CreateResetTokenAsync_UserExists_CreatesToken()
    {
        // Arrange
        var userId = 1;
        var user = CreateTestUser(userId);

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockResetTokenRepository.Setup(x => x.AddAsync(It.IsAny<ResetToken>()))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.CreateResetTokenAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
        Assert.Equal(userId, result.UserId);
        _mockResetTokenRepository.Verify(x => x.AddAsync(It.IsAny<ResetToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateResetTokenAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var userId = 999;

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        var service = CreateService();

        // Act
        var result = await service.CreateResetTokenAsync(userId);

        // Assert
        Assert.Null(result);
        _mockResetTokenRepository.Verify(x => x.AddAsync(It.IsAny<ResetToken>()), Times.Never);
    }

    [Fact]
    public async Task FindAndDeleteResetTokenAsync_TokenExists_ReturnsUserAndDeletesToken()
    {
        // Arrange
        var token = "test-token-123";
        var userId = 1;
        var resetToken = new ResetToken { Id = 1, Token = token, UserId = userId };
        var user = CreateTestUser(userId);

        _mockResetTokenRepository.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync(resetToken);
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockResetTokenRepository.Setup(x => x.DeleteAsync(resetToken.Id))
            .Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        var result = await service.FindAndDeleteResetTokenAsync(token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        _mockResetTokenRepository.Verify(x => x.DeleteAsync(resetToken.Id), Times.Once);
    }

    [Fact]
    public async Task FindAndDeleteResetTokenAsync_TokenDoesNotExist_ReturnsNull()
    {
        // Arrange
        var token = "nonexistent-token";

        _mockResetTokenRepository.Setup(x => x.GetByTokenAsync(token))
            .ReturnsAsync((ResetToken?)null);

        var service = CreateService();

        // Act
        var result = await service.FindAndDeleteResetTokenAsync(token);

        // Assert
        Assert.Null(result);
        _mockResetTokenRepository.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "user1@example.com"),
            CreateTestUser(2, "user2@example.com"),
            CreateTestUser(3, "user3@example.com")
        };

        _mockUserRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(users);

        var service = CreateService();

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllForAdminAsync_ReturnsAllUsersForAdmin()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser(1, "user1@example.com"),
            CreateTestUser(2, "user2@example.com")
        };

        _mockUserRepository.Setup(x => x.GetAllForAdminAsync())
            .ReturnsAsync(users);

        var service = CreateService();

        // Act
        var result = await service.GetAllForAdminAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    #endregion

    #region ValidateEmailAsync Tests

    [Fact]
    public async Task ValidateEmailAsync_CallsRepository()
    {
        // Arrange
        var userId = 1;
        var email = "test@example.com";

        _mockUserRepository.Setup(x => x.ValidateEmailAsync(userId, email))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var result = await service.ValidateEmailAsync(userId, email);

        // Assert
        Assert.True(result);
        _mockUserRepository.Verify(x => x.ValidateEmailAsync(userId, email), Times.Once);
    }

    #endregion
}
