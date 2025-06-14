using System.ComponentModel.DataAnnotations;
namespace projet1.Models

{
    public class UploadCVModel
    {
        [Required(ErrorMessage = "Please select cv file !!")]
        public IFormFile CVFile { get; set; }
    }
}
