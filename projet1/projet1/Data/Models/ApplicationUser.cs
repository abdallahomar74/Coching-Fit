using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace projet1.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
    
        [Required]
        public int Age { get; set; }

        [Required]
        public float Weight { get; set; }

        [Required]
        public float Height { get; set; }

        [Required]
        public string Gender { get; set; }
        public byte[]? Image { get; set; }


    }
}
