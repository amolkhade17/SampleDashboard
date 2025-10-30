using AdminDashboard.Domain.Entities;

namespace AdminDashboard.Domain.Interfaces;

public interface IUploadedFileRepository
{
    Task<int> CreateAsync(UploadedFile file);
    Task<IEnumerable<UploadedFile>> GetAllAsync();
    Task<UploadedFile?> GetByIdAsync(int fileId);
    Task<UploadedFile?> GetByFileNameAsync(string fileName);
    Task<bool> UpdateAsync(int fileId, string fileName, string modifiedBy);
    Task<bool> DeleteAsync(string fileName, string deletedBy);
    Task<FileStatistics> GetStatisticsAsync();
}

public class FileStatistics
{
    public int TotalFiles { get; set; }
    public long TotalSize { get; set; }
    public DateTime? LastUploadDate { get; set; }
    public int TotalUploaders { get; set; }
}
