using Microsoft.AspNetCore.Mvc;

namespace DATN.Dtos.ProductDto
{
    public class ImportProductsRequest
    {
        [FromForm(Name = "excelFile")]
        public IFormFile ExcelFile { get; set; }

        [FromForm(Name = "images")]
        public List<IFormFile> Images { get; set; }
    }
}
