using AdminDashboard.Domain.Entities;

namespace AdminDashboard.Domain.Interfaces;

public interface IPendingRecordRepository
{
    Task<int> CreateAsync(PendingRecord pendingRecord);
    Task<PendingRecord?> GetByIdAsync(int pendingId);
    Task<IEnumerable<PendingRecord>> GetAllAsync(string? status = null);
    Task<int> ApproveAsync(int pendingId, int checkerId, string checkerName, string? checkerComments);
    Task<int> RejectAsync(int pendingId, int checkerId, string checkerName, string checkerComments);
    Task<int> ExecuteApprovedUserOperationAsync(int pendingId);
}
