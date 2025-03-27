using System.ComponentModel.DataAnnotations;

namespace projet1.Models
{
    public class RegisterModel
    {
        [Required,StringLength(60)]
        public string UserName { get; set; }
        [Required, StringLength(50)]
        public string Email { get; set; }
        [Required, StringLength(100)]
        public string Password { get; set; }
        [Required]
        public int Age { get; set; }
        [Required]
        public float Weight { get; set; }
        [Required]
        public float Height { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public bool IsCoach { get; set; } 

    }
}
