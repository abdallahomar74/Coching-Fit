using projet1.Data;
using projet1.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;


namespace projet1.Services
{
    public class PersonalizedPlanService : IPersonalizedPlanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PersonalizedPlanService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task AddPersonalizedPlanAsync(ClaimsPrincipal user, string subscriberUserName,[FromForm] IFormFile file)
        {
            var coachId = user.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(coachId))
                 new UnauthorizedObjectResult("This Coach is UnAuthorized");


            var subscriber = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == subscriberUserName);
            if (subscriber == null)
                throw new Exception("Subscriber not found.");


            var sub = await _context.coachsubscriptions
                .Include(cs => cs.SubscriptionPlan)
                .FirstOrDefaultAsync(cs => cs.SubscriberId == subscriber.Id
                    && cs.SubscriptionPlan.CoachId == coachId
                    && cs.ExpirationDate > DateTime.UtcNow);
            if (sub == null)
                throw new Exception("Invalid or expired subscription.");

            string root = _environment.WebRootPath
              ?? _environment.ContentRootPath;
            string uploads = Path.Combine(root, "uploads", "personalized");

            Directory.CreateDirectory(uploads);
            string uniqueName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string path = Path.Combine(uploads, uniqueName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            var relativePath = $"uploads/personalized/{uniqueName}";

            var plan = new PersonalizedPlan
            {
                CoachSubscriptionId = sub.Id,
                FileName = file.FileName,
                FilePath = relativePath,
                FileType = Path.GetExtension(file.FileName).TrimStart('.'),
                PublishedDate = DateTime.UtcNow
            };

            _context.PersonalizedPlans.Add(plan);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PersonalizedPlan>> GetPlansForSubscriberAsync(ClaimsPrincipal user)
        {
            var subscriberId = user.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(subscriberId))
                new UnauthorizedObjectResult("This User is UnAuthorized");

            var sub = await _context.coachsubscriptions
                .Where(cs => cs.SubscriberId == subscriberId && cs.ExpirationDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();
            if (sub == null) return new List<PersonalizedPlan>();

            return await _context.PersonalizedPlans
                .Where(pp => pp.CoachSubscriptionId == sub.Id)
                .OrderByDescending(pp => pp.PublishedDate)
                .ToListAsync();
        }
    }
}
