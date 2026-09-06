using FluentAssertions;
using FluentValidation;
using LabLedger.Application.Features.Samples;
using LabLedger.DataModel;
using LabLedger.DataModel.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LabLedger.Tests.Unit;

public class SampleComponentTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LabLedgerDbContext _context;
    private readonly IValidator<CreateSampleRequest> _validator = new SampleValidator();

    public SampleComponentTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LabLedgerDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LabLedgerDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PersistsSample()
    {
        var user = SeedUser();
        var component = new SampleComponent(_context, _validator);
        var request = new CreateSampleRequest("Blood Panel", "Blood", "Clinic 1");

        var result = await component.CreateAsync(request, submittedById: user.Id);

        var savedSample = await _context.Samples.SingleAsync();
        savedSample.Name.Should().Be("Blood Panel");
        savedSample.Type.Should().Be("Blood");
        savedSample.Origin.Should().Be("Clinic 1");
        savedSample.Status.Should().Be(SampleStatus.Submitted);
        savedSample.SubmittedById.Should().Be(user.Id);

        result.Name.Should().Be("Blood Panel");
        result.SubmittedById.Should().Be(user.Id);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        var component = new SampleComponent(_context, _validator);
        var request = new CreateSampleRequest("", "Blood", "Clinic 1");

        var act = () => component.CreateAsync(request, submittedById: 1);

        await act.Should().ThrowAsync<ValidationException>();
        (await _context.Samples.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSample_ReturnsDto()
    {
        var sample = SeedSample("Tissue Sample", "Tissue", "Lab B");
        var component = new SampleComponent(_context, _validator);

        var result = await component.GetByIdAsync(sample.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(sample.Id);
        result.Name.Should().Be("Tissue Sample");
    }

    [Fact]
    public async Task GetByIdAsync_MissingSample_ReturnsNull()
    {
        var component = new SampleComponent(_context, _validator);
        var result = await component.GetByIdAsync(99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatusAsync_ExistingSample_UpdatesStatus()
    {
        var sample = SeedSample("Status Sample", "Blood", "Clinic 1");
        var component = new SampleComponent(_context, _validator);

        var result = await component.UpdateStatusAsync(sample.Id, SampleStatus.Completed);

        result.Should().NotBeNull();
        result!.Status.Should().Be(SampleStatus.Completed);
        sample.Status.Should().Be(SampleStatus.Completed);
    }

    [Fact]
    public async Task UpdateStatusAsync_MissingSample_ReturnsNull()
    {
        var component = new SampleComponent(_context, _validator);
        var result = await component.UpdateStatusAsync(404, SampleStatus.Rejected);

        result.Should().BeNull();
        (await _context.Samples.CountAsync()).Should().Be(0);
    }

    private User SeedUser()
    {
        var user = new User
        {
            Email = "scientist@lab.test",
            PasswordHash = "hash",
            FullName = "Test User",
            Role = UserRole.Scientist,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }

    private Sample SeedSample(string name, string type, string origin)
    {
        var user = SeedUser();
        var sample = new Sample
        {
            Name = name,
            Type = type,
            Origin = origin,
            Status = SampleStatus.Submitted,
            SubmittedById = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Samples.Add(sample);
        _context.SaveChanges();
        return sample;
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
