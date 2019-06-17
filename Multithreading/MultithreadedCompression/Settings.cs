using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultithreadedCompression
{
    internal static class Settings
    {
        /// <summary>
        /// Compressed file extension
        /// </summary>
       internal const string CompressedExtension = ".gz";

        internal static int ThreadCount;
        internal static int ChunkSizeBytes;



        internal static void SetSettings(int threadCount, int chunkSize)
        {
            ThreadCount = threadCount;
            ChunkSizeBytes = chunkSize;
        }
    }
}
