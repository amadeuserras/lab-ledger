using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using LabLedger.Application.Features.Results;
using LabLedger.Application.Features.Samples;
using LabLedger.Application.Features.Tests;
using LabLedger.DataModel.Entities;

namespace LabLedger.Tests.Integration;

public class GraphQLSamplesQueryTests(LabLedgerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public async Task Samples_WithoutToken_ReturnsAuthError()
    {
        var response = await PostGraphQLAsync(
            """
            {
              samples {
                id
                name
              }
            }
            """);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await DeserializeAsync<SamplesQueryData>(response);
        body.Data.Should().BeNull();
        body.Errors.Should().ContainSingle();
        body.Errors![0].Extensions!.Code.Should().Be("AUTH_NOT_AUTHENTICATED");
    }

    [Fact]
    public async Task Samples_WithToken_ReturnsNestedData()
    {
        await SeedSampleWithTestAndResultAsync("GraphQL User");

        var token = await RegisterAndLoginAsync("gql-reader@lab.test", fullName: "GraphQL User");
        Authenticate(token);

        var response = await PostGraphQLAsync(
            """
            {
              samples {
                id
                name
                status
                submittedBy { fullName }
                tests {
                  method
                  status
                  result { value unit isPublished }
                }
              }
            }
            """);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await DeserializeAsync<SamplesQueryData>(response);
        body.Errors.Should().BeNull();
        body.Data!.Samples.Should().ContainSingle();

        var sample = body.Data.Samples[0];
        sample.Name.Should().Be("GraphQL Sample");
        sample.Status.Should().Be("SUBMITTED");
        sample.SubmittedBy!.FullName.Should().Be("GraphQL User");
        sample.Tests.Should().ContainSingle();

        var test = sample.Tests![0];
        test.Method.Should().Be("PCR");
        test.Status.Should().Be("PENDING");
        test.Result!.Value.Should().Be("42.5");
        test.Result.Unit.Should().Be("mg/dL");
        test.Result.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task Samples_WithMultipleTests_ReturnsAllTests()
    {
        await SeedSampleWithMultipleTestsAsync();

        var token = await RegisterAndLoginAsync("gql-multi@lab.test");
        Authenticate(token);

        var response = await PostGraphQLAsync(
            """
            {
              samples {
                name
                tests {
                  method
                  status
                  result { value unit isPublished }
                }
              }
            }
            """);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await DeserializeAsync<SamplesQueryData>(response);
        body.Errors.Should().BeNull();
        body.Data!.Samples.Should().ContainSingle();

        var sample = body.Data.Samples[0];
        sample.Name.Should().Be("Multi-Test Sample");
        sample.Tests.Should().HaveCount(2);
        sample.Tests.Should().BeEquivalentTo(
        [
            new TestGraphQL("PCR", "PENDING", new ResultGraphQL("42.5", "mg/dL", false)),
            new TestGraphQL("ELISA", "PENDING", null)
        ]);
    }

    [Fact]
    public async Task Samples_WithStatusFilter_ReturnsMatchingSamplesOnly()
    {
        var token = await RegisterAndLoginAsync("gql-filter@lab.test");
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

        var response = await PostGraphQLAsync(
            """
            {
              samples(status: IN_PROGRESS) {
                id
                name
                status
              }
            }
            """);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await DeserializeAsync<SamplesQueryData>(response);
        body.Errors.Should().BeNull();
        body.Data!.Samples.Should().ContainSingle();
        body.Data.Samples[0].Name.Should().Be("In Progress Sample");
        body.Data.Samples[0].Status.Should().Be("IN_PROGRESS");
    }

    private async Task SeedSampleWithMultipleTestsAsync()
    {
        var scientistToken = await RegisterAndLoginAsync("gql-multi-scientist@lab.test");
        Authenticate(scientistToken);

        var sampleResponse = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("Multi-Test Sample", "Blood", "Clinic 1"));
        var sample = await sampleResponse.Content.ReadFromJsonAsync<SampleDto>();

        var technicianToken = await SeedUserAndLoginAsync("gql-multi-tech@lab.test", UserRole.Technician);
        Authenticate(technicianToken);

        var pcrResponse = await Client.PostAsJsonAsync(
            $"/api/samples/{sample!.Id}/tests",
            new CreateTestRequest("PCR"));
        var pcr = await pcrResponse.Content.ReadFromJsonAsync<TestDto>();

        await Client.PostAsJsonAsync(
            $"/api/samples/{sample.Id}/tests",
            new CreateTestRequest("ELISA"));

        var loginResponse = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LabLedger.Application.Features.Auth.LoginRequest("gql-multi-tech@lab.test", "Password123!"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LabLedger.Application.Features.Auth.LoginResponse>();

        await Client.PatchAsJsonAsync(
            $"/api/tests/{pcr!.Id}/assign",
            new AssignTestRequest(login!.UserId));

        await Client.PostAsJsonAsync(
            $"/api/tests/{pcr.Id}/result",
            new CreateResultRequest("42.5", "mg/dL", null));
    }

    private async Task SeedSampleWithTestAndResultAsync(string submitterName)
    {
        var scientistToken = await RegisterAndLoginAsync(
            "gql-seed-scientist@lab.test",
            fullName: submitterName);
        Authenticate(scientistToken);

        var sampleResponse = await Client.PostAsJsonAsync(
            "/api/samples",
            new CreateSampleRequest("GraphQL Sample", "Blood", "Clinic 1"));
        var sample = await sampleResponse.Content.ReadFromJsonAsync<SampleDto>();

        var technicianToken = await SeedUserAndLoginAsync("gql-seed-tech@lab.test", UserRole.Technician);
        Authenticate(technicianToken);

        var testResponse = await Client.PostAsJsonAsync(
            $"/api/samples/{sample!.Id}/tests",
            new CreateTestRequest("PCR"));
        var test = await testResponse.Content.ReadFromJsonAsync<TestDto>();

        var loginResponse = await Client.PostAsJsonAsync(
            "/api/auth/login",
            new LabLedger.Application.Features.Auth.LoginRequest("gql-seed-tech@lab.test", "Password123!"));
        var login = await loginResponse.Content.ReadFromJsonAsync<LabLedger.Application.Features.Auth.LoginResponse>();

        await Client.PatchAsJsonAsync(
            $"/api/tests/{test!.Id}/assign",
            new AssignTestRequest(login!.UserId));

        await Client.PostAsJsonAsync(
            $"/api/tests/{test.Id}/result",
            new CreateResultRequest("42.5", "mg/dL", null));
    }

    private Task<HttpResponseMessage> PostGraphQLAsync(string query) =>
        Client.PostAsJsonAsync("/graphql", new { query });

    private static async Task<GraphQLResponse<T>> DeserializeAsync<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<GraphQLResponse<T>>(JsonOptions);
        return body!;
    }

    private sealed record GraphQLResponse<T>(T? Data, IReadOnlyList<GraphQLError>? Errors);

    private sealed record GraphQLError(string Message, GraphQLErrorExtensions? Extensions);

    private sealed record GraphQLErrorExtensions(string Code);

    private sealed record SamplesQueryData(IReadOnlyList<SampleGraphQL> Samples);

    private sealed record SampleGraphQL(
        int Id,
        string Name,
        string Status,
        UserGraphQL? SubmittedBy,
        IReadOnlyList<TestGraphQL>? Tests);

    private sealed record UserGraphQL(string FullName);

    private sealed record TestGraphQL(string Method, string Status, ResultGraphQL? Result);

    private sealed record ResultGraphQL(string Value, string Unit, bool IsPublished);
}
