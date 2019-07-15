namespace MultithreadedCompression
{
    internal class SharingPosition
    {
        private long _lastPosition;
        private readonly object _loker = new object();
        private readonly long _maxPosition;
        private readonly int _step;
        internal SharingPosition(long startPosition, long maxPosition, int step)
        {
            _lastPosition = startPosition;
            _maxPosition = maxPosition;
            _step = step;
        }

        internal long GetLastPositionAndIncrement()
        {
            lock (_loker)
            {
                if (_lastPosition >= _maxPosition)
                    return -1;
                var currentPosition = _lastPosition;
                _lastPosition += _step;
                return currentPosition;
            }
        }
        internal void FinishReadAll()
        {
            lock (_loker)
            {
                _lastPosition = _maxPosition;
            }
        }
    }
}
