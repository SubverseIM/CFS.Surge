namespace CFS.Surge.Core
{
    public class References
    {
        public static void Dereference<T>(ref T value) where T : class
        {
            value = null!;
        }
    }
}
