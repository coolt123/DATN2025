
using DATN.DbContexts;
using DATN.Dtos.ProductDto;
using DATN.Entities;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace DATN.Services.Implements
{
    public class RecommendationService : IRecommendationService
    {
        private readonly Data _context;
        private readonly IProductVectorizer _vectorizer;

        public RecommendationService(Data context, IProductVectorizer vectorizer)
        {
            _context = context;
            _vectorizer = vectorizer;
        }

        public async Task<List<ProductDto>> RecommendForGuestAsync(string sessionId)
        {
            var viewedIds = await _context.ProductViews
                .Where(v => v.SessionId == sessionId)
                .Select(v => v.ProductId)
                .Distinct()
                .ToListAsync();

            var viewedProducts = await GetProductsByIdsAsync(viewedIds);
            if (!viewedProducts.Any()) return new List<ProductDto>();

            var userVector = CombineVectors(viewedProducts);
            var allProducts = await GetAllProductsAsync();

            return RecommendHybrid(userVector, allProducts, viewedIds, new Dictionary<int, double>()); // guest không có CF
        }

        public async Task<List<ProductDto>> RecommendForUserAsync(string userId)
        {
            var viewedIds = await _context.ProductViews
                .Where(v => v.UserId == userId)
                .Select(v => v.ProductId)
                .Distinct()
                .ToListAsync();

            var viewedProducts = await GetProductsByIdsAsync(viewedIds);
            if (!viewedProducts.Any()) return new List<ProductDto>();

            var userVector = CombineVectors(viewedProducts);
            var allProducts = await GetAllProductsAsync();
            var cfScores = ComputeCFScore(userId);

            return RecommendHybrid(userVector, allProducts, viewedIds, cfScores);
        }

        private async Task<List<Product>> GetProductsByIdsAsync(List<int> ids)
        {
            return await _context.Products
                .Where(p => ids.Contains(p.ProductId))
                .Include(p => p.Category)
                .Include(p => p.Style)
                .Include(p => p.Material)
                .ToListAsync();
        }

        private async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Style)
                .Include(p => p.Material)
                .ToListAsync();
        }

        private Dictionary<string, int> CombineVectors(List<Product> products)
        {
            var combined = new Dictionary<string, int>();
            foreach (var product in products)
            {
                var vector = _vectorizer.Vectorize(product);
                foreach (var kvp in vector)
                {
                    if (combined.ContainsKey(kvp.Key))
                        combined[kvp.Key] += kvp.Value;
                    else
                        combined[kvp.Key] = kvp.Value;
                }
            }
            return combined;
        }

        private List<ProductDto> RecommendHybrid(
            Dictionary<string, int> userVector,
            List<Product> allProducts,
            List<int> excludeIds,
            Dictionary<int, double> cfScores,
            double alpha = 0.6,
            double beta = 0.4)
        {
            return allProducts
                .Where(p => !excludeIds.Contains(p.ProductId))
                .Select(p =>
                {
                    var cbfScore = CosineSimilarity(userVector, _vectorizer.Vectorize(p));
                    var cfScore = cfScores.GetValueOrDefault(p.ProductId, 0);
                    var total = alpha * cbfScore + beta * cfScore;

                    return new
                    {
                        Product = p,
                        Score = total
                    };
                })
                .OrderByDescending(x => x.Score)
            .Take(10)
                .Select(x => new ProductDto
                {
                    ProductId = x.Product.ProductId,
                    NameProduct = x.Product.NameProduct,
                    Price = x.Product.Price,
                    ImageUrl = x.Product.ImageUrl,
                    Style = x.Product.Style?.Name,
                    Material = x.Product.Material?.Name
                })
                .ToList();
        }

        private double CosineSimilarity(Dictionary<string, int> a, Dictionary<string, int> b)
        {
            var keys = a.Keys.Union(b.Keys);
            double dot = 0, normA = 0, normB = 0;

            foreach (var key in keys)
            {
                var x = a.GetValueOrDefault(key, 0);
                var y = b.GetValueOrDefault(key, 0);
                dot += x * y;
                normA += x * x;
                normB += y * y;
            }

            return (normA == 0 || normB == 0) ? 0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
        }

        private Dictionary<string, HashSet<int>> BuildUserProductMatrix()
        {
            return _context.OrderDetails
                .Include(od => od.Order)
                .GroupBy(od => od.Order.IdUser)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ProductId).ToHashSet()
                );
        }

        private double JaccardSimilarity(HashSet<int> setA, HashSet<int> setB)
        {
            var intersection = setA.Intersect(setB).Count();
            var union = setA.Union(setB).Count();
            return union == 0 ? 0 : (double)intersection / union;
        }

        private Dictionary<int, double> ComputeCFScore(string userId)
        {
            var matrix = BuildUserProductMatrix();
            if (!matrix.ContainsKey(userId)) return new Dictionary<int, double>();

            var currentUserProducts = matrix[userId];
            var scoreDict = new Dictionary<int, double>();

            foreach (var kvp in matrix)
            {
                if (kvp.Key == userId) continue;

                var similarity = JaccardSimilarity(currentUserProducts, kvp.Value);
                if (similarity == 0) continue;

                foreach (var productId in kvp.Value.Except(currentUserProducts))
                {
                    if (!scoreDict.ContainsKey(productId))
                        scoreDict[productId] = 0;
                    scoreDict[productId] += similarity;
                }
            }

            return scoreDict;
        }
    }
}
