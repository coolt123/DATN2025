using Identity.Entities;

namespace DATN.Dtos.Resetpassdto
{
    public class EmailVerificationCode
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Code { get; set; }
        public DateTime ExpiredAt { get; set; }

        public ApplicationUser User { get; set; }
    }
}
