using DATN.Dtos;
using DATN.Dtos.CartDto;
using DATN.Exceptions;
using DATN.Helpers;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new AppException("Vui lòng đăng nhập để có thể xem giỏ hàng.", (int)HttpStatusCode.Unauthorized);
            return userId;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCart()
        {
            var userId = GetUserId();
            var cart = await _cartService.GetUserCartAsync(userId);
            return ResponseHelper.ResponseSuccess(cart, "Lấy giỏ hàng thành công.");
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
        {
            var userId = GetUserId();
            await _cartService.AddToCartAsync(userId, dto);
            return ResponseHelper.ResponseSuccess<object>(null, "Thêm vào giỏ hàng thành công.");
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartQuantityDto dto)
        {
            await _cartService.UpdateQuantityAsync(dto);
            return ResponseHelper.ResponseSuccess<object>(null, "Cập nhật số lượng thành công.");
        }

        [HttpDelete("product/{productId}")]
        public async Task<IActionResult> RemoveFromCartByProduct(int productId)
        {
            var userId = GetUserId();
            await _cartService.RemoveFromCartAsync(userId, productId);
            return ResponseHelper.ResponseSuccess<object>(null, "Đã xoá sản phẩm khỏi giỏ hàng.");
        }

        [HttpDelete("item/{cartId}")]
        public async Task<IActionResult> RemoveFromCartById(int cartId)
        {
            await _cartService.RemoveFromCartAsync(cartId);
            return ResponseHelper.ResponseSuccess<object>(null, "Đã xoá mục khỏi giỏ hàng.");
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            await _cartService.ClearCartAsync(userId);
            return ResponseHelper.ResponseSuccess<object>(null, "Đã xoá toàn bộ giỏ hàng.");
        }
    }
}
