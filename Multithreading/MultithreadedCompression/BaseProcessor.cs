using System;
using System.Threading;

namespace MultithreadedCompression
{
    abstract class BaseProcessor
    {
        private Thread _statusThread;
        protected SharingExecutionStatus ExecutionStatus;

        private Thread[] _processThreadPool;
        protected SharingTaskQueue<Task> QueueToWrite;
        protected SharingPosition PositionToRead;

        protected string SourceFileName;
        protected string DestinationFileName;
        protected Metadata Metadata;

        internal BaseProcessor(string source, string destination)
        {
            SourceFileName = source;
            DestinationFileName = destination;
        }

        internal abstract int Run();

        protected void StartStatusThread(string command, double step)
        {
            ExecutionStatus = new SharingExecutionStatus(step);
            _statusThread = new Thread(StartShowExecutionStatus) { Name = "Informer" };
            _statusThread.Start(command + ": ");
        }

        protected void StartShowExecutionStatus(Object processName)
        {
            Console.Write((string)processName);
            Console.CursorVisible = false;
            var cursorPosition = Console.CursorLeft;
            double currentPercent = -1;
            while (true)
            {
                var percent = ExecutionStatus.GetStatus();
                if (percent < 0 || percent >= 100)
                {
                    currentPercent = percent;
                    break;
                }
                if (currentPercent < percent)
                {
                    currentPercent = percent;
                    Console.CursorLeft = cursorPosition;
                    Console.Write("{0:0} %", currentPercent);
                }
            }

            if (currentPercent >= 100)
            {
                Console.CursorLeft = cursorPosition;
                Console.WriteLine("100 %");
            }
            else
            {
                Console.WriteLine();
            }
            Console.CursorVisible = true;
        }

        protected void JoinStatusThread()
        {
            _statusThread?.Join();
        }

        protected void StartProcessThreads(int count, string name, ThreadStart threadStart)
        {
            _processThreadPool = new Thread[count];
            for (int i = 0; i < count; i++)
            {
                _processThreadPool[i] = new Thread(threadStart) { Name = name + i };
                _processThreadPool[i].Start();
            }
        }

        protected void JoinProcessThreads()
        {
            if (_processThreadPool == null || _processThreadPool.Length == 0)
                return;
            for (int i = 0; i < _processThreadPool.Length; i++)
                _processThreadPool[i]?.Join();
        }

        protected void AbortAllThreads()
        {
            ExecutionStatus?.FinishStatus(false);
            QueueToWrite?.FinishEnqueueAll();
            PositionToRead?.FinishReadAll();
            JoinStatusThread();
            JoinProcessThreads();
        }
    }
}
