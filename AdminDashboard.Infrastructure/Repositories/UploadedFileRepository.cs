using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace AdminDashboard.Infrastructure.Repositories;

public class UploadedFileRepository : IUploadedFileRepository
{
    private readonly DbHelperWithConfig _dbHelper;

    public UploadedFileRepository(DbHelperWithConfig dbHelper)
    {
        _dbHelper = dbHelper;
    }

    public async Task<int> CreateAsync(UploadedFile file)
    {
        var fileId = await _dbHelper.ExecuteScalarAsync<decimal>(
            entity: "File",
            operation: "Create",
            parameterValues: new Dictionary<string, object?>
            {
                { "@FileName", file.FileName },
                { "@OriginalFileName", file.OriginalFileName },
                { "@FilePath", file.FilePath },
                { "@FileSize", file.FileSize },
                { "@FileExtension", file.FileExtension },
                { "@MimeType", file.MimeType },
                { "@UploadedBy", file.UploadedBy }
            }
        );
        return Convert.ToInt32(fileId);
    }

    public async Task<IEnumerable<UploadedFile>> GetAllAsync()
    {
        return await _dbHelper.ExecuteListAsync<UploadedFile>(
            entity: "File",
            operation: "GetAll",
            mapper: MapUploadedFileFromReader
        );
    }

    public async Task<UploadedFile?> GetByIdAsync(int fileId)
    {
        return await _dbHelper.ExecuteSingleAsync<UploadedFile>(
            entity: "File",
            operation: "GetById",
            mapper: MapUploadedFileFromReader,
            parameterValues: new Dictionary<string, object?>
            {
                { "@FileId", fileId }
            }
        );
    }

    public async Task<UploadedFile?> GetByFileNameAsync(string fileName)
    {
        return await _dbHelper.ExecuteSingleAsync<UploadedFile>(
            entity: "File",
            operation: "GetByFileName",
            mapper: MapUploadedFileFromReader,
            parameterValues: new Dictionary<string, object?>
            {
                { "@FileName", fileName }
            }
        );
    }

    public async Task<bool> UpdateAsync(int fileId, string fileName, string modifiedBy)
    {
        await _dbHelper.ExecuteNonQueryAsync(
            entity: "File",
            operation: "Update",
            parameterValues: new Dictionary<string, object?>
            {
                { "@FileId", fileId },
                { "@FileName", fileName },
                { "@ModifiedBy", modifiedBy }
            }
        );
        return true;
    }

    public async Task<bool> DeleteAsync(string fileName, string deletedBy)
    {
        await _dbHelper.ExecuteNonQueryAsync(
            entity: "File",
            operation: "Delete",
            parameterValues: new Dictionary<string, object?>
            {
                { "@FileName", fileName },
                { "@DeletedBy", deletedBy }
            }
        );
        return true;
    }

    public async Task<FileStatistics> GetStatisticsAsync()
    {
        return await _dbHelper.ExecuteSingleAsync<FileStatistics>(
            entity: "File",
            operation: "GetStatistics",
            mapper: MapStatisticsFromReader
        ) ?? new FileStatistics();
    }

    private UploadedFile MapUploadedFileFromReader(SqlDataReader reader)
    {
        return new UploadedFile
        {
            FileId = reader.GetInt32(reader.GetOrdinal("FileId")),
            FileName = reader.GetString(reader.GetOrdinal("FileName")),
            OriginalFileName = reader.GetString(reader.GetOrdinal("OriginalFileName")),
            FilePath = reader.GetString(reader.GetOrdinal("FilePath")),
            FileSize = reader.GetInt64(reader.GetOrdinal("FileSize")),
            FileExtension = reader.GetString(reader.GetOrdinal("FileExtension")),
            MimeType = reader.IsDBNull(reader.GetOrdinal("MimeType")) ? null : reader.GetString(reader.GetOrdinal("MimeType")),
            UploadedBy = reader.GetString(reader.GetOrdinal("UploadedBy")),
            UploadedDate = reader.GetDateTime(reader.GetOrdinal("UploadedDate")),
            ModifiedBy = reader.IsDBNull(reader.GetOrdinal("ModifiedBy")) ? null : reader.GetString(reader.GetOrdinal("ModifiedBy")),
            ModifiedDate = reader.IsDBNull(reader.GetOrdinal("ModifiedDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ModifiedDate")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted"))
        };
    }

    private FileStatistics MapStatisticsFromReader(SqlDataReader reader)
    {
        return new FileStatistics
        {
            TotalFiles = reader.IsDBNull(reader.GetOrdinal("TotalFiles")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalFiles")),
            TotalSize = reader.IsDBNull(reader.GetOrdinal("TotalSize")) ? 0 : reader.GetInt64(reader.GetOrdinal("TotalSize")),
            LastUploadDate = reader.IsDBNull(reader.GetOrdinal("LastUploadDate")) ? null : reader.GetDateTime(reader.GetOrdinal("LastUploadDate")),
            TotalUploaders = reader.IsDBNull(reader.GetOrdinal("TotalUploaders")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalUploaders"))
        };
    }
}
