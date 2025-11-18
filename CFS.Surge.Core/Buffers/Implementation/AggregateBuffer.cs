namespace CFS.Surge.Core.Buffers.Implementation
{
    public readonly struct AggregateBuffer<T> : IBuffer<T>
    {
        private readonly SortedList<long, IBuffer<T>> bufferList;

        private readonly long totalLength;

        public AggregateBuffer(params IEnumerable<IBuffer<T>> buffers)
        {
            bufferList = new();
            foreach (IBuffer<T> buffer in buffers)
            {
                bufferList.Add(totalLength += buffer.Length, buffer);
            }
        }

        public AggregateBuffer<T> Append(IBuffer<T> buffer)
        {
            return new AggregateBuffer<T>([.. bufferList.Values, buffer]);
        }

        T IReadOnlyBuffer<T>.this[long index] => this[index];

        public ref T this[long index]
        {
            get
            {
                if (index < 0 || index >= totalLength)
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    int i; long offset = 0;
                    for (i = 0; i < bufferList.Count; i++) 
                    {
                        if (bufferList.GetKeyAtIndex(i) > index) break;
                        offset = bufferList.GetKeyAtIndex(i);
                    }

                    return ref bufferList.GetValueAtIndex(i)[index - offset];
                }
            }
        }

        public long Length => totalLength;
    }
}
