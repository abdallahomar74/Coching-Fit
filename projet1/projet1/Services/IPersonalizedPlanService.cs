using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace projet1.Services
{
    public interface IPersonalizedPlanService
    {
        Task<object> AddPersonalizedPlanAsync(ClaimsPrincipal user, [FromQuery] string subscriberUserName, [FromForm] IFormFile file);
        Task<object> GetPlansForSubscriberAsync(ClaimsPrincipal user);
        Task<object> GetPlanDownloadUrlAsync(ClaimsPrincipal user, [FromQuery] int planId);
    }
}