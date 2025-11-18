namespace CFS.Surge.Core.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> Common<T>(
            this IEnumerable<T> source,
            IEnumerable<T> sequence,
            IEqualityComparer<T>? comparer = null)
        {
            if (sequence == null)
            {
                return Enumerable.Empty<T>();
            }

            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            return source.GroupBy(t => t, comparer)
                .Join(
                    sequence.GroupBy(t => t, comparer),
                    g => g.Key,
                    g => g.Key,
                    (lg, rg) => lg.Zip(rg, (l, r) => l),
                    comparer)
                .SelectMany(g => g);
        }

        public static IEnumerable<int> Subtract(this IEnumerable<int> a, IEnumerable<int> b)
        {
            var aLookup = a.ToLookup(x => x);
            var bLookup = b.ToLookup(x => x);
            var filtered = aLookup
                .SelectMany(aItem => aItem.Take(aItem.Count() - bLookup[aItem.Key].Count()));
            return filtered;
        }
    }
}
