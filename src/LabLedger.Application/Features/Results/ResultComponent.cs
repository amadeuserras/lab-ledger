using FluentValidation;
using LabLedger.Core.Interfaces;
using LabLedger.DataModel.Entities;
using Mapster;

namespace LabLedger.Application.Features.Results;

public class ResultComponent : IResultComponent
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateResultRequest> _validator;

    public ResultComponent(IUnitOfWork unitOfWork, IValidator<CreateResultRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<ResultDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.GetRepository<Result>().GetByIdAsync(id);
        return result?.Adapt<ResultDto>();
    }

    public async Task<ResultDto?> CreateAsync(
        int testId,
        CreateResultRequest request,
        int recordedById,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var test = await _unitOfWork.GetRepository<Test>().GetByIdAsync(testId);
        if (test is null)
            return null;

        var results = await _unitOfWork.GetRepository<Result>().GetAllAsync();
        if (results.Any(r => r.TestId == testId))
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

        await _unitOfWork.GetRepository<Result>().AddAsync(result);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Adapt<ResultDto>();
    }

    public async Task<ResultDto?> PublishAsync(
        int id,
        int publishedById,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.GetRepository<Result>();
        var result = await repo.GetByIdAsync(id);
        if (result is null)
            return null;

        if (result.IsPublished)
            throw new InvalidOperationException("Result is already published.");

        result.IsPublished = true;
        result.PublishedById = publishedById;
        result.PublishedAt = DateTime.UtcNow;
        repo.Update(result);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Adapt<ResultDto>();
    }
}
