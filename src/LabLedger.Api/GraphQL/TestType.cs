using LabLedger.DataModel.Entities;

namespace LabLedger.Api.GraphQL;

public class TestType : ObjectType<Test>
{
    protected override void Configure(IObjectTypeDescriptor<Test> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(t => t.Method);
        descriptor.Field(t => t.Status);
        descriptor.Field(t => t.Result);
    }
}
