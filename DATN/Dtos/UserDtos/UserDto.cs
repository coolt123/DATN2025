using DATN.Entities;

namespace DATN.Dtos.UserDtos
{
    public class UserDto
    {
        public string IdUser { get; set; }
        public string NameUser { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public  List <string> Role { get; set; }

    }
}
