using DATN.Entities;

namespace DATN.Services.Interfaces
{
    public interface IProductVectorizer
    {
        Dictionary<string, int> Vectorize(Product product);
    }
}
