using System.Linq.Expressions;
using DATN.Entities;
using Microsoft.EntityFrameworkCore;

public static class IQueryableExtensions
{
    public class QueryOptions<T>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; } 
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
    }
    public class Meta
    {
        public int PageSize { get; set; }
        public int Page { get; set; }
        public int Total { get; set; }
    }
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public Meta Meta { get; set; }
    }

    
    public static IQueryable<T> ApplySearch<T>(this IQueryable<T> source, string? search, params Expression<Func<T, string>>[] stringSelectors)
    {
        if (string.IsNullOrWhiteSpace(search) || stringSelectors.Length == 0)
            return source;

        var lowered = search.Trim().ToLowerInvariant();
        Expression? combined = null;
        var parameter = Expression.Parameter(typeof(T), "x");

        foreach (var selector in stringSelectors)
        {
            var body = Expression.Invoke(selector, parameter); 
            var toLower = Expression.Call(body, typeof(string).GetMethod("ToLower", Type.EmptyTypes)!);
            var contains = Expression.Call(toLower, typeof(string).GetMethod("Contains", new[] { typeof(string) })!, Expression.Constant(lowered));
            combined = combined == null ? contains : Expression.OrElse(combined, contains);
        }

        if (combined == null)
            return source;

        var lambda = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return source.Where(lambda);
    }


    public static IQueryable<Product> ApplySort(this IQueryable<Product> source, string? sortBy, bool desc)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return source;

        if (sortBy == "BestSeller")
        {
            source = desc
                ? source.OrderByDescending(p => p.OrderDetails.Sum(od => (int?)od.Quantity) ?? 0)
                : source.OrderBy(p => p.OrderDetails.Sum(od => (int?)od.Quantity) ?? 0);
            return source;
        }
        var propertyInfo = typeof(Product).GetProperty(sortBy);
        if (propertyInfo == null)
            return source;
        var param = Expression.Parameter(typeof(Product), "x");
        var property = Expression.Property(param, propertyInfo);
        var converted = Expression.Convert(property, typeof(object)); // tránh lỗi với nullable
        var lambda = Expression.Lambda<Func<Product, object>>(converted, param);

        return desc
            ? source.OrderByDescending(lambda)
            : source.OrderBy(lambda);
    }



    public static async Task<PagedResult<TResult>> ToPagedResultAsync<TSource, TResult>(
        this IQueryable<TSource> source,
        int page,
        int pageSize,
        Func<List<TSource>, List<TResult>> projector)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        const int maxPageSize = 100;
        if (pageSize > maxPageSize) pageSize = maxPageSize;

        var total = await source.CountAsync();
        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var projected = projector(items);

        return new PagedResult<TResult>
        {
            Items = projected,
            Meta = new Meta
            {
                Total = total,
                Page = page,
                PageSize = pageSize
            }
          

        };
    }
}
