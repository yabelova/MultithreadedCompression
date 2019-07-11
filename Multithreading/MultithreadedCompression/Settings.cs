namespace MultithreadedCompression
{
    internal static class Settings
    {
        /// <summary>
        /// Compressed file extension
        /// </summary>
        internal static readonly string CompressedExtension = ".gz";

        internal static int ThreadCount;
        internal static int ChunkSizeBytes;

        internal static void SetSettings(int threadCount, int chunkSize)
        {
            ThreadCount = threadCount;
            ChunkSizeBytes = chunkSize;
        }
    }
}
