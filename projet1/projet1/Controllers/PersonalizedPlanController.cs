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
        [HttpPost("publishPersonalizedPlan")]
        public async Task<IActionResult> PublishForSubscriber( string subscriberUserName, IFormFile file)
        {
            try
            {
                await _personalizedPlanService.AddPersonalizedPlanAsync(User, subscriberUserName, file);
                return Ok("The plan has been successfully published to the subscriber.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet("myPlans")]
        public async Task<IActionResult> MyPlans()
        {
            var plans = await _personalizedPlanService.GetPlansForSubscriberAsync(User);
            var result = plans.Select(p => new {
                p.Id,
                p.FileName,
                FileUrl = $"{Request.Scheme}://{Request.Host}/{p.FilePath}",
                p.FileType,
                p.PublishedDate
            });
            if (!plans.Any())
                return NotFound("There are no plans for you at this time.");
            return Ok(result);
        }

    }

}
