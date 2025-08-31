//using DATN.Entities;
//using DATN.Services.Interfaces;

//namespace DATN.Services.Implements
//{
//    public class AlphaBetaTuner
//    {
//        private readonly List<Product> _products;
//        private readonly Dictionary<int, List<int>> _groundTruth;
//        // key = productId, value = danh sách productId thực sự liên quan (ground truth)

//        public AlphaBetaTuner(List<Product> products, Dictionary<int, List<int>> groundTruth)
//        {
//            _products = products;
//            _groundTruth = groundTruth;
//        }

//        public (double bestAlpha, double bestBeta, double bestScore) GridSearch()
//        {
//            double bestAlpha = 0, bestBeta = 0, bestScore = double.MinValue;

//            // Ví dụ thử alpha, beta trong khoảng [0.1 -> 1.0] với bước 0.1
//            for (double alpha = 0.1; alpha <= 1.0; alpha += 0.1)
//            {
//                for (double beta = 0.1; beta <= 1.0; beta += 0.1)
//                {
//                    double score = Evaluate(alpha, beta);

//                    if (score > bestScore)
//                    {
//                        bestScore = score;
//                        bestAlpha = alpha;
//                        bestBeta = beta;
//                    }
//                }
//            }

//            return (bestAlpha, bestBeta, bestScore);
//        }

//        private double Evaluate(double alpha, double beta)
//        {
//            // tạo vectorizer mới với alpha, beta hiện tại
//            var vectorizer = new ProductVectorizer(_products, alpha, beta);

//            double totalPrecision = 0;
//            int count = 0;

//            foreach (var kv in _groundTruth)
//            {
//                int productId = kv.Key;
//                var expected = kv.Value;

//                var queryProduct = _products.First(p => p.ProductId == productId);

//                // vectorize query với α, β
//                var qVec = vectorizer.Vectorize(queryProduct).Dict;

//                // cosine similarity với tất cả sp khác
//                var sims = new List<(int, double)>();
//                foreach (var other in _products.Where(p => p.ProductId != productId))
//                {
//                    var oVec = vectorizer.Vectorize(other).Dict;
//                    double sim = CosineSimilarity(qVec, oVec);
//                    sims.Add((other.ProductId, sim));
//                }

//                var topK = sims.OrderByDescending(x => x.Item2).Take(5).Select(x => x.Item1).ToList();

//                // precision@5 = |giao giữa topK và ground truth| / |topK|
//                double precision = topK.Intersect(expected).Count() / (double)topK.Count;
//                totalPrecision += precision;
//                count++;
//            }

//            return totalPrecision / count; // precision trung bình
//        }

//        private double CosineSimilarity(Dictionary<string, double> v1, Dictionary<string, double> v2)
//        {
//            double dot = 0, norm1 = 0, norm2 = 0;

//            foreach (var kv in v1)
//            {
//                double val1 = kv.Value;
//                if (v2.TryGetValue(kv.Key, out double val2))
//                    dot += val1 * val2;
//                norm1 += val1 * val1;
//            }
//            foreach (var val2 in v2.Values)
//                norm2 += val2 * val2;

//            if (norm1 == 0 || norm2 == 0) return 0;
//            return dot / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
//        }
//    }
//}
