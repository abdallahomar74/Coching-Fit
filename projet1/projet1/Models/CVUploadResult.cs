namespace projet1.Models
{
    public class CVUploadResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public string? FileId { get; set; }
        public string? FileName { get; set; }
        public string? ViewUrl { get; set; }
        public string? DownloadUrl { get; set; }
        public DateTime? UploadDate { get; set; }

    }
}
