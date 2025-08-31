namespace DATN.Dtos.StyleDto
{
    public class StyleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateStyleDto
    {
        public string Name { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public class UpdateStyleDto
    {
        public string Name { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}
