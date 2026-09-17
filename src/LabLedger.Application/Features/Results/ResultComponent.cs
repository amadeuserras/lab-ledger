using FluentValidation;
using LabLedger.DataModel;
using LabLedger.DataModel.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace LabLedger.Application.Features.Results;

public class ResultComponent : IResultComponent
{
    private readonly LabLedgerDbContext _context;
    private readonly IValidator<CreateResultRequest> _validator;

    public ResultComponent(LabLedgerDbContext context, IValidator<CreateResultRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<ResultDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _context.Results
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
            
        return result?.Adapt<ResultDto>();
    }

    public async Task<ResultDto?> CreateAsync(
        int testId,
        CreateResultRequest request,
        int recordedById,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var test = await _context.Tests.FindAsync([testId], cancellationToken);
        if (test is null)
            return null;

        bool testHasResult = await _context.Results.AnyAsync(r => r.TestId == testId, cancellationToken);
        if (testHasResult)
            throw new InvalidOperationException("This test already has a result.");

        var result = new Result
        {
            Value = request.Value.Trim(),
            Unit = request.Unit.Trim(),
            Notes = request.Notes?.Trim(),
            IsPublished = false,
            TestId = testId,
            RecordedById = recordedById,
            CreatedAt = DateTime.UtcNow
        };

        _context.Results.Add(result);
        await _context.SaveChangesAsync(cancellationToken);

        return result.Adapt<ResultDto>();
    }

    public async Task<ResultDto?> PublishAsync(
        int id,
        int publishedById,
        CancellationToken cancellationToken = default)
    {
        var result = await _context.Results.FindAsync([id], cancellationToken);
        if (result is null)
            return null;

        if (result.IsPublished)
            throw new InvalidOperationException("Result is already published.");

        result.IsPublished = true;
        result.PublishedById = publishedById;
        result.PublishedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return result.Adapt<ResultDto>();
    }
}
