namespace LabLedger.Application.Features.Samples;

public record CreateSampleRequest(string Name, string Type, string Origin);

public record SampleResponse(
    int Id,
    string Name,
    string Type,
    string Origin,
    string Status,
    int SubmittedById,
    string SubmittedByName,
    DateTime CreatedAt);
