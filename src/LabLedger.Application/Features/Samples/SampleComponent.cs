using FluentValidation;
using LabLedger.DataModel;
using LabLedger.DataModel.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LabLedger.Application.Features.Samples;

public class SampleComponent : ISampleComponent
{
    private readonly LabLedgerDbContext _context;
    private readonly IValidator<CreateSampleRequest> _validator;

    public SampleComponent(LabLedgerDbContext context, IValidator<CreateSampleRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<IReadOnlyList<SampleDto>> GetAllAsync(SampleStatus? status, CancellationToken cancellationToken = default)
    {
        IQueryable<Sample> query = _context.Samples;

        if (status is not null)
            query = query.Where(s => s.Status == status);

        var samples = await query.ToListAsync(cancellationToken);
        return samples.Adapt<List<SampleDto>>();
    }

    public async Task<SampleDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var sample = await _context.Samples.FindAsync([id], cancellationToken);
        return sample.Adapt<SampleDto>();
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

        _context.Samples.Add(sample);
        await _context.SaveChangesAsync(cancellationToken);

        return sample.Adapt<SampleDto>();
    }

    public async Task<SampleDto?> UpdateStatusAsync(
        int id,
        SampleStatus status,
        CancellationToken cancellationToken = default)
    {
        var sample = await _context.Samples.FindAsync([id], cancellationToken);
        if (sample is null)
            return null;

        sample.Status = status;
        await _context.SaveChangesAsync(cancellationToken);

        return sample.Adapt<SampleDto>();
    }
}
