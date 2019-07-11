﻿using System;
using System.IO;
using System.IO.Compression;

namespace MultithreadedCompression
{
    public class Decompressor
    {
        public void Decompress(string source, string destination)
        {
            using (FileStream sourceStream = File.OpenRead(source))
            {
                var metadataSizeBytes = new byte[4];
                sourceStream.Seek(4, SeekOrigin.Begin);
                sourceStream.Read(metadataSizeBytes, 0, 4);
                var metadataSize = BitConverter.ToInt32(metadataSizeBytes, 0);
                sourceStream.Seek(0, SeekOrigin.Begin);
                var metadataBytes = new byte[metadataSize];
                sourceStream.Read(metadataBytes, 0, metadataSize);
                var metadata = new Metadata(metadataBytes, metadataSize);

                destination = GetDecompressedFileNameWithExtension(destination, metadata.FileExtension);
                using (FileStream destinationStream = File.Create(destination))
                {
                    foreach (var chunk in metadata.ChunkList)
                    {
                        sourceStream.Seek(chunk.CompressedOffset, SeekOrigin.Begin);
                        destinationStream.Seek(chunk.DecompressedOffset, SeekOrigin.Begin);
                        using (GZipStream zipStream = new GZipStream(sourceStream, CompressionMode.Decompress, true))
                        {
                            zipStream.CopyTo(destinationStream, Settings.ChunkSizeBytes);
                        }
                    }
                }
            }
            Console.WriteLine($"Decompressed {source} to {destination}");
        }

        /// <summary>
        /// Get decompressed file name with extension
        /// </summary>
        /// <param name="destinationName">file name entered</param>
        /// <param name="extension">file extension from metadata</param>
        /// <returns>file name with extension</returns>
        private string GetDecompressedFileNameWithExtension(string destinationName, string extension)
        {
            return destinationName.EndsWith(extension)
                ? destinationName
                : destinationName + extension;
        }
    }
}