using DATN.DbContexts;
using DATN.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductViewController : ControllerBase
    {
        private readonly Data _context;

        public ProductViewController(Data context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> LogView([FromBody] ProductView model)
        {
            model.ViewedAt = DateTime.UtcNow;
            _context.ProductViews.Add(model);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
