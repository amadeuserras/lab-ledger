using LabLedger.DataModel.Entities;

namespace LabLedger.Api.GraphQL;

public class ResultType : ObjectType<Result>
{
    protected override void Configure(IObjectTypeDescriptor<Result> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(r => r.Value);
        descriptor.Field(r => r.Unit);
        descriptor.Field(r => r.IsPublished);
    }
}
