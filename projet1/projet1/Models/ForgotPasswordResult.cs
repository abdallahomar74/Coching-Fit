namespace projet1.Models
{
    public class ForgotPasswordResult
    {
        public bool Succeeded { get; set; }
        public string? Token { get; set; }     
        public string? Message { get; set; }
    }
}
