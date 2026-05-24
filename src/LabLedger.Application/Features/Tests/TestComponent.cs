using FluentValidation;
using LabLedger.Core.Interfaces;
using LabLedger.DataModel.Entities;
using Mapster;

namespace LabLedger.Application.Features.Tests;

public class TestComponent : ITestComponent
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateTestRequest> _validator;

    public TestComponent(IUnitOfWork unitOfWork, IValidator<CreateTestRequest> validator)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<TestDto?> CreateAsync(
        int sampleId,
        CreateTestRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var sample = await _unitOfWork.GetRepository<Sample>().GetByIdAsync(sampleId);
        if (sample is null)
            return null;

        var test = new Test
        {
            Method = request.Method.Trim(),
            Status = TestStatus.Pending,
            SampleId = sampleId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.GetRepository<Test>().AddAsync(test);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return test.Adapt<TestDto>();
    }

    public async Task<TestDto?> AssignAsync(
        int testId,
        AssignTestRequest request,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.GetRepository<Test>();
        var test = await repo.GetByIdAsync(testId);
        if (test is null)
            return null;

        var user = await _unitOfWork.GetRepository<User>().GetByIdAsync(request.AssignedToId);
        if (user is null)
            throw new InvalidOperationException("Assigned user not found.");

        test.AssignedToId = request.AssignedToId;
        repo.Update(test);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return test.Adapt<TestDto>();
    }

    public async Task<TestDto?> UpdateStatusAsync(
        int testId,
        TestStatus status,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.GetRepository<Test>();
        var test = await repo.GetByIdAsync(testId);
        if (test is null)
            return null;

        test.Status = status;
        repo.Update(test);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return test.Adapt<TestDto>();
    }
}
