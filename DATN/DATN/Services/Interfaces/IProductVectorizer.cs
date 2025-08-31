using DATN.Entities;
using DATN.Services.Implements;

namespace DATN.Services.Interfaces
{
    public interface IProductVectorizer
    {
        VectorResult Vectorize(Product product);
        (List<string> categories, List<string> styles, List<string> materials, List<string> priceBuckets) GetMetaLists();
    }
}
