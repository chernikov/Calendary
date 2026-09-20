using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Calendary.Api.Dtos;
using Xunit;

namespace Calendary.Api.Tests;

public class AuthFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthFlowTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static string UniqueEmail() => $"test-{Guid.NewGuid():N}@example.com";

    [Fact]
    public async Task Register_then_login_then_me_returns_the_same_user()
    {
        var email = UniqueEmail();

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest(email, "Sup3rSecret!", "Тест"));
        registerResponse.EnsureSuccessStatusCode();
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(registered);
        Assert.Equal(email, registered.User.Email);
        Assert.False(registered.User.EmailConfirmed);

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(email, "Sup3rSecret!"));
        loginResponse.EnsureSuccessStatusCode();
        var loggedIn = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loggedIn);
        Assert.Equal(registered.User.Id, loggedIn.User.Id);

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loggedIn.BearerToken);
        var meResponse = await _client.SendAsync(meRequest);
        meResponse.EnsureSuccessStatusCode();
        var me = await meResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.Equal(registered.User.Id, me!.Id);
    }

    [Fact]
    public async Task Register_with_an_already_registered_email_returns_409()
    {
        var email = UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Sup3rSecret!", "Тест"));

        var second = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "AnotherPass1!", "Тест2"));

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_with_a_too_short_password_returns_400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest(UniqueEmail(), "short", "Тест"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_with_an_invalid_email_returns_400()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/register", new RegisterRequest("not-an-email", "Sup3rSecret!", "Тест"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        var email = UniqueEmail();
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Sup3rSecret!", "Тест"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword1!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_with_an_unknown_email_returns_401()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/login", new LoginRequest(UniqueEmail(), "Sup3rSecret!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_without_a_bearer_token_returns_401()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_with_a_garbage_bearer_token_returns_401()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
