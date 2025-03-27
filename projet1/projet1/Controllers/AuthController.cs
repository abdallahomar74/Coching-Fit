using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using projet1.Models;
using projet1.Services;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;

namespace projet1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody]RegisterModel model)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);
            var reslut = await _authService.RegisterAsync(model);
            if (!reslut.IsAuthenticated)
                return BadRequest(reslut.Message);
            return Ok(reslut);
        }
        [HttpPost("token")]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var reslut = await _authService.GetTokenAsync(model);
            if (!reslut.IsAuthenticated)
                return BadRequest(reslut.Message);
            return Ok(reslut);
        }
        [Authorize]
        [HttpGet("GetProfile")]
        public async Task<IActionResult> GetProfileAsync()
        {
            var reslut = await _authService.GetProfileAsync(User);
            return Ok(reslut);
        }
        [HttpGet("Coaches")]
        public async Task<IActionResult> GetCoachesAsync()
        {
            var Coaches = await _authService.GetCoachesAsync();
            return Ok(Coaches);
        }
        [Authorize]
        [HttpPatch("EditImage")]
        public async Task<IActionResult> UpdateUserImageAsync([FromForm] UpdateImageModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _authService.UpdateUserImageAsync(model, User);
            if (!response.IsAuthenticated)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }

    }
}
