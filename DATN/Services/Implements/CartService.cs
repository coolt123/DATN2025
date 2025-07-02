using AutoMapper;
using DATN.DbContexts;
using DATN.Dtos.CartDto;
using DATN.Dtos.CartDto;
using DATN.Entities;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Services
{
    public class CartService : ICartService
    {
        private readonly Data _context;
        private readonly IMapper _mapper;

        public CartService(Data context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper;
        }

        public async Task<List<CartDto>> GetUserCartAsync(string userId)
        {
            var carts = await _context.Carts
                .Where(c => c.IdUser == userId)
                .Include(c => c.Product)
                .ToListAsync();

            return _mapper.Map<List<CartDto>>(carts);
        }

        public async Task<CartDto> GetCartItemAsync(string userId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.IdUser == userId && c.ProductId == productId);

            return _mapper.Map<CartDto>(cart);
        }

        public async Task AddToCartAsync(string userId, AddToCartDto dto)
        {
            var existingItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.IdUser == userId && c.ProductId == dto.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                var cart = new Cart
                {
                    IdUser = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
               
                };
                await _context.Carts.AddAsync(cart);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(UpdateCartQuantityDto dto)
        {
            var item = await _context.Carts.FindAsync(dto.CartId);
            if (item != null)
            {
                item.Quantity = dto.Quantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(int cartId)
        {
            var item = await _context.Carts.FindAsync(cartId);
            if (item != null)
            {
                _context.Carts.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var items = await _context.Carts.Where(c => c.IdUser == userId).ToListAsync();
            _context.Carts.RemoveRange(items);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveFromCartAsync(string userId, int productId)
        {
            var item = await _context.Carts
                .FirstOrDefaultAsync(c => c.IdUser == userId && c.ProductId == productId);

            if (item != null)
            {
                _context.Carts.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

      
    }
}
