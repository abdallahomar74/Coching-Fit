namespace projet1.Services
{
    public interface IAppEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
