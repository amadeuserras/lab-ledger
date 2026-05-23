namespace LabLedger.DataModel.Entities;

public class Sample
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Origin { get; set; }
    public SampleStatus Status { get; set; }
    public int SubmittedById { get; set; }
    public DateTime CreatedAt { get; set; }

    public User SubmittedBy { get; set; } = null!;
    public ICollection<Test> Tests { get; set; } = new List<Test>();
}
