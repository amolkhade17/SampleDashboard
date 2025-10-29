using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

public class PendingRecordRepository : IPendingRecordRepository
{
    private readonly DbHelperWithConfig _dbHelper;

    public PendingRecordRepository(DbHelperWithConfig dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<int> CreateAsync(PendingRecord pendingRecord)
    {
        return await _dbHelper.ExecuteNonQueryAsync(
            entity: "MakerChecker",
            operation: "CreateRequest",
            parameterValues: new Dictionary<string, object?>
            {
                { "@RecordType", pendingRecord.RecordType },
                { "@Operation", pendingRecord.Operation },
                { "@RecordId", pendingRecord.RecordId ?? (object)DBNull.Value },
                { "@RecordData", pendingRecord.RecordData },
                { "@MakerId", pendingRecord.MakerId },
                { "@MakerName", pendingRecord.MakerName }
            }
        );
    }

    public async Task<PendingRecord?> GetByIdAsync(int pendingId)
    {
        return await _dbHelper.ExecuteSingleAsync<PendingRecord>(
            entity: "MakerChecker",
            operation: "GetRequestById",
            mapper: MapFromReader,
            parameterValues: new Dictionary<string, object?>
            {
                { "@PendingId", pendingId }
            }
        );
    }

    public async Task<IEnumerable<PendingRecord>> GetAllAsync(string? status = null)
    {
        return await _dbHelper.ExecuteListAsync<PendingRecord>(
            entity: "MakerChecker",
            operation: "GetPendingRequests",
            mapper: MapFromReader,
            parameterValues: new Dictionary<string, object?>
            {
                { "@Status", status ?? (object)DBNull.Value }
            }
        );
    }

    public async Task<int> ApproveAsync(int pendingId, int checkerId, string checkerName, string? checkerComments)
    {
        return await _dbHelper.ExecuteNonQueryAsync(
            entity: "MakerChecker",
            operation: "ApproveRequest",
            parameterValues: new Dictionary<string, object?>
            {
                { "@PendingId", pendingId },
                { "@CheckerId", checkerId },
                { "@CheckerName", checkerName },
                { "@CheckerComments", checkerComments ?? (object)DBNull.Value }
            }
        );
    }

    public async Task<int> RejectAsync(int pendingId, int checkerId, string checkerName, string checkerComments)
    {
        return await _dbHelper.ExecuteNonQueryAsync(
            entity: "MakerChecker",
            operation: "RejectRequest",
            parameterValues: new Dictionary<string, object?>
            {
                { "@PendingId", pendingId },
                { "@CheckerId", checkerId },
                { "@CheckerName", checkerName },
                { "@CheckerComments", checkerComments }
            }
        );
    }

    public async Task<int> ExecuteApprovedUserOperationAsync(int pendingId)
    {
        // This method would need custom implementation
        // For now, just return success
        return await Task.FromResult(1);
    }

    private static PendingRecord MapFromReader(IDataReader reader)
    {
        return new PendingRecord
        {
            PendingId = reader.GetInt32(reader.GetOrdinal("PendingId")),
            RecordType = reader.GetString(reader.GetOrdinal("RecordType")),
            Operation = reader.GetString(reader.GetOrdinal("Operation")),
            RecordId = reader.IsDBNull(reader.GetOrdinal("RecordId")) ? null : reader.GetInt32(reader.GetOrdinal("RecordId")),
            RecordData = reader.GetString(reader.GetOrdinal("RecordData")),
            MakerId = reader.GetInt32(reader.GetOrdinal("MakerId")),
            MakerName = reader.GetString(reader.GetOrdinal("MakerName")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            CheckerId = reader.IsDBNull(reader.GetOrdinal("CheckerId")) ? null : reader.GetInt32(reader.GetOrdinal("CheckerId")),
            CheckerName = reader.IsDBNull(reader.GetOrdinal("CheckerName")) ? null : reader.GetString(reader.GetOrdinal("CheckerName")),
            CheckerComments = reader.IsDBNull(reader.GetOrdinal("CheckerComments")) ? null : reader.GetString(reader.GetOrdinal("CheckerComments")),
            AuthorizedDate = reader.IsDBNull(reader.GetOrdinal("AuthorizedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("AuthorizedDate"))
        };
    }
}
