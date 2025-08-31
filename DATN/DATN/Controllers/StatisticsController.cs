using DATN.DbContexts;
using DATN.Entities;
using DATN.Helpers;
using Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly Data _context;

        public StatisticsController(Data context)
        {
            _context = context;
        }

        [HttpGet("revenue")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetRevenueByMonth()
        {
            var result = await _context.Orders
                .Where(o => o.Status == "Complete")
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(x => x.TotalAmount)
                }).ToListAsync();

            return Ok(new
            {
                statusCode = 200,
                message = "Thống kê doanh thu theo tháng thành công",
                data = result
            });
        }
        [HttpGet("revenue/summary")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetRevenueSummary()
        {
            var today = DateTime.Today;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            var totalToday = await _context.Orders.Where(o => o.OrderDate.Date == today && o.Status == "Complete").SumAsync(o => o.TotalAmount);
            var totalWeek = await _context.Orders.Where(o => o.OrderDate >= startOfWeek && o.Status == "Complete").SumAsync(o => o.TotalAmount);
            var totalMonth = await _context.Orders.Where(o => o.OrderDate >= startOfMonth && o.Status == "Complete").SumAsync(o => o.TotalAmount);
            var totalYear = await _context.Orders.Where(o => o.OrderDate >= startOfYear && o.Status == "Complete").SumAsync(o => o.TotalAmount);

            var data = new
            {
                today = totalToday,
                week = totalWeek,
                month = totalMonth,
                year = totalYear
            };

            return ResponseHelper.ResponseSuccess(data, "Tổng doanh thu");
        }

        [HttpGet("revenue/by-month")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetRevenueByMonth(int year)
        {
            var data = await _context.Orders
                .Where(o => o.OrderDate.Year == year && o.Status == "Complete")
                .GroupBy(o => o.OrderDate.Month)
                .Select(g => new {
                    Month = g.Key,
                    TotalRevenue = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

           return  ResponseHelper.ResponseSuccess(data);
        }

        [HttpGet("top-products")]
        [Authorize(Roles = AppRole.Admin)]
        public async Task<IActionResult> GetTopProducts()
        {
            var result = await _context.OrderDetails
                 .Where(d => d.Order.Status == "Complete")
                .GroupBy(d => d.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(10)
                .ToListAsync();

            return ResponseHelper.ResponseSuccess(result, "Top 10 sản phẩm bán chạy");
        }
    }
}
