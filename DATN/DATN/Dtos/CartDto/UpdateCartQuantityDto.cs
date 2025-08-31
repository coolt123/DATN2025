namespace DATN.Dtos.CartDto
{
    public class UpdateCartQuantityDto
    {
        public int CartId { get; set; }
        public int Quantity { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}
