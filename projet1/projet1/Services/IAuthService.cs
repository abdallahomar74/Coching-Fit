using Microsoft.AspNetCore.Mvc;
using projet1.Data.Models;
using projet1.Models;
using System.Security.Claims;

namespace projet1.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> GetTokenAsync(TokenRequestModel model);
        Task<AuthModel> UpdateUserImageAsync([FromForm] UpdateImageModel model, ClaimsPrincipal currentUser);
        Task<GetUserDataModel> GetProfileAsync(ClaimsPrincipal currentUser);
        Task<List<GetUserDataModel>> GetCoachesAsync();
    }
}
