namespace DATN.Entities
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPublished { get; set; }
    }
}
