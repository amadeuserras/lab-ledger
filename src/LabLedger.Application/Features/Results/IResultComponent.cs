namespace LabLedger.Application.Features.Results;

public interface IResultComponent
{
    Task<ResultDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ResultDto?> CreateAsync(int testId, CreateResultRequest request, int recordedById, CancellationToken cancellationToken = default);
    Task<ResultDto?> PublishAsync(int id, int publishedById, CancellationToken cancellationToken = default);
}
