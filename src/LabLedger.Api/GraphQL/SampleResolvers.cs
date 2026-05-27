using LabLedger.Api.GraphQL.DataLoaders;
using LabLedger.DataModel.Entities;

namespace LabLedger.Api.GraphQL;

public class SampleResolvers
{
    public async Task<IReadOnlyList<Test>> GetTestsAsync(
        [Parent] Sample sample,
        TestsBySampleIdDataLoader testsBySampleId,
        CancellationToken cancellationToken) =>
        await testsBySampleId.LoadAsync(sample.Id, cancellationToken) ?? [];
}
