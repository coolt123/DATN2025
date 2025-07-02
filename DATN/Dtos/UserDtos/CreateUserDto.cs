using System.ComponentModel.DataAnnotations;

namespace DATN.Dtos.UserDtos
{
    public class CreateUserDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = " không được bỏ trống UserName")]
        [MaxLength(255)]
        public string NameUser { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = " không được bỏ trống UserName")]
        [MaxLength(50, ErrorMessage = "Tài khoản dài tối đa 50 ký tự")]
        [MinLength(3, ErrorMessage = "Tài khoản dài tối thiểu 3 ký tự")]
        public string UserName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = " không được bỏ trống Password")]
        public string Password { get; set; }
    }
}
