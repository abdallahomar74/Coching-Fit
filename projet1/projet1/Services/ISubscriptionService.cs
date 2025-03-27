using Microsoft.AspNetCore.Mvc;
using projet1.Data.Models;
using projet1.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace projet1.Services
{
    public interface ISubscriptionService
    {
        Task<ActionResult> SubscribeUserToCoachAsync(int subscriptionPlanId,ClaimsPrincipal user);
        Task<ActionResult<List<GetTrainerSubscribedData>>> GetSubscribersForCoachAsync(ClaimsPrincipal user);
        Task<ActionResult> CreateSubscriptionPlanAsync(CreateSubscriptionPlanModel model, ClaimsPrincipal user);
        Task<ActionResult> GetSubscriptionPlansAsync(); 
    }
}
