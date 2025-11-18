namespace CFS.Surge.Core.Buffers
{
    public interface IBuffer<T> : IReadOnlyBuffer<T>
    {
        new ref T this[long index] { get; }
    }
}
