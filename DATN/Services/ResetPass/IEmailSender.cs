namespace DATN.Services.ResetPass
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

}
