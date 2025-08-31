using AutoMapper;
using DATN.DbContexts;
using DATN.Dtos.ProductDto;
using DATN.Entities;
using DATN.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DATN.Services.Implements
{
    public class RecommendationService : IRecommendationService
    {
        private readonly Data _context;
        private readonly IProductVectorizer _vectorizer;
        private readonly IMapper _mapper;
        private readonly ConcurrentDictionary<int, double[]> _cachedVectors
            = new ConcurrentDictionary<int, double[]>();

        public RecommendationService(Data context, IProductVectorizer vectorizer, IMapper mapper)
        {
            _context = context;
            _vectorizer = vectorizer;
            _mapper = mapper;
            var products = _context.Products
           .AsNoTracking()
           .Include(p => p.Category)
           .Include(p => p.Style)
           .Include(p => p.Material)
           .ToList();

            _vectorizer = new ProductVectorizer(products);
            Task.Run(() => LoadAllProductVectorsAsync(products)).Wait();
            
        }

        private async Task LoadAllProductVectorsAsync(List<Product> products)
        {
            foreach (var product in products)
            {
                var vector = _vectorizer.Vectorize(product).Array;
                _cachedVectors[product.ProductId] = Normalize(vector);
            }
        }

        public IEnumerable<object> GetAllProductVectorInfo()
        {
            var metaLists = _vectorizer.GetMetaLists();
            int metaLength = metaLists.categories.Count
                             + metaLists.styles.Count
                             + metaLists.materials.Count
                             + metaLists.priceBuckets.Count;

            foreach (var kvp in _cachedVectors)
            {
                var productId = kvp.Key;
                var vector = kvp.Value;

                double metaNorm = Math.Sqrt(vector.Take(metaLength).Sum(x => x * x));
                double tfidfNorm = Math.Sqrt(vector.Skip(metaLength).Sum(x => x * x));
                double totalNorm = metaNorm + tfidfNorm;

                double metaPercent = totalNorm > 0 ? metaNorm / totalNorm * 100 : 0;
                double tfidfPercent = totalNorm > 0 ? tfidfNorm / totalNorm * 100 : 0;

                yield return new
                {
                    ProductId = productId,
                    Vector = vector,
                    MetaPercent = metaPercent,
                    TfidfPercent = tfidfPercent
                };
            }
        }
        public double[]? GetProductVector(int productId)
        {
            if (_cachedVectors.TryGetValue(productId, out var vector))
            {
                return vector;
            }
            return null; 
        }
        private double[] Normalize(double[] vector)
        {
            double norm = Math.Sqrt(vector.Sum(v => v * v));
            if (norm == 0) return vector.ToArray();
            return vector.Select(v => v / norm).ToArray();
        }
        public void UpdateProductVector(Product product)
        {
            var vector = _vectorizer.Vectorize(product).Array;
            _cachedVectors[product.ProductId] = Normalize(vector);
        }

        public void RemoveProductVector(int productId)
        {
            _cachedVectors.TryRemove(productId, out _);
        }

        private double[] BuildUserVector(List<int> viewedIds)
        {
            if (!viewedIds.Any()) return Array.Empty<double>();
            var sum = new double[_cachedVectors.Values.First().Length];

            foreach (var id in viewedIds)
            {
                if (_cachedVectors.TryGetValue(id, out var vec))
                {
                    for (int i = 0; i < sum.Length; i++)
                        sum[i] += vec[i];
                }
            }

            return Normalize(sum);
        }

        private double CosineSimilarity(double[] a, double[] b)
        {
            if (a.Length != b.Length) return 0;
            double dot = 0;
            for (int i = 0; i < a.Length; i++)
                dot += a[i] * b[i];
            return dot;
        }

        public async Task<List<ProductDto>> RecommendForGuestAsync(string sessionId)
        {
            var viewedIds = await _context.ProductViews
                .AsNoTracking()
                .Where(v => v.SessionId == sessionId)
                .Select(v => v.ProductId)
                .Distinct()
                .ToListAsync();

            if (!viewedIds.Any()) return new List<ProductDto>();

            var userVector = BuildUserVector(viewedIds);

            var allProducts = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Style)
                .Include(p => p.Material)
                .Include(p => p.ProductImages)
                .Where(p => !p.deleteflag)
                .ToListAsync();

            return RecommendHybrid(userVector, allProducts, viewedIds, new Dictionary<int, double>());
        }

        public async Task<List<ProductDto>> RecommendForUserAsync(string userId)
        {
            var viewedIds = await _context.ProductViews
                .AsNoTracking()
                .Where(v => v.UserId == userId)
                .Select(v => v.ProductId)
                .Distinct()
                .ToListAsync();
            var vieworder=await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == "Complete" && o.IdUser == userId)
                .SelectMany(o => o.OrderDetails)
                .Select(od => od.ProductId)
                .Distinct()
                .ToListAsync();
            if (!viewedIds.Any()) return new List<ProductDto>();
            if (!viewedIds.Any() && !vieworder.Any())
                return new List<ProductDto>();
            var excludeIds = viewedIds.Concat(vieworder).Distinct().ToList();
            var userVector = BuildUserVector(viewedIds);

            var allProducts = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Style)
                .Include(p => p.Material)
                .Include(p => p.ProductImages)
                .Where(p => !p.deleteflag)
                .ToListAsync();

            var cfScores = ComputeCFScore(userId);

            return RecommendHybrid(userVector, allProducts, excludeIds, cfScores);
        }

        public async Task<List<ProductDto>> RecommendRelatedAsync(int productId)
        {
            if (!_cachedVectors.TryGetValue(productId, out var targetVector))
                return new List<ProductDto>();

            var allProducts = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Style)
                .Include(p => p.Material)
                .Include(p => p.ProductImages)
                .Where(p => !p.deleteflag && p.ProductId != productId)
                .ToListAsync();

            return allProducts
                .Select(p =>
                {
                    var cbfVector = _cachedVectors.GetValueOrDefault(p.ProductId);
                    var score = cbfVector != null ? CosineSimilarity(targetVector, cbfVector) : 0;
                   
                    return new { Product = p, Score = score };
                })
                .OrderByDescending(x => x.Score)
                .Take(8)
                .Select(x => _mapper.Map<ProductDto>(x.Product))
                .ToList();
        }

        private List<ProductDto> RecommendHybrid(
            double[] userVector,
            List<Product> allProducts,
            List<int> excludeIds,
            Dictionary<int, double> cfScores,
            double alpha = 0.7,
            double beta = 0.3)
        {
            var excludeSet = new HashSet<int>(excludeIds);

            return allProducts
                .Where(p => !excludeSet.Contains(p.ProductId))
                .Select(p =>
                {
                    var cbfVector = _cachedVectors.GetValueOrDefault(p.ProductId);
                    var cbfScore = cbfVector != null ? CosineSimilarity(userVector, cbfVector) : 0;
                    var cfScore = cfScores.GetValueOrDefault(p.ProductId, 0);
                    var total = alpha * cbfScore + beta * cfScore;

                    return new { Product = p, Score = total };
                })
                .OrderByDescending(x => x.Score)
                .Take(8)
                .Select(x => _mapper.Map<ProductDto>(x.Product))
                .ToList();
        }

        private Dictionary<string, HashSet<int>> BuildUserProductMatrix()
        {
            return _context.OrderDetails
                .Include(od => od.Order)
                .AsNoTracking()
                .GroupBy(od => od.Order.IdUser)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ProductId).ToHashSet());
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
