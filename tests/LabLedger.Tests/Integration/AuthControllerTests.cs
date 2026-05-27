using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LabLedger.Application.Features.Auth;

namespace LabLedger.Tests.Integration;

public class AuthControllerTests(LabLedgerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_ReturnsCreated()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("scientist@lab.test", "Password123!", "Dr. Smith"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be("scientist@lab.test");
        body.FullName.Should().Be("Dr. Smith");
        body.Role.Should().Be("Scientist");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var request = new RegisterRequest("duplicate@lab.test", "Password123!", "First User");
        await Client.PostAsJsonAsync("/api/auth/register", request);

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        await Client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("login@lab.test", "Password123!", "Login User"));

        var response = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("login@lab.test", "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Email.Should().Be("login@lab.test");
        body.Role.Should().Be("Scientist");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        const string email = "bad-login@lab.test";

        await Client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest(email, "Password123!", "Bad Login User"));

        var response = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest(email, "Password123"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
