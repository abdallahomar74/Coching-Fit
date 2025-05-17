using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using projet1.Services;
using System.ComponentModel.DataAnnotations;

namespace projet1.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IAuthService _auth;
        private readonly ILogger<ResetPasswordModel> _logger;

        public ResetPasswordModel(IAuthService auth, ILogger<ResetPasswordModel> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        [BindProperty] public string Email { get; set; } = string.Empty;
        [BindProperty] public string Token { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        public string? Message { get; set; }
        public List<string> Errors { get; } = new();

        public void OnGet(string email, string token)
        {
            Email = email;
            Token = token;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _logger.LogInformation($"ResetPassword POST - Email: {Email}");

            var result = await _auth.ResetPasswordAsync(Email, Token, NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    Errors.Add(e.Description);

                ModelState.AddModelError(string.Empty, "Password reset failed.");
                return Page();
            }

            _logger.LogInformation("Password reset successful");
            return RedirectToPage("/ResetPasswordSuccess");
        }
    }
}
