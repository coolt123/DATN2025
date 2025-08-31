using Identity.Entities;

namespace DATN.Entities
{
    public class CustomerLoyalty : Time
    {
        public int LoyaltyId { get; set; }
        public string IdUser { get; set; }
        public int Points { get; set; }
        public string Tier { get; set; }
        public DateTime LastUpdated { get; set; }
        public ApplicationUser User { get; set; }
    }
}
