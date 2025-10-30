using AdminDashboard.Domain.Entities;
using AdminDashboard.Domain.Interfaces;
using AdminDashboard.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AdminDashboard.Web.Controllers;

[Authorize]
public class FileUploadController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileUploadController> _logger;
    private readonly IUploadedFileRepository _fileRepository;
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip" };

    public FileUploadController(IWebHostEnvironment environment, ILogger<FileUploadController> logger, IUploadedFileRepository fileRepository)
    {
        _environment = environment;
        _logger = logger;
        _fileRepository = fileRepository;
    }

    private string GetCurrentUserName()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    // GET: FileUpload/Index
    public async Task<IActionResult> Index()
    {
        try
        {
            var dbFiles = await _fileRepository.GetAllAsync();
            var files = new List<FileInfoModel>();

            foreach (var file in dbFiles)
            {
                files.Add(new FileInfoModel
                {
                    FileName = file.FileName,
                    FileSize = FormatFileSize(file.FileSize),
                    FileSizeBytes = file.FileSize,
                    UploadDate = file.UploadedDate,
                    FileExtension = file.FileExtension,
                    FilePath = file.FilePath,
                    UploadedBy = file.UploadedBy
                });
            }

            ViewBag.MaxFileSize = _maxFileSize;
            ViewBag.AllowedExtensions = string.Join(", ", _allowedExtensions);
            return View(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading files from database");
            TempData["ErrorMessage"] = "Error loading files.";
            ViewBag.MaxFileSize = _maxFileSize;
            ViewBag.AllowedExtensions = string.Join(", ", _allowedExtensions);
            return View(new List<FileInfoModel>());
        }
    }

    // POST: FileUpload/Upload
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a file to upload.";
            return RedirectToAction(nameof(Index));
        }

        // Check file size
        if (file.Length > _maxFileSize)
        {
            TempData["ErrorMessage"] = $"File size exceeds the maximum allowed size of {FormatFileSize(_maxFileSize)}.";
            return RedirectToAction(nameof(Index));
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            TempData["ErrorMessage"] = $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            // Generate unique filename to avoid conflicts
            var uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Save file information to database
            var uploadedFile = new UploadedFile
            {
                FileName = uniqueFileName,
                OriginalFileName = file.FileName,
                FilePath = $"/uploads/{uniqueFileName}",
                FileSize = file.Length,
                FileExtension = extension,
                MimeType = GetContentType(uniqueFileName),
                UploadedBy = GetCurrentUserName(),
                UploadedDate = DateTime.Now
            };

            await _fileRepository.CreateAsync(uploadedFile);

            _logger.LogInformation($"File uploaded: {uniqueFileName} by {GetCurrentUserName()}");
            TempData["SuccessMessage"] = $"File '{file.FileName}' uploaded successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            TempData["ErrorMessage"] = $"Error uploading file: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: FileUpload/UploadMultiple
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadMultiple(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            TempData["ErrorMessage"] = "Please select at least one file to upload.";
            return RedirectToAction(nameof(Index));
        }

        var successCount = 0;
        var errorCount = 0;
        var errors = new List<string>();

        foreach (var file in files)
        {
            if (file.Length == 0)
                continue;

            // Check file size
            if (file.Length > _maxFileSize)
            {
                errors.Add($"{file.FileName}: File size exceeds maximum allowed size");
                errorCount++;
                continue;
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                errors.Add($"{file.FileName}: File type not allowed");
                errorCount++;
                continue;
            }

            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var uniqueFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                successCount++;
                _logger.LogInformation($"File uploaded: {uniqueFileName} by {GetCurrentUserName()}");
            }
            catch (Exception ex)
            {
                errors.Add($"{file.FileName}: {ex.Message}");
                errorCount++;
                _logger.LogError(ex, $"Error uploading file: {file.FileName}");
            }
        }

        if (successCount > 0)
        {
            TempData["SuccessMessage"] = $"{successCount} file(s) uploaded successfully!";
        }

        if (errorCount > 0)
        {
            TempData["ErrorMessage"] = $"{errorCount} file(s) failed to upload. " + string.Join("; ", errors);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: FileUpload/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            TempData["ErrorMessage"] = "File name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            
            if (System.IO.File.Exists(filePath))
            {
                // Delete physical file
                System.IO.File.Delete(filePath);
                
                // Update database (soft delete)
                await _fileRepository.DeleteAsync(fileName, GetCurrentUserName());
                
                _logger.LogInformation($"File deleted: {fileName} by {GetCurrentUserName()}");
                TempData["SuccessMessage"] = $"File '{fileName}' deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "File not found.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file: {fileName}");
            TempData["ErrorMessage"] = $"Error deleting file: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: FileUpload/Download
    public IActionResult Download(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            TempData["ErrorMessage"] = "File name is required.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "File not found.";
                return RedirectToAction(nameof(Index));
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            var contentType = GetContentType(fileName);
            _logger.LogInformation($"File downloaded: {fileName} by {GetCurrentUserName()}");
            
            return File(memory, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading file: {fileName}");
            TempData["ErrorMessage"] = $"Error downloading file: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: FileUpload/Rename
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename(string oldFileName, string newFileName)
    {
        if (string.IsNullOrEmpty(oldFileName) || string.IsNullOrEmpty(newFileName))
        {
            return Json(new { success = false, message = "File names are required." });
        }

        try
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            var oldFilePath = Path.Combine(uploadsPath, oldFileName);
            
            if (!System.IO.File.Exists(oldFilePath))
            {
                return Json(new { success = false, message = "Original file not found." });
            }

            // Keep the original extension
            var extension = Path.GetExtension(oldFileName);
            var newFileNameWithExt = Path.GetFileNameWithoutExtension(newFileName) + extension;
            var newFilePath = Path.Combine(uploadsPath, newFileNameWithExt);

            if (System.IO.File.Exists(newFilePath))
            {
                return Json(new { success = false, message = "A file with the new name already exists." });
            }

            // Rename physical file
            System.IO.File.Move(oldFilePath, newFilePath);
            
            // Update database
            var fileRecord = await _fileRepository.GetByFileNameAsync(oldFileName);
            if (fileRecord != null)
            {
                await _fileRepository.UpdateAsync(fileRecord.FileId, newFileNameWithExt, GetCurrentUserName());
            }
            
            _logger.LogInformation($"File renamed from {oldFileName} to {newFileNameWithExt} by {GetCurrentUserName()}");
            
            return Json(new { success = true, message = "File renamed successfully!", newFileName = newFileNameWithExt });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error renaming file: {oldFileName}");
            return Json(new { success = false, message = $"Error renaming file: {ex.Message}" });
        }
    }

    // POST: FileUpload/Copy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Copy(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return Json(new { success = false, message = "File name is required." });
        }

        try
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            var sourceFilePath = Path.Combine(uploadsPath, fileName);
            
            if (!System.IO.File.Exists(sourceFilePath))
            {
                return Json(new { success = false, message = "File not found." });
            }

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var copyFileName = $"{fileNameWithoutExt}_copy_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var destFilePath = Path.Combine(uploadsPath, copyFileName);

            System.IO.File.Copy(sourceFilePath, destFilePath);
            _logger.LogInformation($"File copied: {fileName} to {copyFileName} by {GetCurrentUserName()}");
            
            return Json(new { success = true, message = "File copied successfully!", newFileName = copyFileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error copying file: {fileName}");
            return Json(new { success = false, message = $"Error copying file: {ex.Message}" });
        }
    }

    // POST: FileUpload/DeleteMultiple
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteMultiple([FromBody] List<string> fileNames)
    {
        if (fileNames == null || fileNames.Count == 0)
        {
            return Json(new { success = false, message = "No files selected." });
        }

        try
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            var deletedCount = 0;

            foreach (var fileName in fileNames)
            {
                var filePath = Path.Combine(uploadsPath, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    deletedCount++;
                }
            }

            _logger.LogInformation($"{deletedCount} files deleted by {GetCurrentUserName()}");
            return Json(new { success = true, message = $"{deletedCount} file(s) deleted successfully!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple files");
            return Json(new { success = false, message = $"Error deleting files: {ex.Message}" });
        }
    }

    // GET: FileUpload/GetFileDetails
    public IActionResult GetFileDetails(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return Json(new { success = false, message = "File name is required." });
        }

        try
        {
            var filePath = Path.Combine(_environment.WebRootPath, "uploads", fileName);
            
            if (!System.IO.File.Exists(filePath))
            {
                return Json(new { success = false, message = "File not found." });
            }

            var fileInfo = new FileInfo(filePath);
            var details = new
            {
                success = true,
                fileName = fileInfo.Name,
                fileSize = FormatFileSize(fileInfo.Length),
                fileSizeBytes = fileInfo.Length,
                extension = fileInfo.Extension,
                created = fileInfo.CreationTime.ToString("MMM dd, yyyy hh:mm tt"),
                modified = fileInfo.LastWriteTime.ToString("MMM dd, yyyy hh:mm tt"),
                path = $"/uploads/{fileName}"
            };

            return Json(details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting file details: {fileName}");
            return Json(new { success = false, message = $"Error getting file details: {ex.Message}" });
        }
    }

    // GET: FileUpload/DownloadMultiple
    public async Task<IActionResult> DownloadMultiple([FromQuery] string[] fileNames)
    {
        if (fileNames == null || fileNames.Length == 0)
        {
            TempData["ErrorMessage"] = "No files selected.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
            var memory = new MemoryStream();

            using (var archive = new System.IO.Compression.ZipArchive(memory, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                foreach (var fileName in fileNames)
                {
                    var filePath = Path.Combine(uploadsPath, fileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        var entry = archive.CreateEntry(fileName);
                        using var entryStream = entry.Open();
                        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            memory.Position = 0;
            _logger.LogInformation($"{fileNames.Length} files downloaded as ZIP by {GetCurrentUserName()}");
            
            return File(memory, "application/zip", $"files_{DateTime.Now:yyyyMMddHHmmss}.zip");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading multiple files");
            TempData["ErrorMessage"] = $"Error downloading files: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".doc" => "application/vnd.ms-word",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".csv" => "text/csv",
            ".zip" => "application/zip",
            _ => "application/octet-stream",
        };
    }
}
