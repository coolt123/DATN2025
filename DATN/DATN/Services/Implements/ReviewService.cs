using AutoMapper;
using AutoMapper.QueryableExtensions;
using DATN.DbContexts;
using DATN.Dtos.ReviewDto;
using DATN.Entities;
using DATN.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DATN.Services.Implements
{
    public class ReviewService : IReviewService
    {
        private readonly Data _context;
        private readonly IMapper _mapper;

        public ReviewService(Data context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async  Task<ReviewDto> AddReviewAsync(string userId, ReviewCreateDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                throw new Exception("Sản phẩm không tồn tại");
            }

            var review = new Review
            {
                IdUser = userId,
                ProductId = dto.ProductId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedDate = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return _mapper.Map<ReviewDto>(review);

        }

        public Task<bool> DeleteReviewAsync(string userId, int reviewId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<ReviewDto>> GetReviewsByProductIdAsync(int productId)
        {
            return await _context.Reviews
               .Where(r => r.ProductId == productId)
               .Include(r => r.User)
               .Include(r => r.Product)
               .ProjectTo<ReviewDto>(_mapper.ConfigurationProvider)
               .ToListAsync();
        }

        public Task<ReviewDto> UpdateReviewAsync(string userId, ReviewUpdateDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
