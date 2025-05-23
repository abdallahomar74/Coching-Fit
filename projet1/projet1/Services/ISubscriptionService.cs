﻿using Microsoft.AspNetCore.Mvc;
using projet1.Data.Models;
using projet1.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace projet1.Services
{
    public interface ISubscriptionService
    {
        Task<List<GetTrainerSubscribedData>> GetSubscribersForCoachAsync(ClaimsPrincipal coachId);
        Task<ActionResult> GetSubscriptionPlansByCoachNameAsync(string userName);
        Task<ActionResult> SubscribeUserToCoachAsync( int subscriptionPlanId,ClaimsPrincipal user);
        Task<ActionResult> GetSubscriptionPlansAsync();
        Task<ActionResult> CreateSubscriptionPlanAsync(CreateSubscriptionPlanModel model, ClaimsPrincipal user);
    }
}
