using DATN.Entities;

namespace DATN.Dtos.UserDtos
{
    public class UserDto
    {
        public string IdUser { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AddresLine { get; set; }
        public string PhoneNumber { get; set; }

    }
}
