using DATN.Dtos.CartDto;
using DATN.Entities;

namespace DATN.Services.Interfaces
{
    public interface ICartService
    {
        Task<List<CartDto>> GetUserCartAsync(string userId);
        Task<CartDto> GetCartItemAsync(string userId, int productId);
        Task AddToCartAsync(string userId, AddToCartDto dto);
        Task UpdateQuantityAsync(UpdateCartQuantityDto dto);
        Task RemoveFromCartAsync(int cartId);
        Task ClearCartAsync(string userId);
        Task RemoveFromCartAsync(string userId, int productId);
    }
}
