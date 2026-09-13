using LabLedger.DataModel.Entities;

namespace LabLedger.Application.Features.Samples;

public record SampleDto(
    int Id,
    string Name,
    string Type,
    string Origin,
    SampleStatus Status,
    int SubmittedById,
    string SubmittedByFullName,
    DateTime CreatedAt);

public record UpdateSampleStatusRequest(SampleStatus Status);
