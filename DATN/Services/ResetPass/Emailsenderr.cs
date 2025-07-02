using System.Net.Mail;
using System.Net;

namespace DATN.Services.ResetPass
{
    public class Emailsenderr : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Emailsenderr> _logger;

        public Emailsenderr(IConfiguration configuration, ILogger<Emailsenderr> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendEmailAsync(string Email, string subject, string body)
        {
            _logger.LogInformation("📧 Bắt đầu gửi email đến: {Email}", Email);

            try
            {
                // Lấy các giá trị cấu hình từ appsettings.json
                var host = _configuration["Emailconfiguration:Host"];
                var portStr = _configuration["Emailconfiguration:Port"];
                var username = _configuration["Emailconfiguration:Username"];
                var password = _configuration["Emailconfiguration:Password"];
                var enableSslStr = _configuration["Emailconfiguration:EnableSSL"];

                // Log cấu hình email
                _logger.LogInformation("📌 Cấu hình SMTP - Host: {Host}, Port: {Port}, Username: {Username}, EnableSSL: {EnableSSL}",
                    host, portStr, username, enableSslStr);

                // Kiểm tra giá trị null hoặc rỗng
                if (string.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host), "⚠️ Emailconfiguration:Host bị null hoặc rỗng!");
                if (string.IsNullOrEmpty(portStr)) throw new ArgumentNullException(nameof(portStr), "⚠️ Emailconfiguration:Port bị null hoặc rỗng!");
                if (string.IsNullOrEmpty(username)) throw new ArgumentNullException(nameof(username), "⚠️ Emailconfiguration:Username bị null hoặc rỗng!");
                if (string.IsNullOrEmpty(password)) throw new ArgumentNullException(nameof(password), "⚠️ Emailconfiguration:Password bị null hoặc rỗng!");
                if (string.IsNullOrEmpty(enableSslStr)) throw new ArgumentNullException(nameof(enableSslStr), "⚠️ Emailconfiguration:EnableSSL bị null hoặc rỗng!");

                // Chuyển đổi kiểu dữ liệu
                if (!int.TryParse(portStr, out var port)) throw new FormatException($"⚠️ Giá trị Port '{portStr}' không hợp lệ!");
                if (!bool.TryParse(enableSslStr, out var enableSsl)) throw new FormatException($"⚠️ Giá trị EnableSSL '{enableSslStr}' không hợp lệ!");

                _logger.LogInformation("✅ Cấu hình SMTP hợp lệ.");

                // Khởi tạo SmtpClient với cấu hình
                using var smtpClient = new SmtpClient(host, port)
                {
                    EnableSsl = enableSsl,
                    Credentials = new NetworkCredential(username, password),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                // Kiểm tra dữ liệu email trước khi gửi
                if (string.IsNullOrEmpty(Email)) throw new ArgumentNullException(nameof(Email), "⚠️ Địa chỉ email người nhận không được để trống!");
                if (string.IsNullOrEmpty(subject)) throw new ArgumentNullException(nameof(subject), "⚠️ Tiêu đề email không được để trống!");
                if (string.IsNullOrEmpty(body)) throw new ArgumentNullException(nameof(body), "⚠️ Nội dung email không được để trống!");

                _logger.LogInformation("📌 Chuẩn bị gửi email đến {Email}", Email);

                // Tạo mail message
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(username, "Admin"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(Email);

                // Gửi email
                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation("Email đã gửi thành công đến: {Email}", Email);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "Lỗi SMTP khi gửi email đến: {Email}. Mã lỗi: {StatusCode}", Email, smtpEx.StatusCode);
                throw new InvalidOperationException("Lỗi SMTP khi gửi email.", smtpEx);
            }
            catch (ArgumentNullException argEx)
            {
                _logger.LogError(argEx, "Lỗi tham số null khi gửi email đến: {Email}", Email);
                throw new InvalidOperationException("Lỗi tham số null khi gửi email.", argEx);
            }
            catch (FormatException formatEx)
            {
                _logger.LogError(formatEx, "Lỗi định dạng trong cấu hình email.");
                throw new InvalidOperationException("Lỗi định dạng trong cấu hình email.", formatEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi gửi email đến: {Email}", Email);
                throw new InvalidOperationException("Không thể gửi email do lỗi không xác định.", ex);
            }
        }
    }
}
