using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;

namespace MultithreadedCompression
{
    public class MyThreadPool
    {
        private Thread[] _Pool;
        public object LockerPosition;
        public object LockerBufferToWrite;
        public object LockerBufferToRead;

        private long LastPosition;
        public Dictionary<long, byte[]> BufferToWrite;

        public void StartCompressThreadPool(long sourceLength, string fileSource)
        {
            LastPosition = 0;
            BufferToWrite = new Dictionary<long, byte[]>();
            LockerPosition = new object();
            LockerBufferToWrite = new object();

            var count = (int)Math.Min(Settings.ThreadCount, Math.Ceiling(sourceLength / (decimal) Settings.ChunkSizeBytes));

            _Pool = new Thread[count];
            for (int i = 0; i < count; i++)
            {
                _Pool[i] = new Thread(ThreadStartCompress) { Name = i.ToString() };
                _Pool[i].Start(new ThreadTask(CurrentWork.Processing, fileSource));
            }
        }

        public void Join()
        {
            while (_Pool.Any(x => x.IsAlive))
                ;
        }

        public bool IsAlive()
        {
            return (_Pool.Any(x => x.IsAlive));
        }

        private void ThreadStartCompress(Object oTask)
        {
            //Console.WriteLine(Thread.CurrentThread.Name + string.Intern(" start"));

            var task = oTask as ThreadTask;
            if (task == null)
                return;

            var fs = new FileStream(task.FileSource, FileMode.Open, FileAccess.Read);

            var maxPosition = fs.Length;
            byte[] buffer = new byte[Settings.ChunkSizeBytes];
            while (true)
            {
                long currentPosition;
                lock (LockerPosition)
                {
                    currentPosition = LastPosition;
                    LastPosition += Settings.ChunkSizeBytes;
                }

                if (currentPosition >= maxPosition) break;
                //Console.WriteLine(new StringBuilder(Thread.CurrentThread.Name).Append( " processing ").Append(currentPosition).ToString());

                var chunkSize = (int) Math.Min(Settings.ChunkSizeBytes, maxPosition - currentPosition);

                fs.Seek(currentPosition, SeekOrigin.Begin);
                fs.Read(buffer, 0, chunkSize);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress))
                        gs.Write(buffer, 0, chunkSize);

                    lock (LockerBufferToWrite)
                    {
                        BufferToWrite.Add(currentPosition, ms.ToArray());
                    }
                }
            }

            fs.Close();
            //Console.WriteLine(Thread.CurrentThread.Name + string.Intern(" end"));
        }

        enum CurrentWork
        {
            Processing,
            Read,
            Write
        }
        class ThreadTask
        {
            internal CurrentWork Work { get; set; }
            internal string FileSource { get; private set; }

            public ThreadTask(CurrentWork work, string fileSource)
            {
                Work = work;
                FileSource = fileSource;
            }
        }

    }
}