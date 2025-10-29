using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

/// <summary>
/// Refactored MakerCheckerRepository using generic DbHelper
/// </summary>
public class MakerCheckerRepositoryRefactored : IMakerCheckerRepository
{
    private readonly DbHelper _dbHelper;

    public MakerCheckerRepositoryRefactored(DbHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<IEnumerable<PendingRecord>> GetPendingRecordsAsync()
    {
        return await _dbHelper.ExecuteStoredProcedureListAsync(
            StoredProcedureNames.MakerChecker.GetPendingRecords,
            MapPendingRecordFromReader
        );
    }

    public async Task<IEnumerable<PendingRecord>> GetApprovedRecordsAsync()
    {
        return await _dbHelper.ExecuteStoredProcedureListAsync(
            StoredProcedureNames.MakerChecker.GetApprovedRecords,
            MapPendingRecordFromReader
        );
    }

    public async Task<IEnumerable<PendingRecord>> GetRejectedRecordsAsync()
    {
        return await _dbHelper.ExecuteStoredProcedureListAsync(
            StoredProcedureNames.MakerChecker.GetRejectedRecords,
            MapPendingRecordFromReader
        );
    }

    public async Task<PendingRecord?> GetByIdAsync(int recordId)
    {
        return await _dbHelper.ExecuteStoredProcedureSingleAsync(
            StoredProcedureNames.MakerChecker.GetRecordById,
            MapPendingRecordFromReader,
            DbHelper.CreateParameter("@RecordId", recordId)
        );
    }

    public async Task<int> CreatePendingRecordAsync(PendingRecord record)
    {
        return await _dbHelper.ExecuteStoredProcedureWithOutputAsync<int>(
            StoredProcedureNames.MakerChecker.CreatePendingRecord,
            "@RecordId",
            System.Data.SqlDbType.Int,
            DbHelper.CreateParameter("@RecordType", record.RecordType),
            DbHelper.CreateParameter("@Operation", record.Operation),
            DbHelper.CreateParameter("@RecordData", record.RecordData),
            DbHelper.CreateParameter("@Status", record.Status),
            DbHelper.CreateParameter("@MakerId", record.MakerId),
            DbHelper.CreateParameter("@MakerName", record.MakerName)
        );
    }

    public async Task<bool> ApproveRecordAsync(int recordId, int checkerId, string checkerName, string? checkerComments)
    {
        var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
            StoredProcedureNames.MakerChecker.ApproveRecord,
            DbHelper.CreateParameter("@RecordId", recordId),
            DbHelper.CreateParameter("@CheckerId", checkerId),
            DbHelper.CreateParameter("@CheckerName", checkerName),
            DbHelper.CreateParameter("@CheckerComments", checkerComments)
        );

        return rowsAffected > 0;
    }

    public async Task<bool> RejectRecordAsync(int recordId, int checkerId, string checkerName, string? checkerComments)
    {
        var rowsAffected = await _dbHelper.ExecuteStoredProcedureNonQueryAsync(
            StoredProcedureNames.MakerChecker.RejectRecord,
            DbHelper.CreateParameter("@RecordId", recordId),
            DbHelper.CreateParameter("@CheckerId", checkerId),
            DbHelper.CreateParameter("@CheckerName", checkerName),
            DbHelper.CreateParameter("@CheckerComments", checkerComments)
        );

        return rowsAffected > 0;
    }

    private static PendingRecord MapPendingRecordFromReader(SqlDataReader reader)
    {
        return new PendingRecord
        {
            RecordId = reader.GetInt32(reader.GetOrdinal("RecordId")),
            RecordType = reader.GetString(reader.GetOrdinal("RecordType")),
            Operation = reader.GetString(reader.GetOrdinal("Operation")),
            RecordData = reader.GetString(reader.GetOrdinal("RecordData")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            MakerId = reader.GetInt32(reader.GetOrdinal("MakerId")),
            MakerName = reader.GetString(reader.GetOrdinal("MakerName")),
            CheckerId = reader.IsDBNull(reader.GetOrdinal("CheckerId"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("CheckerId")),
            CheckerName = reader.IsDBNull(reader.GetOrdinal("CheckerName"))
                ? null
                : reader.GetString(reader.GetOrdinal("CheckerName")),
            CheckerComments = reader.IsDBNull(reader.GetOrdinal("CheckerComments"))
                ? null
                : reader.GetString(reader.GetOrdinal("CheckerComments")),
            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
            ReviewedDate = reader.IsDBNull(reader.GetOrdinal("ReviewedDate"))
                ? null
                : reader.GetDateTime(reader.GetOrdinal("ReviewedDate"))
        };
    }
}
