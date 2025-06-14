namespace projet1.Services
{
    public interface IGoogleDriveService
    {
        Task<(string fileId, string viewUrl, string downloadUrl)> UploadFileAsync(IFormFile file);
        Task<bool> DeleteFileAsync(string fileId);
        Task<string> GetFileViewUrlAsync(string fileId);
        Task<string> GetFileDownloadUrlAsync(string fileId);
    }
}