using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LabLedger.Application.Features.Results;
using LabLedger.Application.Features.Samples;
using LabLedger.Application.Features.Tests;
using LabLedger.DataModel.Entities;

namespace LabLedger.Tests.Integration;

public class TestsAndResultsControllerTests(LabLedgerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateTest_OnSample_ReturnsCreated()
    {
        var scientistToken = await RegisterAndLoginAsync("test-scientist@lab.test");
        Authenticate(scientistToken);

        var sampleResponse = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("Test Sample", "Blood", "Clinic 1"));
        var sample = await sampleResponse.Content.ReadFromJsonAsync<SampleDto>();

        var technicianToken = await SeedUserAndLoginAsync("test-tech@lab.test", UserRole.Technician);
        Authenticate(technicianToken);

        var response = await Client.PostAsJsonAsync(
            $"/api/samples/{sample!.Id}/tests",
            new CreateTestRequest("PCR"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var test = await response.Content.ReadFromJsonAsync<TestDto>();
        test.Should().NotBeNull();
        test!.Method.Should().Be("PCR");
        test.Status.Should().Be(TestStatus.Pending);
        test.SampleId.Should().Be(sample.Id);
    }

    [Fact]
    public async Task AssignTest_ReturnsOk()
    {
        var (testId, technicianId) = await SeedSampleAndTestAsync();

        var technicianToken = await SeedUserAndLoginAsync("assign-tech@lab.test", UserRole.Technician);
        Authenticate(technicianToken);

        var response = await Client.PatchAsJsonAsync(
            $"/api/tests/{testId}/assign",
            new AssignTestRequest(technicianId));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var test = await response.Content.ReadFromJsonAsync<TestDto>();
        test!.AssignedToId.Should().Be(technicianId);
    }

    [Fact]
    public async Task UpdateTestStatus_ReturnsOk()
    {
        var (testId, _) = await SeedSampleAndTestAsync();

        var technicianToken = await SeedUserAndLoginAsync("status-tech@lab.test", UserRole.Technician);
        Authenticate(technicianToken);

        var response = await Client.PatchAsJsonAsync(
            $"/api/tests/{testId}/status",
            new UpdateTestStatusRequest(TestStatus.InProgress));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var test = await response.Content.ReadFromJsonAsync<TestDto>();
        test!.Status.Should().Be(TestStatus.InProgress);
    }

    [Fact]
    public async Task CreateResult_ReturnsCreated()
    {
        var (testId, technicianId) = await SeedSampleAndTestAsync();

        var technicianToken = await SeedUserAndLoginAsync("result-tech@lab.test", UserRole.Technician);
        Authenticate(technicianToken);

        await Client.PatchAsJsonAsync(
            $"/api/tests/{testId}/assign",
            new AssignTestRequest(technicianId));

        var response = await Client.PostAsJsonAsync(
            $"/api/tests/{testId}/result",
            new CreateResultRequest("42.5", "mg/dL", "Within range"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ResultDto>();
        result.Should().NotBeNull();
        result!.Value.Should().Be("42.5");
        result.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task PublishResult_ReturnsOk()
    {
        var resultId = await SeedDraftResultAsync();

        var supervisorToken = await SeedUserAndLoginAsync("supervisor@lab.test", UserRole.Supervisor);
        Authenticate(supervisorToken);

        var response = await Client.PatchAsync($"/api/results/{resultId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ResultDto>();
        result!.IsPublished.Should().BeTrue();
        result.PublishedById.Should().NotBeNull();
        result.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetResult_ReturnsOk()
    {
        var resultId = await SeedDraftResultAsync();

        var scientistToken = await RegisterAndLoginAsync("result-reader@lab.test");
        Authenticate(scientistToken);

        var response = await Client.GetAsync($"/api/results/{resultId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ResultDto>();
        result!.Id.Should().Be(resultId);
    }

    [Fact]
    public async Task Scientist_CannotPublishResult_ReturnsForbidden()
    {
        var resultId = await SeedDraftResultAsync();

        var scientistToken = await RegisterAndLoginAsync("no-publish@lab.test");
        Authenticate(scientistToken);

        var response = await Client.PatchAsync($"/api/results/{resultId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Scientist_CannotAssignTest_ReturnsForbidden()
    {
        var (testId, technicianId) = await SeedSampleAndTestAsync();

        var scientistToken = await RegisterAndLoginAsync("no-assign@lab.test");
        Authenticate(scientistToken);

        var response = await Client.PatchAsJsonAsync(
            $"/api/tests/{testId}/assign",
            new AssignTestRequest(technicianId));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<(int TestId, int TechnicianId)> SeedSampleAndTestAsync()
    {
        var scientistToken = await RegisterAndLoginAsync("seed-scientist@lab.test");
        Authenticate(scientistToken);

        var sampleResponse = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("Seed Sample", "Blood", "Clinic 1"));
        var sample = await sampleResponse.Content.ReadFromJsonAsync<SampleDto>();

        var technicianToken = await SeedUserAndLoginAsync("seed-tech@lab.test", UserRole.Technician);
        Authenticate(technicianToken);

        var testResponse = await Client.PostAsJsonAsync(
            $"/api/samples/{sample!.Id}/tests",
            new CreateTestRequest("ELISA"));
        var test = await testResponse.Content.ReadFromJsonAsync<TestDto>();

        return (test!.Id, await GetUserIdAsync("seed-tech@lab.test"));
    }

    private async Task<int> SeedDraftResultAsync()
    {
        var (testId, technicianId) = await SeedSampleAndTestAsync();

        var technicianToken = await SeedUserAndLoginAsync("seed-result-tech@lab.test", UserRole.Technician);
        Authenticate(technicianToken);

        await Client.PatchAsJsonAsync(
            $"/api/tests/{testId}/assign",
            new AssignTestRequest(technicianId));

        var resultResponse = await Client.PostAsJsonAsync(
            $"/api/tests/{testId}/result",
            new CreateResultRequest("10.0", "U/L", null));
        var result = await resultResponse.Content.ReadFromJsonAsync<ResultDto>();

        return result!.Id;
    }

    private async Task<int> GetUserIdAsync(string email)
    {
        var loginResponse = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LabLedger.Application.Features.Auth.LoginRequest(email, "Password123!"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LabLedger.Application.Features.Auth.LoginResponse>();
        return login!.UserId;
    }
}
