using DATN.DbContexts;
using DATN.Dtos;
using DATN.Entities;
using Microsoft.AspNetCore.Authorization;
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
        public async Task<IActionResult> LogView(int dto)
        {
            var sessionId = HttpContext.Session.GetString("SessionId");

            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("SessionId", sessionId);
            }
            var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var entity = new ProductView
            {
                ProductId = dto,
                UserId = userId,
                SessionId = sessionId,
                ViewedAt = DateTime.UtcNow
            };

            _context.ProductViews.Add(entity);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
