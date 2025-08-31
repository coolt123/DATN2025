using Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using DATN.Services.ResetPass; 

namespace DATN.Services.ResetPass
{
    public class PasswordService : IPasswordService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<PasswordService> _logger;

        public PasswordService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IEmailSender emailSender,
            ILogger<PasswordService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

     
        public async Task<string> ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                _logger.LogWarning("Thông tin reset mật khẩu không hợp lệ.");
                return "Thông tin reset mật khẩu không hợp lệ.";
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning($"Không tìm thấy người dùng với email: {email}");
                    return "Không tìm thấy người dùng.";
                }

                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Người dùng {email} đã đổi mật khẩu thành công.");
                    return "Đổi mật khẩu thành công.";
                }

                _logger.LogWarning($"Lỗi đổi mật khẩu cho người dùng {email}: {string.Join(", ", result.Errors)}");
                return "Đổi mật khẩu không thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi đổi mật khẩu: {ex.Message}");
                throw new InvalidOperationException("Lỗi hệ thống khi đặt lại mật khẩu.", ex);
            }
        }
    }
}
