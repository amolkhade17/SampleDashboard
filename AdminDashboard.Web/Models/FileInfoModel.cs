namespace AdminDashboard.Web.Models;

public class FileInfoModel
{
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadDate { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
}
