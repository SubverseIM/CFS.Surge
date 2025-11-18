namespace CFS.Surge.Core
{
    public static class Primes
    {
        public static int GreatestCommonDivisor(int a, int b) => b == 0 ? a : GreatestCommonDivisor(b, a % b);

        public static IEnumerable<int> Factors(this int number)
        {
            for (int div = 2; div <= Math.Sqrt(number); div++)
            {
                while (number % div == 0)
                {
                    yield return div;
                    number = number / div;
                }
            }

            if (number > 1) yield return number;
        }

        public static bool IsPrime(this int n)
        {
            return n.Factors().Contains(n);
        }
    }
}
