using LabLedger.DataModel.Entities;

namespace LabLedger.Application.Features.Samples;

public interface ISampleComponent
{
    Task<IReadOnlyList<SampleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SampleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<SampleDto> CreateAsync(CreateSampleRequest request, int submittedById, CancellationToken cancellationToken = default);
    Task<SampleDto?> UpdateStatusAsync(int id, SampleStatus status, CancellationToken cancellationToken = default);
}
