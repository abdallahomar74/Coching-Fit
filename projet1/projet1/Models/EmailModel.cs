using System.ComponentModel.DataAnnotations;

namespace projet1.Models
{
    public class EmailModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
