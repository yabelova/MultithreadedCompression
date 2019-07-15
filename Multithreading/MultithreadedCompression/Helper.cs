using System;

namespace MultithreadedCompression
{
    internal static class Helper
    {
        internal static string GetCompressedFileNameWithExtension(string destinationName)
        {
            return destinationName.EndsWith(Settings.CompressedFileExtension)
                ? destinationName
                : destinationName + Settings.CompressedFileExtension;
        }

        internal static string GetDecompressedFileNameWithExtension(string destinationName, string extension)
        {
            return destinationName.EndsWith(extension)
                ? destinationName
                : destinationName + extension;
        }

        internal static string GetFileExtension(string sourceName)
        {
            return sourceName.Remove(0, sourceName.LastIndexOf('.'));
        }

        internal static int GetCompressThreadsCount(long sourceLength)
        {
            //If given file is small, we don't need max threads count to compress
            var result = (int)Math.Min(Settings.MaxThreadCount,
                                        Math.Ceiling(sourceLength / (decimal)Settings.ChunkSizeBytes));
            return result;
        }

        internal static int GetDecompressThreadsCount(long chunkCount)
        {
            //If given file gets small number of chunks, we don't need max threads count to decompress
            var result = (int)Math.Min(Settings.MaxThreadCount, chunkCount);
            return result;
        }

        internal static int GetMaxTaskQueueLength(int threadCount)
        {
            //Calculate length to do not use much memory, but we need length for all threads at least
            var memoryForPool = Settings.ChunkSizeBytes * threadCount;
            var result = Math.Max(Settings.AvailableMemoryBytes / memoryForPool, threadCount);
            return result;
        }
    }
}
