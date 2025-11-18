namespace CFS.Surge.Core.Buffers
{
    public interface IReadOnlyBuffer<out T>
    {
        T this[long index] { get; }

        long Length { get; }
    }
}
