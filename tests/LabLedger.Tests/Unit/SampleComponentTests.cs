using FluentAssertions;
using FluentValidation;
using LabLedger.Application.Features.Samples;
using LabLedger.Core.Interfaces;
using LabLedger.DataModel.Entities;
using Moq;

namespace LabLedger.Tests.Unit;

public class SampleComponentTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IRepository<Sample>> _sampleRepository = new();
    private readonly IValidator<CreateSampleRequest> _validator = new SampleValidator();

    public SampleComponentTests()
    {
        _unitOfWork.Setup(u => u.GetRepository<Sample>()).Returns(_sampleRepository.Object);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusFilter_ReturnsMatchingSamplesOnly()
    {
        _sampleRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(CreateSamples());

        var component = new SampleComponent(_unitOfWork.Object, _validator);
        var result = await component.GetAllAsync(SampleStatus.Submitted);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(1);
        result[0].Status.Should().Be(SampleStatus.Submitted);
    }

    [Fact]
    public async Task GetAllAsync_WithoutStatus_ReturnsAllSamples()
    {
        _sampleRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(CreateSamples());

        var component = new SampleComponent(_unitOfWork.Object, _validator);
        var result = await component.GetAllAsync(null);

        result.Should().HaveCount(2);
        result[0].Id.Should().Be(1);
        result[0].Status.Should().Be(SampleStatus.Submitted);
        result[1].Id.Should().Be(2);
        result[1].Status.Should().Be(SampleStatus.InProgress);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsSample()
    {
        Sample? savedSample = null;
        _sampleRepository
            .Setup(r => r.AddAsync(It.IsAny<Sample>()))
            .Callback<Sample>(s => savedSample = s)
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var component = new SampleComponent(_unitOfWork.Object, _validator);
        var request = new CreateSampleRequest("Blood Panel", "Blood", "Clinic 1");

        var result = await component.CreateAsync(request, submittedById: 7);

        savedSample.Should().NotBeNull();
        savedSample!.Name.Should().Be("Blood Panel");
        savedSample.Type.Should().Be("Blood");
        savedSample.Origin.Should().Be("Clinic 1");
        savedSample.Status.Should().Be(SampleStatus.Submitted);
        savedSample.SubmittedById.Should().Be(7);

        result.Name.Should().Be("Blood Panel");
        result.SubmittedById.Should().Be(7);

        _sampleRepository.Verify(r => r.AddAsync(It.IsAny<Sample>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        var component = new SampleComponent(_unitOfWork.Object, _validator);
        var request = new CreateSampleRequest("", "Blood", "Clinic 1");

        var act = () => component.CreateAsync(request, submittedById: 1);

        await act.Should().ThrowAsync<ValidationException>();
        _sampleRepository.Verify(r => r.AddAsync(It.IsAny<Sample>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSample_ReturnsDto()
    {
        var sample = new Sample
        {
            Id = 3,
            Name = "Tissue Sample",
            Type = "Tissue",
            Origin = "Lab B",
            Status = SampleStatus.Submitted,
            SubmittedById = 2,
            CreatedAt = DateTime.UtcNow
        };

        _sampleRepository.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(sample);

        var component = new SampleComponent(_unitOfWork.Object, _validator);
        var result = await component.GetByIdAsync(3);

        result.Should().NotBeNull();
        result!.Id.Should().Be(3);
        result.Name.Should().Be("Tissue Sample");
    }

    [Fact]
    public async Task GetByIdAsync_MissingSample_ReturnsNull()
    {
        _sampleRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Sample?)null);

        var component = new SampleComponent(_unitOfWork.Object, _validator);
        var result = await component.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_ExistingSample_UpdatesStatus()
    {
        var sample = new Sample
        {
            Id = 5,
            Name = "Status Sample",
            Type = "Blood",
            Origin = "Clinic 1",
            Status = SampleStatus.Submitted,
            SubmittedById = 1,
            CreatedAt = DateTime.UtcNow
        };

        _sampleRepository.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(sample);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var component = new SampleComponent(_unitOfWork.Object, _validator);
        var result = await component.UpdateStatusAsync(5, SampleStatus.Completed);

        result.Should().NotBeNull();
        result!.Status.Should().Be(SampleStatus.Completed);
        sample.Status.Should().Be(SampleStatus.Completed);
        _sampleRepository.Verify(r => r.Update(sample), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_MissingSample_ReturnsNull()
    {
        _sampleRepository.Setup(r => r.GetByIdAsync(404)).ReturnsAsync((Sample?)null);

        var component = new SampleComponent(_unitOfWork.Object, _validator);
        var result = await component.UpdateStatusAsync(404, SampleStatus.Rejected);

        result.Should().BeNull();
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static List<Sample> CreateSamples() =>
    [
        new Sample
        {
            Id = 1,
            Name = "Submitted Sample",
            Type = "Blood",
            Origin = "Clinic 1",
            Status = SampleStatus.Submitted,
        },
        new Sample
        {
            Id = 2,
            Name = "In Progress Sample",
            Type = "Tissue",
            Origin = "Clinic 2",
            Status = SampleStatus.InProgress,
        },
    ];
}
