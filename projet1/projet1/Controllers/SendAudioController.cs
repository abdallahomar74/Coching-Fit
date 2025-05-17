using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace projet1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendAudioController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public SendAudioController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [Authorize]
        [HttpPost("upload-audio")]
        public async Task<IActionResult> UploadAudio(IFormFile file)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "audio");
            Directory.CreateDirectory(uploads);
            var name = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(uploads, name);
            using var fs = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(fs);
            // return the relative path your Hub expects
            return Ok(new { filePath = $"uploads/audio/{name}" });
        }
    }
}
