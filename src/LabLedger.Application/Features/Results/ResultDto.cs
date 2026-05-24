namespace LabLedger.Application.Features.Results;

public record ResultDto(
    int Id,
    string Value,
    string Unit,
    string? Notes,
    bool IsPublished,
    int TestId,
    int RecordedById,
    int? PublishedById,
    DateTime CreatedAt,
    DateTime? PublishedAt);
