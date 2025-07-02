using System.ComponentModel.DataAnnotations;

namespace DATN.Dtos.CategoriesDto
{
    public class DeleteCategoryDto
    {
        [Required]
        public int CategoryId { get; set; }
    }
}
