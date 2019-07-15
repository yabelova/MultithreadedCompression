namespace MultithreadedCompression
{
    internal class SharingExecutionStatus
    {
        private double _currentPercent;
        private readonly object _loker = new object();
        private readonly double _step;
        internal SharingExecutionStatus(double step)
        {
            _currentPercent = 0;
            _step = step;
        }

        internal double GetStatus()
        {
            lock (_loker)
            {
                return _currentPercent;
            }
        }

        internal void IncrementStatus()
        {
            lock (_loker)
            {
                _currentPercent += _step;
            }
        }

        internal void FinishStatus(bool success)
        {
            lock (_loker)
            {
                _currentPercent = success ? 100 : -1;
            }
        }
    }
}