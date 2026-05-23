namespace LabLedger.DataModel.Entities;

public class Test
{
    public int Id { get; set; }
    public required string Method { get; set; }
    public TestStatus Status { get; set; }
    public int SampleId { get; set; }
    public int? AssignedToId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Sample Sample { get; set; } = null!;
    public User? AssignedTo { get; set; }
    public Result? Result { get; set; }
}
