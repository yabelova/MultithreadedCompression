using System;

namespace MultithreadedCompression
{
    internal static class Settings
    {
        internal static readonly string CompressedFileExtension = ".gz";
        internal static int MaxThreadCount = Environment.ProcessorCount;
        internal static int ChunkSizeBytes = 1024 * 1024 * 10;
        internal static int AvailableMemoryBytes = 1024 * 1024 * 1024;
    }
}
