using Microsoft.AspNetCore.Mvc;
using projet1.Data.Models;
using System.Security.Claims;

namespace projet1.Services
{
    public interface IPersonalizedPlanService
    {
        Task AddPersonalizedPlanAsync(ClaimsPrincipal user, string subscriberUserName, [FromForm] IFormFile file);
        Task<List<PersonalizedPlan>> GetPlansForSubscriberAsync(ClaimsPrincipal user);
    }
}
