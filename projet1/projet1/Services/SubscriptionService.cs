using projet1.Data;
using projet1.Data.Models;
using Microsoft.EntityFrameworkCore;
using projet1.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace projet1.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> GetSubscriptionPlansAsync()
        {
            var plans = await _context.subscriptionplans.ToListAsync();

            return new OkObjectResult(plans);
        }

        public async Task<ActionResult> SubscribeUserToCoachAsync(int subscriptionPlanId,ClaimsPrincipal user)
        {
            var subscriberId = user.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(subscriberId))
                return new UnauthorizedObjectResult("This Coach is UnAuthorized");

            // Check if the subscriber already has an active (non-expired) subscription.
            var activeSubscription = await _context.coachsubscriptions
                .Where(cs => cs.SubscriberId == subscriberId && cs.ExpirationDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (activeSubscription != null)
            {
                // User already has an active subscription; cannot subscribe to another coach.
                return new BadRequestObjectResult("User already has an active subscription!");
            }

            // Retrieve the subscription plan details.
            var plan = await _context.subscriptionplans.FindAsync(subscriptionPlanId);
            if (plan == null)
            {
                return new BadRequestObjectResult("There's no Subscription plan with This id ");
            }

            // Create a new subscription record.
            var newSubscription = new CoachSubscription
            {
                SubscriptionPlanId = subscriptionPlanId,
                SubscriberId = subscriberId,
                SubscriptionDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(plan.DurationInDays)
            };

            _context.coachsubscriptions.Add(newSubscription);
            await _context.SaveChangesAsync();
            return new OkObjectResult("Subscribed successfully.");
        }

        // Retrieve a list of all subscribers for a given coach.
        public async Task<ActionResult<List<GetTrainerSubscribedData>>> GetSubscribersForCoachAsync(ClaimsPrincipal user)
        {
            var coachId = user.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(coachId))
                return new UnauthorizedObjectResult("This Coach is UnAuthorized");


            var subscribers = await _context.coachsubscriptions
                .Where(cs => cs.SubscriptionPlan.CoachId == coachId)
                .Select(cs => cs.Subscriber)
                .ToListAsync();

            var result = subscribers.Select(s => new GetTrainerSubscribedData
            {
                UserName = s.UserName,
                Email = s.Email,
                Gender = s.Gender,
                Age = s.Age,
                Weight = s.Weight,
                Height = s.Height
            }).ToList();

           return new OkObjectResult(result);
        }

        public async Task<ActionResult> CreateSubscriptionPlanAsync(CreateSubscriptionPlanModel model,ClaimsPrincipal user )
        {
            var coachId = user.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(coachId))
                return new UnauthorizedObjectResult("This Coach is UnAuthorized");

            var plan = new SubscriptionPlan
            {
                CoachId = coachId,
                Price = model.Price,
                DurationInDays = model.DurationInDays,
                Details = model.Details,
                CreatedDate = DateTime.UtcNow
            };

            _context.subscriptionplans.Add(plan); 
            var result = await _context.SaveChangesAsync();
            return new OkObjectResult(result);
        }

        
    }
}
