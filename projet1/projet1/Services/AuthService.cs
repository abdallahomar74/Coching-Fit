using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using projet1.Data.Models;
using projet1.Helpers;
using projet1.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace projet1.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JWT _jwt;


        public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt)
        {
            this._userManager = userManager;
            _jwt = jwt.Value;
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
                Image = user.Image
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
                Image = user.Image
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

            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                Message = "Image updated successfully.",
                Email = user.Email!,
                UserName = user.UserName!,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                IsAuthenticated = true,
                Image = stream.ToArray(),
            };
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
