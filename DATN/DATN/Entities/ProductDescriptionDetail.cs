namespace DATN.Entities
{
    public class ProductDescriptionDetail
    {
        public int IdDescription { get; set; }
        public int ProductId { get; set; }

        public string Title { get; set; }       
        public string Detail { get; set; }     
        public string? ImageUrl { get; set; }    

        public Product Product { get; set; }
    }
}
