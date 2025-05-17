using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using projet1.Data;
using projet1.Models;
using projet1.Services;

namespace projet1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
          
        }

        [Authorize(Roles = "User")]
        [HttpGet("SubscriptionPlans")]
        public async Task<IActionResult> GetSubscriptionPlans()
        {
            var result = await _subscriptionService.GetSubscriptionPlansAsync();
            return result;
        }
        [Authorize]
        [HttpGet("SubscriptionPlanByCoach")]
        public async Task<IActionResult> GetSubscriptionPlanByCoach( string userName)
        {
            var result = await _subscriptionService.GetSubscriptionPlansByCoachNameAsync(userName);
            return result;
        }

        [Authorize(Roles = "User")]
        [HttpPost("Subscribe")]
        public async Task<IActionResult> SubscribeToCoach(int subscriptionPlanId)
        {
            var result = await _subscriptionService.SubscribeUserToCoachAsync(subscriptionPlanId, User);
            return result;
        }

        [Authorize]
        [HttpGet("Subscribers")]
        public async Task<IActionResult> GetSubscribers()
        {
            var subscribers = await _subscriptionService.GetSubscribersForCoachAsync(User);
            return Ok(subscribers);
        }

        [Authorize(Roles = "Coach")]
        [HttpPost("CreatePlan")]
        public async Task<IActionResult> CreatePlan([FromBody] CreateSubscriptionPlanModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _subscriptionService.CreateSubscriptionPlanAsync(model,User);
            return result;
        }

    }
}
