namespace DATN.Helpers
{
    using DATN.Entities;
    using Microsoft.EntityFrameworkCore;

    

        public static class ProductQueryExtensions
        {
            public enum SearchMode { All,Any,Exact }
            public static IQueryable<Product> ApplyKeywordSearch(this IQueryable<Product> source, string? search, SearchMode mode = SearchMode.All)

            {
                if (string.IsNullOrWhiteSpace(search))
                    return source;

            search = search.Trim().ToLowerInvariant();

            if (mode == SearchMode.Exact)
            {
                return source.Where(p =>
                    (p.NameProduct != null && EF.Functions.Like(p.NameProduct.ToLower(), $"%" +search+"%")) ||
                    (p.Description != null && EF.Functions.Like(p.Description.ToLower(), $"%" + search + "%")) ||
                    (p.Category != null && p.Category.NameCategory != null && EF.Functions.Like(p.Category.NameCategory.ToLower(), $"%" + search + "%")) ||
                    (p.Style != null && p.Style.Name != null && EF.Functions.Like(p.Style.Name.ToLower(), $"%" + search + "%")) ||
                    (p.Material != null && p.Material.Name != null && EF.Functions.Like(p.Material.Name.ToLower(), $"%" + search + "%"))
                );
            }
            var keywords = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (mode == SearchMode.All) 
            {
                return source.Where(p =>
                    keywords.All(k =>
                        (p.NameProduct != null && EF.Functions.Like(p.NameProduct.ToLower(), $"%" + k + "%")) ||
                        (p.Description != null && EF.Functions.Like(p.Description.ToLower(), $"%"+k+"%")) ||
                        (p.Category != null && p.Category.NameCategory != null && EF.Functions.Like(p.Category.NameCategory.ToLower(), $"%"+k+"%")) ||
                        (p.Style != null && p.Style.Name != null && EF.Functions.Like(p.Style.Name.ToLower(), $"%"+k+"%")) ||
                        (p.Material != null && p.Material.Name != null && EF.Functions.Like(p.Material.Name.ToLower(), $"%"+k+"%"))
                    )
                );
            }
            return source.Where(p =>
                keywords.Any(k =>
                    (p.NameProduct != null && EF.Functions.Like(p.NameProduct.ToLower(), $"%" + k + "%")) ||
                    (p.Description != null && EF.Functions.Like(p.Description.ToLower(), $"%" + k + "%")) ||
                    (p.Category != null && p.Category.NameCategory != null && EF.Functions.Like(p.Category.NameCategory.ToLower(), $"%" + k + "%")) ||
                    (p.Style != null && p.Style.Name != null && EF.Functions.Like(p.Style.Name.ToLower(), $"%" + k + "%")) ||
                    (p.Material != null && p.Material.Name != null && EF.Functions.Like(p.Material.Name.ToLower(), $"%" + k + "%"))
                )
            );
        }
        }

}
