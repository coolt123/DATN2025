using System.ComponentModel.DataAnnotations;

namespace DATN.Dtos.CategoriesDto
{
    public class CreateCategoryDto
    {
        [Required]
        public string NameCategory { get; set; }    
    }
}
