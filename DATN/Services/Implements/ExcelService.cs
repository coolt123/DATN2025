
//using OfficeOpenXml;
//using DATN.DbContexts;

//using DATN.Dtos.Excel;
//using DATN.Entities;
//using DATN.Services.Interfaces;

//namespace DATN.Services.Implements
//{
//    public class ExcelService : IExcelService
//    {
//        private readonly Data _context;
//        public ExcelService(Data context) 
//        {
//            _context = context;
//        }
//        public async Task<List<CreateAuthorDto>> ImportExcelAsync(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                throw new ArgumentException("File không hợp lệ");

//            var extension = Path.GetExtension(file.FileName).ToLower();
//            if (extension != ".xlsx" && extension != ".xls")
//                throw new Exception("Chỉ hỗ trợ file Excel (.xlsx, .xls)");

//            // Đặt license context cho EPPlus
//            ExcelPackage.License.SetNonCommercialPersonal("pham kim the");
//            var result = new List<CreateAuthorDto>();

//            using (var stream = new MemoryStream())
//            {
//                await file.CopyToAsync(stream);
//                using (var package = new ExcelPackage(stream))
//                {
//                    var worksheet = package.Workbook.Worksheets[0]; // sheet đầu tiên
//                    var rowCount = worksheet.Dimension.Rows;

//                    for (int row = 2; row <= rowCount; row++) // bỏ dòng tiêu đề
//                    {
//                        var name = worksheet.Cells[row, 1].Text?.Trim();

//                        // Bỏ qua nếu tên trống
//                        if (string.IsNullOrWhiteSpace(name)) continue;

//                        var dto = new CreateAuthorDto
//                        {
//                            NameAuthor = name
//                        };
//                        result.Add(dto);

//                        // Tạo entity để lưu vào DB
//                        var author = new Author
//                        {
//                            NameAuthor = name
//                        };

//                        _context.authors.Add(author); // Thêm vào DbSet<Author>
//                    }

//                    await _context.SaveChangesAsync(); // Lưu vào database sau khi thêm tất cả
//                }
//            }

//            return result; // Trả về danh sách các DTO đã import (nếu cần hiển thị hoặc log)
//        }

//    }
//}
