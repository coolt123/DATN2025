using System.ComponentModel.DataAnnotations;

namespace DATN.Dtos.UserDtos
{
    public class ResetPasswordRequest
    {
        [Required]
        public string userId { get; set; }

        [Required]
        public string token { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự.")]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
