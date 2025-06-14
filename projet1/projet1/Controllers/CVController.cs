using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using projet1.Models;
using projet1.Services;

namespace projet1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Coach")] // يتطلب تسجيل الدخول
    public class CVController : ControllerBase
    {
        private readonly IAuthService _authService;

        public CVController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// رفع السيرة الذاتية للمدربين
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadCV([FromForm] UploadCVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.UploadCoachCVAsync(model, User);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// الحصول على السيرة الذاتية للمستخدم الحالي
        /// </summary>
        [HttpGet("my-cv")]
        public async Task<IActionResult> GetMyCV()
        {
            var result = await _authService.GetCoachCVAsync(User);

            if (result.Succeeded)
                return Ok(result);

            return NotFound(result);
        }

        /// <summary>
        /// حذف السيرة الذاتية
        /// </summary>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteCV()
        {
            var result = await _authService.DeleteCoachCVAsync(User);

            if (result.Succeeded)
                return Ok(result);

            return BadRequest(result);
        }
    }
}