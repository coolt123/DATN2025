using DATN.Entities;
using DATN.Services.Interfaces;

namespace DATN.Services.Implements
{
    public class ProductVectorizer : IProductVectorizer
    {
        public Dictionary<string, int> Vectorize(Product product)
        {
            return new Dictionary<string, int>
            {
                [$"Category:{product.Category?.NameCategory}"] = 1,
                [$"Style:{product.Style?.Name}"] = 1,
                [$"Material:{product.Material?.Name}"] = 1,
                [$"Price:{GetPriceBucket(product.Price)}"] = 1
            };
        }

        private string GetPriceBucket(decimal price)
        {
            if (price < 1000000) return "Low";
            if (price < 5000000) return "Mid";
            return "High";
        }
    }
}
