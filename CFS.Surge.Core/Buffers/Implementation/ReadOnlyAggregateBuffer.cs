namespace CFS.Surge.Core.Buffers.Implementation
{
    public readonly struct ReadOnlyAggregateBuffer<T> : IReadOnlyBuffer<T>
    {
        private readonly SortedList<long, IReadOnlyBuffer<T>> bufferList;

        private readonly long totalLength;

        public ReadOnlyAggregateBuffer(params IEnumerable<IReadOnlyBuffer<T>> buffers)
        {
            bufferList = new();
            foreach (IReadOnlyBuffer<T> buffer in buffers)
            {
                bufferList.Add(totalLength += buffer.Length, buffer);
            }
        }

        public ReadOnlyAggregateBuffer<T> Append(IReadOnlyBuffer<T> buffer)
        {
            return new ReadOnlyAggregateBuffer<T>([.. bufferList.Values, buffer]);
        }

        public T this[long index]
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

                    return bufferList.GetValueAtIndex(i)[index - offset];
                }
            }
        }

        public long Length => totalLength;
    }
}
