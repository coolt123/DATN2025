using DATN.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string AddressLine { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public ICollection<CustomerLoyalty> CustomerLoyalties { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
