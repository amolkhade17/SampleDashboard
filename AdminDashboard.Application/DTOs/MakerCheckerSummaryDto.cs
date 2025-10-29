namespace AdminDashboard.Application.DTOs;

public class MakerCheckerSummaryDto
{
    public string RecordType { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal ApprovalRate { get; set; }
    public int AvgProcessingTimeHours { get; set; }
}
