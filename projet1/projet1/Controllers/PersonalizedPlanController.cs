using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using projet1.Services;

namespace projet1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalizedPlanController : ControllerBase
    {
        private readonly IPersonalizedPlanService _personalizedPlanService;

        public PersonalizedPlanController(IPersonalizedPlanService personalizedPlanService)
        {
            _personalizedPlanService = personalizedPlanService;
        }

        [Authorize(Roles = "Coach")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPlan([FromQuery] string subscriberUserName,  IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No file provided",
                        error = "FILE_REQUIRED"
                    });
                }

                var result = await _personalizedPlanService.AddPersonalizedPlanAsync(User, subscriberUserName, file);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message,
                    error = "UNAUTHORIZED"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = "SERVER_ERROR"
                });
            }
        }

        [HttpGet("GetSubscriberFiles")]
        public async Task<IActionResult> GetSubscriberPlans()
        {
            try
            {
                var result = await _personalizedPlanService.GetPlansForSubscriberAsync(User);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message,
                    error = "UNAUTHORIZED"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = "SERVER_ERROR"
                });
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> GetPlanDownloadUrl([FromQuery] int planId)
        {
            try
            {
                var result = await _personalizedPlanService.GetPlanDownloadUrlAsync(User, planId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = ex.Message,
                    error = "UNAUTHORIZED"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Internal server error",
                    error = "SERVER_ERROR"
                });
            }
        }
    }

}
