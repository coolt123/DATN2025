using System.ComponentModel.DataAnnotations;

namespace DATN.Dtos.Resetpassdto
{ 
    public class ResetPasswordModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
        [Required(ErrorMessage = "password is required")]
        public string Password { get; set; }
        [Compare("Password", ErrorMessage ="the password and confirmpassword is not match")]
        public string ConfirmPassword { get; set; }
    }
}
