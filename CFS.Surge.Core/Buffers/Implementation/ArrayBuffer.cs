namespace CFS.Surge.Core.Buffers.Implementation
{
    public readonly struct ArrayBuffer<T> : IBuffer<T>
    {
        private readonly T[] array;

        public ArrayBuffer(T[] array) 
        { 
            this.array = array;
        }

        T IReadOnlyBuffer<T>.this[long index] => this[index];

        public ref T this[long index] => ref array[index];

        public long Length => array.LongLength;
    }
}
