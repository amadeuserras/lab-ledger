using LabLedger.DataModel.Entities;

namespace LabLedger.Application.Features.Samples;

public record SampleDto(
    int Id,
    string Name,
    string Type,
    string Origin,
    SampleStatus Status,
    int SubmittedById,
    DateTime CreatedAt);

public record UpdateSampleStatusRequest(SampleStatus Status);
