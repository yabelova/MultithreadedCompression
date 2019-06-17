using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace MultithreadedCompression
{

    class Compressor
    {

        public Compressor()
        {
        }

        public void Compress(string source, string destination)
        {
            destination = GetCompressedFileNameWithExtension(destination);

            using (FileStream sourceStream = File.OpenRead(source))
            using (FileStream destinationStream = File.Create(destination))
            {
                var metadata = new Metadata(GetSourceFileExtension(source), sourceStream.Length);
                destinationStream.Seek(metadata.MetadataSize, SeekOrigin.Begin);
                for (long offset = 0; offset < sourceStream.Length; offset += Settings.ChunkSizeBytes)
                {
                    var compressedOffset = destinationStream.Position;
                    using (GZipStream zipStream = new GZipStream(destinationStream, CompressionMode.Compress, true))
                    {
                        sourceStream.CopyTo(zipStream, Settings.ChunkSizeBytes);
                    }
                    metadata.AddChunk(compressedOffset, offset);
                }
                destinationStream.Seek(0, SeekOrigin.Begin);
                var metadataBytes = metadata.GetBytes();
                destinationStream.Write(metadataBytes, 0, metadata.MetadataSize);
            }

            Console.WriteLine($"Compressed {source} to {destination}");
        }

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
