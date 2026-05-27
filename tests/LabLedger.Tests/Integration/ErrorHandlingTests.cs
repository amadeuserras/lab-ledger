using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using LabLedger.Application.Features.Auth;
using LabLedger.Application.Features.Samples;

namespace LabLedger.Tests.Integration;

public class ErrorHandlingTests(LabLedgerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task ValidationError_ReturnsProblemDetailsWithFieldErrors()
    {
        var token = await RegisterAndLoginAsync("validation-error@lab.test");
        Authenticate(token);

        var response = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("", "Blood", "Clinic 1"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("status").GetInt32().Should().Be(400);
        problem.GetProperty("title").GetString().Should().Be("Validation Error");
        problem.TryGetProperty("errors", out var errors).Should().BeTrue();
        errors.GetProperty("Name").EnumerateArray().Should().NotBeEmpty();
    }

    [Fact]
    public async Task NotFound_ReturnsProblemDetails()
    {
        var token = await RegisterAndLoginAsync("not-found@lab.test");
        Authenticate(token);

        var response = await Client.GetAsync("/api/samples/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("status").GetInt32().Should().Be(404);
        problem.GetProperty("title").GetString().Should().Be("Not Found");
    }

    [Fact]
    public async Task Conflict_ReturnsProblemDetails()
    {
        var request = new RegisterRequest("conflict@lab.test", "Password123!", "First User");
        await Client.PostAsJsonAsync("/api/auth/register", request);

        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("status").GetInt32().Should().Be(409);
        problem.GetProperty("title").GetString().Should().Be("Conflict");
        problem.GetProperty("detail").GetString().Should().Contain("already exists");
    }
}
