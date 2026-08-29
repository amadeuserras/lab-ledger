using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LabLedger.Application.Features.Samples;
using LabLedger.DataModel.Entities;

namespace LabLedger.Tests.Integration;

public class SamplesControllerTests(LabLedgerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAll_WithoutToken_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/samples");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WithToken_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync("samples-reader@lab.test");
        Authenticate(token);

        var response = await Client.GetAsync("/api/samples");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var samples = await response.Content.ReadFromJsonAsync<List<SampleDto>>();
        samples.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAll_WithStatusFilter_ReturnsMatchingSamplesOnly()
    {
        var token = await RegisterAndLoginAsync("samples-reader@lab.test");
        Authenticate(token);

        await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("Submitted Sample", "Blood", "Clinic 1"));
        
        var inProgressResponse = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("In Progress Sample", "Tissue", "Clinic 2"));
        var inProgress = await inProgressResponse.Content.ReadFromJsonAsync<SampleDto>();

        await Client.PatchAsJsonAsync(
            $"/api/samples/{inProgress!.Id}/status",
            new UpdateSampleStatusRequest(SampleStatus.InProgress));

        var response = await Client.GetAsync($"/api/samples?status=InProgress");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var samples = await response.Content.ReadFromJsonAsync<List<SampleDto>>();
        samples.Should().NotBeNull();
        samples!.Count.Should().Be(1);
        samples![0].Id.Should().Be(inProgress!.Id);
        samples![0].Status.Should().Be(SampleStatus.InProgress);
    }

    [Fact]
    public async Task CreateSample_ReturnsCreated()
    {
        var token = await RegisterAndLoginAsync("sample-creator@lab.test");
        Authenticate(token);

        var response = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("Blood Panel A", "Blood", "Clinic 1"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var sample = await response.Content.ReadFromJsonAsync<SampleDto>();
        sample.Should().NotBeNull();
        sample!.Name.Should().Be("Blood Panel A");
        sample.Status.Should().Be(SampleStatus.Submitted);
    }

    [Fact]
    public async Task CreateSample_InvalidRequest_ReturnsBadRequest()
    {
        var token = await RegisterAndLoginAsync("sample-invalid@lab.test");
        Authenticate(token);

        var response = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("", "Blood", "Clinic 1"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        var token = await RegisterAndLoginAsync("sample-missing@lab.test");
        Authenticate(token);

        var createResponse = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("Existing Sample", "Blood", "Clinic 1"));
        var created = await createResponse.Content.ReadFromJsonAsync<SampleDto>();

        var response = await Client.GetAsync($"/api/samples/{created!.Id + 1}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStatus_ReturnsOk()
    {
        var token = await RegisterAndLoginAsync("sample-status@lab.test");
        Authenticate(token);

        var createResponse = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("Status Sample", "Tissue", "Lab B"));
        var created = await createResponse.Content.ReadFromJsonAsync<SampleDto>();

        var response = await Client.PatchAsJsonAsync(
            $"/api/samples/{created!.Id}/status",
            new UpdateSampleStatusRequest(SampleStatus.InProgress));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await response.Content.ReadFromJsonAsync<SampleDto>();
        updated!.Status.Should().Be(SampleStatus.InProgress);
    }
}
