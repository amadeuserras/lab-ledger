using LabLedger.DataModel.Entities;

namespace LabLedger.Application.Features.Tests;

public interface ITestComponent
{
    Task<TestDto?> CreateAsync(int sampleId, CreateTestRequest request, CancellationToken cancellationToken = default);
    Task<TestDto?> AssignAsync(int testId, AssignTestRequest request, CancellationToken cancellationToken = default);
    Task<TestDto?> UpdateStatusAsync(int testId, TestStatus status, CancellationToken cancellationToken = default);
}
