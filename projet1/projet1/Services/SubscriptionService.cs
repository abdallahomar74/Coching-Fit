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
            var plans = await _context.subscriptionplans
         .Select(plan => new
         {
             plan.Id,
             Coach = new
             {
                 plan.Coach.UserName,
                 plan.Coach.Email
             },
             plan.Price,
             plan.DurationInDays,
             plan.Details,
             plan.CreatedDate,
            
         })
         .ToListAsync();

            return new OkObjectResult(plans);
        }
        public async Task<ActionResult> GetSubscriptionPlansByCoachNameAsync( string userName)
        {
            

            var plans = await _context.subscriptionplans.Where(cs => cs.Coach.UserName == userName)
         .Select(plan => new
         {
             plan.Id,
             Coach = new
             {
                 plan.Coach.UserName,
                 plan.Coach.Email
             },
             plan.Price,
             plan.DurationInDays,
             plan.Details,
             plan.CreatedDate,

         })
         .ToListAsync();

            return new OkObjectResult(plans);
        }

        public async Task<ActionResult> SubscribeUserToCoachAsync([FromQuery] int subscriptionPlanId,ClaimsPrincipal user)
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
        public async Task<List<GetTrainerSubscribedData>> GetSubscribersForCoachAsync(ClaimsPrincipal user)
        {
            var coachId = user.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(coachId))
                return new List<GetTrainerSubscribedData>
                {
                    new GetTrainerSubscribedData
                    {
                        Message = "The coach is Unauthenticated"       
                    }
                };

            var subscribers = await _context.coachsubscriptions
                .Where(cs => cs.SubscriptionPlan.CoachId == coachId)
                .Select(cs => cs.Subscriber)
                .ToListAsync();
            if (subscribers == null || subscribers.Count == 0)
            {
                return new List<GetTrainerSubscribedData>
                {
                    new GetTrainerSubscribedData
                    {
                        Message = "The coach does not have subscribers"       
                    }
                };
            }


            return subscribers.Select(s => new GetTrainerSubscribedData
            {
                UserName = s.UserName!,
                Email = s.Email!,
                Gender = s.Gender,
                Age = s.Age,
                Weight = s.Weight,
                Height = s.Height
            }).ToList();
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
