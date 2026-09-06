using FluentValidation;
using LabLedger.DataModel;
using LabLedger.DataModel.Entities;
using Mapster;

namespace LabLedger.Application.Features.Tests;

public class TestComponent : ITestComponent
{
    private readonly LabLedgerDbContext _context;
    private readonly IValidator<CreateTestRequest> _validator;

    public TestComponent(LabLedgerDbContext context, IValidator<CreateTestRequest> validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<TestDto?> CreateAsync(
        int sampleId,
        CreateTestRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var sample = await _context.Samples.FindAsync([sampleId], cancellationToken);
        if (sample is null)
            return null;

        var test = new Test
        {
            Method = request.Method.Trim(),
            Status = TestStatus.Pending,
            SampleId = sampleId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tests.Add(test);
        await _context.SaveChangesAsync(cancellationToken);

        return test.Adapt<TestDto>();
    }

    public async Task<TestDto?> AssignAsync(
        int testId,
        AssignTestRequest request,
        CancellationToken cancellationToken = default)
    {
        var test = await _context.Tests.FindAsync([testId], cancellationToken);
        if (test is null)
            return null;

        var user = await _context.Users.FindAsync([request.AssignedToId], cancellationToken);
        if (user is null)
            throw new InvalidOperationException("Assigned user not found.");

        test.AssignedToId = request.AssignedToId;
        await _context.SaveChangesAsync(cancellationToken);

        return test.Adapt<TestDto>();
    }

    public async Task<TestDto?> UpdateStatusAsync(
        int testId,
        TestStatus status,
        CancellationToken cancellationToken = default)
    {
        var test = await _context.Tests.FindAsync([testId], cancellationToken);
        if (test is null)
            return null;

        test.Status = status;
        await _context.SaveChangesAsync(cancellationToken);

        return test.Adapt<TestDto>();
    }
}
