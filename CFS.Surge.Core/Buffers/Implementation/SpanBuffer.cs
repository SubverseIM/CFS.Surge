namespace CFS.Surge.Core.Buffers.Implementation
{
    public readonly struct SpanBuffer<T> : IBuffer<T>
    {
        private readonly IBuffer<T> buffer;

        private readonly long offset, length;

        public SpanBuffer(IBuffer<T> buffer, long offset, long length)
        {
            this.buffer = buffer;

            this.offset = offset;
            if (offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            this.length = length;
            if(length < 0 || offset + length > buffer.Length) 
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
        }

        T IReadOnlyBuffer<T>.this[long index] => this[index];

        public ref T this[long index] => ref buffer[index + offset];

        public long Length => length;
    }
}
