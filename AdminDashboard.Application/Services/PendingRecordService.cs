using AdminDashboard.Application.DTOs;
using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using System.Text.Json;

namespace AdminDashboard.Application.Services;

public interface IPendingRecordService
{
    Task<int> CreatePendingRecordAsync(CreatePendingRecordDto dto, int makerId, string makerName);
    Task<PendingRecordDto?> GetByIdAsync(int pendingId);
    Task<IEnumerable<PendingRecordDto>> GetAllAsync(string? status = null);
    Task<(bool Success, string Message)> ApproveAsync(int pendingId, int checkerId, string checkerName, string? comments);
    Task<(bool Success, string Message)> RejectAsync(int pendingId, int checkerId, string checkerName, string comments);
    Task<(bool Success, string Message)> ExecuteApprovedOperationAsync(int pendingId);
}

public class PendingRecordService : IPendingRecordService
{
    private readonly IPendingRecordRepository _pendingRecordRepository;

    public PendingRecordService(IPendingRecordRepository pendingRecordRepository)
    {
        _pendingRecordRepository = pendingRecordRepository;
    }

    public async Task<int> CreatePendingRecordAsync(CreatePendingRecordDto dto, int makerId, string makerName)
    {
        var pendingRecord = new PendingRecord
        {
            RecordType = dto.RecordType,
            Operation = dto.Operation,
            RecordId = dto.RecordId,
            RecordData = dto.RecordData,
            MakerId = makerId,
            MakerName = makerName,
            CreatedDate = DateTime.Now,
            Status = "Pending"
        };

        return await _pendingRecordRepository.CreateAsync(pendingRecord);
    }

    public async Task<PendingRecordDto?> GetByIdAsync(int pendingId)
    {
        var record = await _pendingRecordRepository.GetByIdAsync(pendingId);
        if (record == null) return null;

        return MapToDto(record);
    }

    public async Task<IEnumerable<PendingRecordDto>> GetAllAsync(string? status = null)
    {
        var records = await _pendingRecordRepository.GetAllAsync(status);
        return records.Select(MapToDto);
    }

    public async Task<(bool Success, string Message)> ApproveAsync(int pendingId, int checkerId, string checkerName, string? comments)
    {
        try
        {
            // Get the pending record to check maker ID
            var pendingRecord = await _pendingRecordRepository.GetByIdAsync(pendingId);
            
            if (pendingRecord == null)
            {
                return (false, "Pending record not found");
            }

            if (pendingRecord.Status != "Pending")
            {
                return (false, "Record has already been processed");
            }

            if (pendingRecord.MakerId == checkerId)
            {
                return (false, "Checker cannot be the same as Maker");
            }

            var result = await _pendingRecordRepository.ApproveAsync(pendingId, checkerId, checkerName, comments);
            
            if (result > 0)
            {
                return (true, "Record approved successfully");
            }
            
            return (false, "Failed to approve record");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> RejectAsync(int pendingId, int checkerId, string checkerName, string comments)
    {
        try
        {
            // Get the pending record to check maker ID
            var pendingRecord = await _pendingRecordRepository.GetByIdAsync(pendingId);
            
            if (pendingRecord == null)
            {
                return (false, "Pending record not found");
            }

            if (pendingRecord.Status != "Pending")
            {
                return (false, "Record has already been processed");
            }

            if (pendingRecord.MakerId == checkerId)
            {
                return (false, "Checker cannot be the same as Maker");
            }

            var result = await _pendingRecordRepository.RejectAsync(pendingId, checkerId, checkerName, comments);
            
            if (result > 0)
            {
                return (true, "Record rejected successfully");
            }
            
            return (false, "Failed to reject record");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> ExecuteApprovedOperationAsync(int pendingId)
    {
        try
        {
            var result = await _pendingRecordRepository.ExecuteApprovedUserOperationAsync(pendingId);
            
            if (result > 0)
            {
                return (true, "Operation executed successfully");
            }
            
            return (false, "Failed to execute operation");
        }
        catch (Exception ex)
        {
            return (false, $"Error: {ex.Message}");
        }
    }

    private static PendingRecordDto MapToDto(PendingRecord record)
    {
        return new PendingRecordDto
        {
            PendingId = record.PendingId,
            RecordType = record.RecordType,
            Operation = record.Operation,
            RecordId = record.RecordId,
            RecordData = record.RecordData,
            MakerId = record.MakerId,
            MakerName = record.MakerName,
            CreatedDate = record.CreatedDate,
            Status = record.Status,
            CheckerId = record.CheckerId,
            CheckerName = record.CheckerName,
            CheckerComments = record.CheckerComments,
            AuthorizedDate = record.AuthorizedDate
        };
    }
}
