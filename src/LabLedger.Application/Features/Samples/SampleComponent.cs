using LabLedger.Core.Interfaces;
using LabLedger.DataModel.Entities;

namespace LabLedger.Application.Features.Samples;

public class SampleComponent : ISampleComponent
{
    private readonly IUnitOfWork _unitOfWork;

    public SampleComponent(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<SampleResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var sampleRepo = _unitOfWork.GetRepository<Sample>();
        var userRepo = _unitOfWork.GetRepository<User>();

        var samples = await sampleRepo.GetAllAsync();
        var users = await userRepo.GetAllAsync();
        var usersById = users.ToDictionary(u => u.Id);

        return samples
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => ToResponse(s, usersById))
            .ToList();
    }

    public async Task<SampleResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var sampleRepo = _unitOfWork.GetRepository<Sample>();
        var userRepo = _unitOfWork.GetRepository<User>();

        var sample = await sampleRepo.GetByIdAsync(id);
        if (sample is null)
            return null;

        var users = await userRepo.GetAllAsync();
        var usersById = users.ToDictionary(u => u.Id);

        return ToResponse(sample, usersById);
    }

    public async Task<SampleResponse> CreateAsync(
        CreateSampleRequest request,
        int submittedById,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var type = request.Type.Trim();
        var origin = request.Origin.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type) || string.IsNullOrEmpty(origin))
            throw new ArgumentException("Name, type, and origin are required.");

        var userRepo = _unitOfWork.GetRepository<User>();
        var submitter = await userRepo.GetByIdAsync(submittedById);
        if (submitter is null)
            throw new InvalidOperationException("Submitting user was not found.");

        var sample = new Sample
        {
            Name = name,
            Type = type,
            Origin = origin,
            Status = SampleStatus.Submitted,
            SubmittedById = submittedById,
            CreatedAt = DateTime.UtcNow
        };

        var sampleRepo = _unitOfWork.GetRepository<Sample>();
        await sampleRepo.AddAsync(sample);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(sample, new Dictionary<int, User> { [submitter.Id] = submitter });
    }

    private static SampleResponse ToResponse(Sample sample, IReadOnlyDictionary<int, User> usersById)
    {
        var submitterName = usersById.TryGetValue(sample.SubmittedById, out var user)
            ? user.FullName
            : string.Empty;

        return new SampleResponse(
            sample.Id,
            sample.Name,
            sample.Type,
            sample.Origin,
            sample.Status.ToString(),
            sample.SubmittedById,
            submitterName,
            sample.CreatedAt);
    }
}
