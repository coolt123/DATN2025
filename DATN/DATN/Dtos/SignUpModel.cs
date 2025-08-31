using System.ComponentModel.DataAnnotations;

namespace DATN.Dtos
{
    public class SignUpModel
    {
        [Required]
        public string FullName { get; set; } = null!;
       

        [Required, EmailAddress]

        public string Email { get; set; } = null!;
        [Required]
        [MinLength(3, ErrorMessage = "Mật khẩu chứa tối thiểu 8 ký tự")]
        public string Password { get; set; } = null!;
        [Required]
        public string ConfirmPassword { get; set; } = null!; 
        [Required(AllowEmptyStrings = false, ErrorMessage = " không được bỏ trống địa chỉ cụ thể")]
        public string AddressLine { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = " không được bỏ trống số điện thoại")]
        public string PhoneNumber { get; set; }
       



    }
}
