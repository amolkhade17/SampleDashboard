namespace AdminDashboard.Domain.Entities;

public class PendingRecord
{
    public int PendingId { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty; // Create, Update, Delete
    public int? RecordId { get; set; }
    public string RecordData { get; set; } = string.Empty;
    public int MakerId { get; set; }
    public string MakerName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public int? CheckerId { get; set; }
    public string? CheckerName { get; set; }
    public string? CheckerComments { get; set; }
    public DateTime? AuthorizedDate { get; set; }
}
