namespace DATN.Dtos.ReviewDto
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public string IdUser { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }   
        public string ProductName { get; set; }
    }
}
