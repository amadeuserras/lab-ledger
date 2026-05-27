using LabLedger.DataModel.Entities;

namespace LabLedger.Api.GraphQL;

public class SampleType : ObjectType<Sample>
{
    protected override void Configure(IObjectTypeDescriptor<Sample> descriptor)
    {
        descriptor.BindFieldsExplicitly();

        descriptor.Field(s => s.Id);
        descriptor.Field(s => s.Name);
        descriptor.Field(s => s.Status);
        descriptor.Field(s => s.SubmittedBy);
        descriptor
            .Field("tests")
            .ResolveWith<SampleResolvers>(r => r.GetTestsAsync(default!, default!, default!));
    }
}
