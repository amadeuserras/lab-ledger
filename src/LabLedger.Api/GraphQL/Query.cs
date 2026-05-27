using HotChocolate.Authorization;
using LabLedger.Api.Authorization;
using LabLedger.Application.Features.Auth;
using LabLedger.DataModel;
using LabLedger.DataModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabLedger.Api.GraphQL;

public class Query
{
    [Authorize(Policy = RequirePermissionAttribute.PolicyPrefix + Permissions.SamplesRead)]
    public IQueryable<Sample> GetSamples(
        [Service] LabLedgerDbContext context,
        SampleStatus? status)
    {
        IQueryable<Sample> query = context.Samples
            .AsNoTracking()
            .Include(s => s.SubmittedBy);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        return query;
    }
}
