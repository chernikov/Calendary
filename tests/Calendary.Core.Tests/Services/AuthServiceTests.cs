using Calendary.Core.Services;
using Calendary.Model;
using Calendary.Repos.Repositories;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Calendary.Core.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public AuthServiceTests()
    {
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration with required JWT settings
        _mockConfiguration.Setup(x => x["Jwt:Key"])
            .Returns("ThisIsAVerySecureSecretKeyForJWTTokenGenerationWithAtLeast32Characters");
        _mockConfiguration.Setup(x => x["Jwt:Issuer"])
            .Returns("calendary.com.ua");
        _mockConfiguration.Setup(x => x["Jwt:Audience"])
            .Returns("calendary-users");
    }

    private AuthService CreateService()
    {
        return new AuthService(_mockRoleRepository.Object, _mockConfiguration.Object);
    }

    private User CreateTestUser(int id = 1, string email = "test@example.com", Guid? identity = null)
    {
        return new User
        {
            Id = id,
            Email = email,
            UserName = "testuser",
            Identity = identity ?? Guid.NewGuid(),
            IsEmailConfirmed = true,
            IsPhoneNumberConfirmed = false
        };
    }

    #region GenerateJwtTokenAsync Tests

    [Fact]
    public async Task GenerateJwtTokenAsync_ValidUser_GeneratesToken()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new List<Role>
        {
            new Role { Id = 1, Name = "User" }
        };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_TokenContainsUserIdentity()
    {
        // Arrange
        var identity = Guid.NewGuid();
        var user = CreateTestUser(identity: identity);
        var roles = new List<Role> { new Role { Id = 1, Name = "User" } };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);

        Assert.NotNull(jtiClaim);
        Assert.Equal(identity.ToString(), jtiClaim.Value);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_TokenContainsEmail()
    {
        // Arrange
        var email = "testuser@example.com";
        var user = CreateTestUser(email: email);
        var roles = new List<Role> { new Role { Id = 1, Name = "User" } };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);

        Assert.NotNull(emailClaim);
        Assert.Equal(email, emailClaim.Value);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_TokenContainsSubject()
    {
        // Arrange
        var email = "testuser@example.com";
        var user = CreateTestUser(email: email);
        var roles = new List<Role> { new Role { Id = 1, Name = "User" } };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

        Assert.NotNull(subClaim);
        Assert.Equal(email, subClaim.Value);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_TokenContainsRoles()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new List<Role>
        {
            new Role { Id = 1, Name = "User" },
            new Role { Id = 2, Name = "Admin" }
        };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

        Assert.Equal(2, roleClaims.Count);
        Assert.Contains(roleClaims, c => c.Value == "User");
        Assert.Contains(roleClaims, c => c.Value == "Admin");
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_TokenContainsNoRoles_WhenUserHasNoRoles()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new List<Role>();

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

        Assert.Empty(roleClaims);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_TokenHasCorrectIssuer()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new List<Role> { new Role { Id = 1, Name = "User" } };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal("calendary.com.ua", jwtToken.Issuer);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_TokenHasCorrectAudience()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new List<Role> { new Role { Id = 1, Name = "User" } };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Contains("calendary-users", jwtToken.Audiences);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_TokenHasExpiration()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new List<Role> { new Role { Id = 1, Name = "User" } };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
        // Token should expire in approximately 30 days
        var expectedExpiration = DateTime.UtcNow.AddDays(30);
        var timeDifference = Math.Abs((jwtToken.ValidTo - expectedExpiration).TotalMinutes);
        Assert.True(timeDifference < 5); // Allow 5 minutes difference
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_UserWithoutEmail_GeneratesTokenWithoutEmailClaim()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = null,
            UserName = "testuser",
            Identity = Guid.NewGuid()
        };
        var roles = new List<Role> { new Role { Id = 1, Name = "User" } };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token = await service.GenerateJwtTokenAsync(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

        Assert.Null(emailClaim);
        Assert.Null(subClaim);
        // But Jti should still be present
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.NotNull(jtiClaim);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_MultipleUsers_GeneratesDifferentTokens()
    {
        // Arrange
        var user1 = CreateTestUser(1, "user1@example.com", Guid.NewGuid());
        var user2 = CreateTestUser(2, "user2@example.com", Guid.NewGuid());
        var roles = new List<Role> { new Role { Id = 1, Name = "User" } };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        var token1 = await service.GenerateJwtTokenAsync(user1);
        var token2 = await service.GenerateJwtTokenAsync(user2);

        // Assert
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_CallsRoleRepository()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new List<Role> { new Role { Id = 1, Name = "User" } };

        _mockRoleRepository.Setup(x => x.GetRolesByUserIdAsync(user.Id))
            .ReturnsAsync(roles);

        var service = CreateService();

        // Act
        await service.GenerateJwtTokenAsync(user);

        // Assert
        _mockRoleRepository.Verify(x => x.GetRolesByUserIdAsync(user.Id), Times.Once);
    }

    #endregion
}
