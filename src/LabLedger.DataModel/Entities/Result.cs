namespace LabLedger.DataModel.Entities;

public class Result
{
    public int Id { get; set; }
    public required string Value { get; set; }
    public required string Unit { get; set; }
    public string? Notes { get; set; }
    public bool IsPublished { get; set; }
    public int TestId { get; set; }
    public int RecordedById { get; set; }
    public int? PublishedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public Test Test { get; set; } = null!;
    public User RecordedBy { get; set; } = null!;
    public User? PublishedBy { get; set; }
}
