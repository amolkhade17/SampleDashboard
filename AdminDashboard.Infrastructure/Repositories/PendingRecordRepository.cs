using System.Data;
using System.Data.SqlClient;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;

namespace AdminDashboard.Infrastructure.Repositories;

public class PendingRecordRepository : IPendingRecordRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public PendingRecordRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(PendingRecord pendingRecord)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_CreatePendingRecord";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@RecordType", pendingRecord.RecordType));
        command.Parameters.Add(new SqlParameter("@Operation", pendingRecord.Operation));
        command.Parameters.Add(new SqlParameter("@RecordId", (object?)pendingRecord.RecordId ?? DBNull.Value));
        command.Parameters.Add(new SqlParameter("@RecordData", pendingRecord.RecordData));
        command.Parameters.Add(new SqlParameter("@MakerId", pendingRecord.MakerId));
        command.Parameters.Add(new SqlParameter("@MakerName", pendingRecord.MakerName));

        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<PendingRecord?> GetByIdAsync(int pendingId)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_GetPendingRecordById";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@PendingId", pendingId));

        connection.Open();
        using var reader = await ((SqlCommand)command).ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<PendingRecord>> GetAllAsync(string? status = null)
    {
        var records = new List<PendingRecord>();

        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_GetAllPendingRecords";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@Status", (object?)status ?? DBNull.Value));

        connection.Open();
        using var reader = await ((SqlCommand)command).ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            records.Add(MapFromReader(reader));
        }

        return records;
    }

    public async Task<int> ApproveAsync(int pendingId, int checkerId, string checkerName, string? checkerComments)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_ApprovePendingRecord";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@PendingId", pendingId));
        command.Parameters.Add(new SqlParameter("@CheckerId", checkerId));
        command.Parameters.Add(new SqlParameter("@CheckerName", checkerName));
        command.Parameters.Add(new SqlParameter("@CheckerComments", (object?)checkerComments ?? DBNull.Value));

        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> RejectAsync(int pendingId, int checkerId, string checkerName, string checkerComments)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_RejectPendingRecord";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@PendingId", pendingId));
        command.Parameters.Add(new SqlParameter("@CheckerId", checkerId));
        command.Parameters.Add(new SqlParameter("@CheckerName", checkerName));
        command.Parameters.Add(new SqlParameter("@CheckerComments", checkerComments));

        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task<int> ExecuteApprovedUserOperationAsync(int pendingId)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();

        command.CommandText = "SP_ExecuteApprovedUserOperation";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@PendingId", pendingId));

        connection.Open();
        var result = await ((SqlCommand)command).ExecuteScalarAsync();

        return Convert.ToInt32(result);
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
