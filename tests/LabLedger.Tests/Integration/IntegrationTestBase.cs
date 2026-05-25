using System.Net.Http.Headers;
using System.Net.Http.Json;
using LabLedger.Application.Features.Auth;
using LabLedger.DataModel;
using LabLedger.DataModel.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace LabLedger.Tests.Integration;

public abstract class IntegrationTestBase : IClassFixture<LabLedgerWebApplicationFactory>, IDisposable
{
    protected IntegrationTestBase(LabLedgerWebApplicationFactory factory)
    {
        Factory = factory;
        Factory.ResetDatabase();
        Client = Factory.CreateClient();
    }

    protected LabLedgerWebApplicationFactory Factory { get; }
    protected HttpClient Client { get; }

    protected async Task<string> RegisterAndLoginAsync(
        string email,
        string password = "Password123!",
        string fullName = "Test User")
    {
        await Client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, password, fullName));

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.Token;
    }

    protected async Task<string> SeedUserAndLoginAsync(
        string email,
        UserRole role,
        string password = "Password123!",
        string fullName = "Seeded User")
    {
        using (var scope = Factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<LabLedgerDbContext>();
            context.Users.Add(new User
            {
                Email = email.ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullName,
                Role = role,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        loginResponse.EnsureSuccessStatusCode();

        var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return login!.Token;
    }

    protected void Authenticate(string token) =>
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    protected void ClearAuthentication() =>
        Client.DefaultRequestHeaders.Authorization = null;

    public void Dispose() => Client.Dispose();
}
