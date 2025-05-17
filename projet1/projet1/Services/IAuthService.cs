using Microsoft.AspNetCore.Identity;
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
        Task<AuthModel> UpdateUserProfileAsync([FromBody] UpdateProfileDataModel model, ClaimsPrincipal currentUser);
        Task<ForgotPasswordResult> ForgotPasswordAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(string email, string encodedToken, string newPassword);
        Task<GetUserDataModel> GetProfileAsync(ClaimsPrincipal currentUser);
        Task<List<GetUserDataModel>> GetCoachesAsync();

    }
}
