using DATN.Dtos.CartDto;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DATN.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // GET: api/cart
        [HttpGet]
        public async Task<IActionResult> GetUserCart()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetUserCartAsync(userId);
            return Ok(cart);
        }

        // POST: api/cart
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = GetUserId();
            await _cartService.AddToCartAsync(userId, dto);
            return Ok(new { message = "Thêm vào giỏ hàng thành công." });
        }

        // PUT: api/cart/update-quantity
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartQuantityDto dto)
        {
            await _cartService.UpdateQuantityAsync(dto);
            return Ok(new { message = "Cập nhật số lượng thành công." });
        }

        // DELETE: api/cart/product/5
        [HttpDelete("product/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = GetUserId();
            await _cartService.RemoveFromCartAsync(userId, productId);
            return Ok(new { message = "Đã xoá sản phẩm khỏi giỏ hàng." });
        }

        // DELETE: api/cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            await _cartService.ClearCartAsync(userId);
            return Ok(new { message = "Đã xoá toàn bộ giỏ hàng." });
        }
    }
}

