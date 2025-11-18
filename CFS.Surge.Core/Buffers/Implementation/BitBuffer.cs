using System.Runtime.CompilerServices;

namespace CFS.Surge.Core.Buffers.Implementation
{
    public readonly struct BitBuffer<T> : IBuffer<T>
        where T : unmanaged
    {
        private readonly byte[] array;

        public BitBuffer(byte[] array)
        {
            this.array = array;
        }

        T IReadOnlyBuffer<T>.this[long index] => this[index];

        public ref T this[long index]
        {
            get
            {
                return ref Unsafe.As<byte, T>(ref array[index * Unsafe.SizeOf<T>()]);
            }
        }

        public long Length => array.LongLength / Unsafe.SizeOf<T>();
    }
}
