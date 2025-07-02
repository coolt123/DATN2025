namespace DATN.Services.ResetPass
{
    public interface IPasswordService
    {
        Task<string> ResetPasswordAsync(string email, string token, string newPassword);
    }
}
