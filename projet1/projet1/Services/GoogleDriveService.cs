using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Mvc;

namespace projet1.Services
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly DriveService _driveService;
        private readonly string _folderId;

        public GoogleDriveService(IConfiguration configuration)
        {
            var serviceAccountPath = configuration["GoogleDrive:ServiceAccountPath"];
            var applicationName = configuration["GoogleDrive:ApplicationName"];

            // إعداد الاعتماد
            GoogleCredential credential;
            using (var stream = new FileStream(serviceAccountPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.Scope.Drive);
            }

            // إنشاء خدمة Drive
            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            // إنشاء أو الحصول على المجلد
            _folderId = GetOrCreateFolderAsync(configuration["GoogleDrive:FolderName"]).Result;
        }

        private async Task<string> GetOrCreateFolderAsync(string folderName)
        {
            // البحث عن المجلد
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"name='{folderName}' and mimeType='application/vnd.google-apps.folder' and trashed=false";
            var files = await listRequest.ExecuteAsync();

            if (files.Files.Any())
            {
                return files.Files.First().Id;
            }

            // إنشاء مجلد جديد
            var folderMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var request = _driveService.Files.Create(folderMetadata);
            var folder = await request.ExecuteAsync();

            // جعل المجلد عام للقراءة
            await MakeFilePublicAsync(folder.Id);

            return folder.Id;
        }

        public async Task<(string fileId, string viewUrl, string downloadUrl)> UploadFileAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { _folderId }
            };

            using var stream = file.OpenReadStream();
            var request = _driveService.Files.Create(fileMetadata, stream, file.ContentType);
            request.Fields = "id";

            var uploadedFile = await request.UploadAsync();
            if (uploadedFile.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                throw new Exception($"Upload failed: {uploadedFile.Exception?.Message}");
            }

            var fileId = request.ResponseBody.Id;

            // جعل الملف عام للقراءة
            await MakeFilePublicAsync(fileId);

            var viewUrl = GetFileViewUrlAsync(fileId).Result;
            var downloadUrl = GetFileDownloadUrlAsync(fileId).Result;

            return (fileId, viewUrl, downloadUrl);
        }

        private async Task MakeFilePublicAsync(string fileId)
        {
            var permission = new Google.Apis.Drive.v3.Data.Permission()
            {
                Role = "reader",
                Type = "anyone"
            };

            await _driveService.Permissions.Create(permission, fileId).ExecuteAsync();
        }

        public async Task<bool> DeleteFileAsync(string fileId)
        {
            try
            {
                await _driveService.Files.Delete(fileId).ExecuteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetFileViewUrlAsync(string fileId)
        {
            return await Task.FromResult($"https://drive.google.com/file/d/{fileId}/view");
        }

        public async Task<string> GetFileDownloadUrlAsync(string fileId)
        {
            return await Task.FromResult($"https://drive.google.com/uc?export=download&id={fileId}");
        }

        public void Dispose()
        {
            _driveService?.Dispose();
        }
    }
}