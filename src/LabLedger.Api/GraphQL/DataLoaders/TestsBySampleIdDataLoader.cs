using GreenDonut;
using LabLedger.DataModel;
using LabLedger.DataModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabLedger.Api.GraphQL.DataLoaders;

public class TestsBySampleIdDataLoader : GroupedDataLoader<int, Test>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TestsBySampleIdDataLoader(
        IBatchScheduler batchScheduler,
        DataLoaderOptions options,
        IServiceScopeFactory scopeFactory)
        : base(batchScheduler, options)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task<ILookup<int, Test>> LoadGroupedBatchAsync(
        IReadOnlyList<int> keys,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LabLedgerDbContext>();

        var tests = await db.Tests
            .AsNoTracking()
            .Include(t => t.Result)
            .Where(t => keys.Contains(t.SampleId))
            .ToListAsync(cancellationToken);

        return tests.ToLookup(t => t.SampleId);
    }
}
