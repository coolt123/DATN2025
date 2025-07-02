using System.ComponentModel.DataAnnotations;

namespace DATN.Dtos.Resetpassdto
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        
        public string? Email { get; set; }
        [Required]
        public string? ClientUrl { get; set; }
    }

}
