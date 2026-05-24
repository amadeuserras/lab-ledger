using FluentValidation;
using LabLedger.Core.Interfaces;
using LabLedger.DataModel.Entities;
using Mapster;

namespace LabLedger.Application.Features.Samples;

public class SampleComponent : ISampleComponent
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateSampleRequest> _validator;

    public SampleComponent(IUnitOfWork unitOfWork, IValidator<CreateSampleRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IReadOnlyList<SampleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var samples = await _unitOfWork.GetRepository<Sample>().GetAllAsync();
        return samples.Adapt<List<SampleDto>>();
    }

    public async Task<SampleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sample = await _unitOfWork.GetRepository<Sample>().GetByIdAsync(id);
        return sample?.Adapt<SampleDto>();
    }

    public async Task<SampleDto> CreateAsync(
        CreateSampleRequest request,
        int submittedById,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var sample = new Sample
        {
            Name = request.Name.Trim(),
            Type = request.Type.Trim(),
            Origin = request.Origin.Trim(),
            Status = SampleStatus.Submitted,
            SubmittedById = submittedById,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.GetRepository<Sample>().AddAsync(sample);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return sample.Adapt<SampleDto>();
    }

    public async Task<SampleDto?> UpdateStatusAsync(
        int id,
        SampleStatus status,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.GetRepository<Sample>();
        var sample = await repo.GetByIdAsync(id);
        if (sample is null)
            return null;

        sample.Status = status;
        repo.Update(sample);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return sample.Adapt<SampleDto>();
    }
}
