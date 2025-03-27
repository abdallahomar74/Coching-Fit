using System.ComponentModel.DataAnnotations;

namespace projet1.Models
{
    public class UpdateImageModel
    {
        [Required]
        public IFormFile Image { get; set; }
    }
}
