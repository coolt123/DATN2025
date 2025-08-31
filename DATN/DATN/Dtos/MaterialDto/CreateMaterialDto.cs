using System.ComponentModel.DataAnnotations;

namespace DATN.Dtos.MaterialDto
{
    public class CreateMaterialDto
    {
        [Required]
        public string Name { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
