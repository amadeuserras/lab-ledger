using LabLedger.DataModel.Entities;

namespace LabLedger.Application.Features.Tests;

public record TestDto(
    int Id,
    string Method,
    TestStatus Status,
    int SampleId,
    int? AssignedToId,
    DateTime CreatedAt);

public record AssignTestRequest(int AssignedToId);

public record UpdateTestStatusRequest(TestStatus Status);
