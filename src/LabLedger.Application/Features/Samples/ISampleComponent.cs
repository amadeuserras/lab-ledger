namespace LabLedger.Application.Features.Samples;

public interface ISampleComponent
{
    Task<IReadOnlyList<SampleResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SampleResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SampleResponse> CreateAsync(
        CreateSampleRequest request,
        int submittedById,
        CancellationToken cancellationToken = default);
}
