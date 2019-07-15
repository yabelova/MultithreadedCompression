using System.Collections.Generic;
using System.Threading;

namespace MultithreadedCompression
{
    internal sealed class SharingTaskQueue<T> where T : class
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly object _loker = new object();
        private readonly int _maxLength;
        private int _writers;

        internal SharingTaskQueue(int writers, int length = -1)
        {
            _writers = writers;
            _maxLength = length;
        }

        internal void Enqueue(T task)
        {
            if (task == null)
                return;
            lock (_loker)
            {
                while (_queue.Count == _maxLength && _writers > 0)
                    Monitor.Wait(_loker);

                if (_writers <= 0)
                    return;

                _queue.Enqueue(task);
                Monitor.Pulse(_loker);
            }
        }

        internal T Dequeue()
        {
            lock (_loker)
            {
                while (_queue.Count == 0 && _writers > 0)
                    Monitor.Wait(_loker);

                if (_queue.Count == 0)
                    return null;

                var t = _queue.Dequeue();
                Monitor.Pulse(_loker);
                return t;
            }
        }

        internal void FinishEnqueue()
        {
            lock (_loker)
            {
                _writers--;
                Monitor.PulseAll(_loker);
            }
        }

        internal void FinishEnqueueAll()
        {
            lock (_loker)
            {
                _writers = 0;
                Monitor.PulseAll(_loker);
            }
        }
    }

    internal sealed class Task
    {
        internal long UncompressedOffset { get; set; }
        internal byte[] Buffer { get; set; }
        internal string RethrowException { get; set; }

        internal Task(long offset, byte[] buffer)
        {
            UncompressedOffset = offset;
            Buffer = buffer;
            RethrowException = null;
        }
        internal Task(string exception)
        {
            UncompressedOffset = -1;
            Buffer = null;
            RethrowException = exception;
        }
    }
}
