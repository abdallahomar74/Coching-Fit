using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using projet1.Data.Models;
using projet1.Helpers;
using projet1.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace projet1.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWT _jwt;
        private readonly IAppEmailSender _emailSender;
        private readonly string _frontendUrl;
        IServiceProvider _serviceProvider;


        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, IAppEmailSender emailSender, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            this._userManager = userManager;
            _jwt = jwt.Value;
            _emailSender = emailSender;
            _frontendUrl = configuration["FrontendUrl"];
            _serviceProvider = serviceProvider;
        }
        


        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already Registered!" };

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                Age = model.Age,
                Height = model.Height,
                Weight = model.Weight,
                Gender = model.Gender.ToLower()
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description},";
                }
                return new AuthModel { Message = errors };
            }
            await _userManager.AddToRoleAsync(user, model.IsCoach ? "Coach" : "User");
            var jwtSecurityToken = await CreateJwtToken(user);
            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                UserName = user.UserName,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            };
        }



        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user ==  null || !await _userManager.CheckPasswordAsync(user,model.Password) )
            {
                authModel.Message = "Email or Password is incorrect!";
                return authModel;
            }
            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email!;
            authModel.UserName = user.UserName!;
            authModel.Roles = rolesList.ToList();
            authModel.Image = user.Image;

            return authModel;
        }



        public async Task<GetUserDataModel> GetProfileAsync(ClaimsPrincipal currentUser)
        {
            var userId = currentUser.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return new GetUserDataModel { Message = "User not found or not authenticated." };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new GetUserDataModel { Message = "User not found." };

            return new GetUserDataModel
            {
                Message = "Welecome Back",
                Email = user.Email!,
                UserName = user.UserName!,
                Age = user.Age,
                Height = user.Height,
                Weight = user.Weight,
                Gender = user.Gender,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                Image = user.Image,
                CVFileName = user.CVFileName,
                CVViewUrl = user.CVViewUrl,
                CVDownloadUrl = user.CVDownloadUrl,
                CVUploadDate = user.CVUploadDate
            };
        }



        public async Task<List<GetUserDataModel>> GetCoachesAsync()
        {
        
            var coaches = await _userManager.GetUsersInRoleAsync("Coach");

            var CoachesDtos = coaches.Select(user => new GetUserDataModel
            {
                UserName = user.UserName!,
                Email = user.Email!,
                Age = user.Age,
                Weight = user.Weight,
                Height = user.Height,
                Gender = user.Gender,
                Image = user.Image,
                CVFileName = user.CVFileName,
                CVViewUrl = user.CVViewUrl,
                CVDownloadUrl = user.CVDownloadUrl,
                CVUploadDate = user.CVUploadDate
            }).ToList();

            return CoachesDtos;
        }



        public async Task<AuthModel> UpdateUserImageAsync([FromForm] UpdateImageModel model, ClaimsPrincipal currentUser)
        {
            using var stream = new MemoryStream();
            await model.Image.CopyToAsync(stream);

            var userId = currentUser.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return new AuthModel { Message = "User not found or not authenticated." };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthModel { Message = "User not found." };
            
            user.Image = stream.ToArray();

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthModel { Message = errors };
            }


            return new AuthModel
            {
                Message = "Image updated successfully.",
                Image = stream.ToArray(),
                IsAuthenticated = true
            };
        }

        public async Task<AuthModel> UpdateUserProfileAsync([FromBody] UpdateProfileDataModel model, ClaimsPrincipal currentUser)
        {

            var userId = currentUser.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
                return new AuthModel { Message = "User not found or not authenticated." };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthModel { Message = "User not found." };
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.Weight = model.Weight;
            user.Height = model.Height;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthModel { Message = errors };
            }

            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                Message = "Profile Updated Successfully. ",
                Email = user.Email!,
                UserName = user.UserName!,
                IsAuthenticated = true,
            };
        }


        public async Task<ForgotPasswordResult> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new()
                {
                    Succeeded = true,
                    Message = "If that email is registered you will receive a link."
                };                                               // hide existence :contentReference[oaicite:0]{index=0}

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var link = $"{_frontendUrl}/ResetPassword?email={email}&token={encodedToken}";

            await _emailSender.SendEmailAsync(email, "Reset Password",
                $"Click here to reset your password: <a href='{link}'>Reset</a>"
            );                                                // send email :contentReference[oaicite:1]{index=1}

            return new()
            {
                Succeeded = true,
                Token = encodedToken,    // so you can test without email
                Message = "Reset link sent."
            };
        }


        public async Task<IdentityResult> ResetPasswordAsync(string email, string encodedToken, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "User not found" });

            // فك ترميز الـ Token بشكل صحيح
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(encodedToken);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            return await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
        }


        public async Task<CVUploadResult> UploadCoachCVAsync(UploadCVModel model, ClaimsPrincipal currentUser)
        {
            try
            {
                // Get current user
                var userId = currentUser.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return new CVUploadResult { Succeeded = false, Message = "User not found or not authenticated." };

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new CVUploadResult { Succeeded = false, Message = "User not found." };

                // Check if user is a coach
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Coach"))
                    return new CVUploadResult { Succeeded = false, Message = "Only coaches can upload CVs." };

                // Validate file
                if (model.CVFile == null || model.CVFile.Length == 0)
                    return new CVUploadResult { Succeeded = false, Message = "Please select a valid CV file." };

                // Check file type (allow PDF, DOC, DOCX)
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var fileExtension = Path.GetExtension(model.CVFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                    return new CVUploadResult { Succeeded = false, Message = "Only PDF, DOC, and DOCX files are allowed." };

                // Check file size (max 10MB)
                if (model.CVFile.Length > 10 * 1024 * 1024)
                    return new CVUploadResult { Succeeded = false, Message = "File size must be less than 10MB." };

                // Delete existing CV if exists
                if (!string.IsNullOrEmpty(user.CVFileId))
                {
                    var googleDriveService = _serviceProvider.GetRequiredService<IGoogleDriveService>();
                    await googleDriveService.DeleteFileAsync(user.CVFileId);
                }

                // Upload to Google Drive
                var driveService = _serviceProvider.GetRequiredService<IGoogleDriveService>();
                var (fileId, viewUrl, downloadUrl) = await driveService.UploadFileAsync(model.CVFile);

                // Update user with CV information
                user.CVFileId = fileId;
                user.CVFileName = model.CVFile.FileName;
                user.CVViewUrl = viewUrl;
                user.CVDownloadUrl = downloadUrl;
                user.CVUploadDate = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    // If user update failed, try to delete the uploaded file
                    await driveService.DeleteFileAsync(fileId);
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new CVUploadResult { Succeeded = false, Message = $"Failed to update user: {errors}" };
                }

                return new CVUploadResult
                {
                    Succeeded = true,
                    Message = "CV uploaded successfully.",
                    FileId = fileId,
                    FileName = model.CVFile.FileName,
                    ViewUrl = viewUrl,
                    DownloadUrl = downloadUrl,
                    UploadDate = user.CVUploadDate
                };
            }
            catch (Exception ex)
            {
                return new CVUploadResult { Succeeded = false, Message = $"An error occurred: {ex.Message}" };
            }
        }


        public async Task<CVUploadResult> GetCoachCVAsync(ClaimsPrincipal currentUser)
        {
            try
            {
                var userId = currentUser.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return new CVUploadResult { Succeeded = false, Message = "User not found or not authenticated." };

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new CVUploadResult { Succeeded = false, Message = "User not found." };

                if (string.IsNullOrEmpty(user.CVFileId))
                    return new CVUploadResult { Succeeded = false, Message = "No CV found." };

                return new CVUploadResult
                {
                    Succeeded = true,
                    Message = "CV retrieved successfully.",
                    FileId = user.CVFileId,
                    FileName = user.CVFileName,
                    ViewUrl = user.CVViewUrl,
                    DownloadUrl = user.CVDownloadUrl,
                    UploadDate = user.CVUploadDate
                };
            }
            catch (Exception ex)
            {
                return new CVUploadResult { Succeeded = false, Message = $"An error occurred: {ex.Message}" };
            }
        }



        public async Task<CVUploadResult> DeleteCoachCVAsync(ClaimsPrincipal currentUser)
        {
            try
            {
                var userId = currentUser.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return new CVUploadResult { Succeeded = false, Message = "User not found or not authenticated." };

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new CVUploadResult { Succeeded = false, Message = "User not found." };

                if (string.IsNullOrEmpty(user.CVFileId))
                    return new CVUploadResult { Succeeded = false, Message = "No CV found to delete." };

                // Delete from Google Drive
                var driveService = _serviceProvider.GetRequiredService<IGoogleDriveService>();
                var deleted = await driveService.DeleteFileAsync(user.CVFileId);

                // Update user (remove CV info even if Drive deletion failed)
                user.CVFileId = null;
                user.CVFileName = null;
                user.CVViewUrl = null;
                user.CVDownloadUrl = null;
                user.CVUploadDate = null;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new CVUploadResult { Succeeded = false, Message = $"Failed to update user: {errors}" };
                }

                return new CVUploadResult
                {
                    Succeeded = true,
                    Message = deleted ? "CV deleted successfully." : "CV removed from profile (file may still exist in Drive)."
                };
            }
            catch (Exception ex)
            {
                return new CVUploadResult { Succeeded = false, Message = $"An error occurred: {ex.Message}" };
            }
        }


        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
    }
}
