using DATN.Entities;
using DATN.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DATN.Services.Implements
{
    public class ProductVectorizer : IProductVectorizer
    {
        private readonly List<string> _categories;
        private readonly List<string> _styles;
        private readonly List<string> _materials;
        private readonly List<string> _priceBuckets;
        private readonly List<string> _vocab;
        private readonly Dictionary<string, double> _idf;

       
        private readonly double _alpha; 
        private readonly double _beta;  
        private readonly double _alphacategory;
        private readonly HashSet<string> _stopwords = new HashSet<string>
        {
            "phong", "cách", "hiện", "đại", "màu", "tự", "nhiên",
            "sản", "phẩm", "nội", "thất" ,"dq","dqs","cổ","điển","VLINE",
            "bàn", "ghế", "giường", "tủ", "kệ", "sofa", "trang", "điểm", "ăn", "ngủ", "bếp"
        };
        public ProductVectorizer(IEnumerable<Product> products, double alpha = 0.2, double beta = 1,double cate=0.8)
        {
            _alpha = alpha;
            _beta = beta;
            _alphacategory = cate;

            _categories = products.Select(p => p.Category?.NameCategory)
                                  .Where(x => !string.IsNullOrEmpty(x))
                                  .Distinct().ToList();

            _styles = products.Select(p => p.Style?.Name)
                              .Where(x => !string.IsNullOrEmpty(x))
                              .Distinct().ToList();

            _materials = products.Select(p => p.Material?.Name)
                                 .Where(x => !string.IsNullOrEmpty(x))
                                 .Distinct().ToList();



            _priceBuckets = new List<string> { "Low", "Mid", "High" };

           
            var docs = products.Select(p => Tokenize(p.NameProduct)).ToList();
            _vocab = docs.SelectMany(x => x).Distinct().ToList();

            _idf = new Dictionary<string, double>();
            int N = products.Count();
            foreach (var term in _vocab)
            {
                int df = docs.Count(d => d.Contains(term));
                _idf[term] = Math.Log((double)N / (1 + df));
            }
        }

        public VectorResult Vectorize(Product product)
        {
            var dict = new Dictionary<string, double>();

           
            foreach (var cat in _categories)
                dict["cat_" + cat] = (product.Category?.NameCategory == cat ? 1.0 : 0.0) * _alphacategory;

            foreach (var style in _styles)
                dict["style_" + style] = (product.Style?.Name == style ? 1.0 : 0.0) * _alpha;

            foreach (var mat in _materials)
                dict["mat_" + mat] = (product.Material?.Name == mat ? 1.0 : 0.0) * _alpha;

            var bucket = GetPriceBucket(product.Price);
            foreach (var p in _priceBuckets)
                dict["price_" + p] = (p == bucket ? 1.0 : 0.0) * _alpha;

           
            var tokens = Tokenize(product.NameProduct);
            var tf = new Dictionary<string, double>();
            foreach (var term in tokens)
                tf[term] = tf.ContainsKey(term) ? tf[term] + 1 : 1;

            var tfidfValues = new List<double>();
            foreach (var term in _vocab)
            {
                double tfValue = tf.ContainsKey(term) ? tf[term] / tokens.Count : 0.0;
                double tfidf = tfValue * _idf[term];
                tfidfValues.Add(tfidf);
            }

            
            double norm = Math.Sqrt(tfidfValues.Sum(v => v * v));
            if (norm > 0)
                for (int i = 0; i < tfidfValues.Count; i++)
                    tfidfValues[i] = (tfidfValues[i] / norm) * _beta;

            
            for (int i = 0; i < _vocab.Count; i++)
                dict["name_" + _vocab[i]] = tfidfValues[i];

            
            var arrayList = new List<double>();
            foreach (var cat in _categories) arrayList.Add(dict["cat_" + cat]);
            foreach (var style in _styles) arrayList.Add(dict["style_" + style]);
            foreach (var mat in _materials) arrayList.Add(dict["mat_" + mat]);
            foreach (var p in _priceBuckets) arrayList.Add(dict["price_" + p]);
            foreach (var term in _vocab) arrayList.Add(dict["name_" + term]);

            return new VectorResult
            {
                Dict = dict,
                Array = arrayList.ToArray()
            };
        }

        private string GetPriceBucket(decimal price)
        {
            if (price < 4000000) return "Low";
            if (price < 10000000) return "Mid";
            return "High";
        }

        private List<string> Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            return text.ToLower()
                       .Split(new[] { ' ', ',', '.', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                       .Where(token => !_stopwords.Contains(token))
                       .ToList();
        }
        public (List<string> categories, List<string> styles, List<string> materials, List<string> priceBuckets) GetMetaLists()
        {
            return (_categories, _styles, _materials, _priceBuckets);
        }

    }

    public class VectorResult
    {
        public Dictionary<string, double> Dict { get; set; }
        public double[] Array { get; set; }
    }

}
