using System;
using System.IO;
using System.Linq;

namespace MultithreadedCompression
{
    class Compressor
    {
        public void Compress(string source, string destination)
        {
            destination = GetCompressedFileNameWithExtension(destination);
            long sourceLength = new System.IO.FileInfo(source).Length;
            var metadata = new Metadata(GetSourceFileExtension(source), sourceLength);

            var pool = new MyThreadPool();
            pool.StartCompressThreadPool(sourceLength, source);

            using (FileStream destinationStream = File.Create(destination))
            {
                destinationStream.Seek(metadata.MetadataSize, SeekOrigin.Begin);

                while (pool.IsAlive() || pool.BufferToWrite.Count > 0)
                {
                    byte[] buffer = null;
                    long offset = 0;
                    int size = 0;
                    lock (pool.LockerBufferToWrite)
                    {
                        if (pool.BufferToWrite.Count > 0)
                        {
                            //Console.WriteLine(pool.BufferToWrite.Count+" "+ pool.BufferToWrite.Count*Settings.ChunkSizeBytes);
                            offset = pool.BufferToWrite.First().Key;
                            buffer = pool.BufferToWrite.First().Value;
                            size = pool.BufferToWrite.First().Value.Length;
                            pool.BufferToWrite.Remove(offset);
                        }
                    }

                    if (buffer!=null)
                    {
                        metadata.AddChunk(destinationStream.Position, offset);
                        destinationStream.Write(buffer, 0, size);
                        //Console.WriteLine(string.Intern("Write ") + offset);
                    }
                }

                destinationStream.Seek(0, SeekOrigin.Begin);
                var metadataBytes = metadata.GetBytes();
                destinationStream.Write(metadataBytes, 0, metadata.MetadataSize);
                destinationStream.Close();
            }
            Console.WriteLine($"Compressed {source} to {destination}");
        }

        /// <summary>
        /// Get compressed file name with extension
        /// </summary>
        /// <param name="destinationName">file name entered</param>
        /// <returns>file name with extension</returns>
        private string GetCompressedFileNameWithExtension(string destinationName)
        {
            return destinationName.EndsWith(Settings.CompressedExtension)
                ? destinationName
                : destinationName + Settings.CompressedExtension;
        }

        /// <summary>
        /// Get source file extension for compressing
        /// </summary>
        /// <param name="sourceName">source file name</param>
        /// <returns>source extension</returns>
        private string GetSourceFileExtension(string sourceName)
        {
            return sourceName.Remove(0, sourceName.LastIndexOf('.'));
        }
    }
}