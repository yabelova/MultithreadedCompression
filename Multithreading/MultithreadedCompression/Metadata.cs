using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCompression
{
    internal sealed class Metadata
    {
        private readonly string _magicWord = "gzip";
        private readonly byte[] _magicWordBytes;
        internal readonly string FileExtension;
        internal readonly int MetadataSize;
        internal readonly List<ChunkMetadata> ChunkList = new List<ChunkMetadata>();

        /// <summary>
        /// Using to compress
        /// </summary>
        internal Metadata(string source, long sourceSize)
        {
            _magicWordBytes = Encoding.UTF8.GetBytes(_magicWord);
            FileExtension = Helper.GetFileExtension(source);
            MetadataSize = _magicWordBytes.Length
                + sizeof(int)
                + 8
                + (int)Math.Ceiling((double)sourceSize / Settings.ChunkSizeBytes) * ChunkMetadata.SizeOf();
        }

        /// <summary>
        /// Using to decompress (parse bytes to Metadata type)
        /// </summary>
        internal Metadata(byte[] bytes, int metadataSize)
        {
            try
            {
                _magicWordBytes = Encoding.UTF8.GetBytes(_magicWord);
                var byteList = bytes.ToList();
                int offset = 0;
                if (Encoding.UTF8.GetString(byteList.GetRange(offset, _magicWordBytes.Length).ToArray()) != _magicWord)
                    throw new WrongSourceFileException("File header has been corrupted or file has not been compressed by this compressor.");
                offset += _magicWordBytes.Length;
                MetadataSize = metadataSize;
                offset += sizeof(int);
                FileExtension = Encoding.UTF8.GetString(byteList.GetRange(offset, 8).ToArray()).Replace("\0", "");
                offset += 8;
                while (offset < byteList.Count)
                {
                    var compressedOffset = BitConverter.ToInt64(byteList.GetRange(offset, sizeof(long)).ToArray(), 0);
                    offset += sizeof(long);
                    var decompressedOffset = BitConverter.ToInt64(byteList.GetRange(offset, sizeof(long)).ToArray(), 0);
                    offset += sizeof(long);
                    var chunk = new ChunkMetadata(compressedOffset, decompressedOffset);
                    ChunkList.Add(chunk);
                }
            }
            catch (WrongSourceFileException ex)
            {
                throw new WrongSourceFileException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        internal byte[] GetBytes()
        {
            var bytes = new List<byte>();
            //Add magic word
            //Add file extension and fill empty bytes for length 8
            var fileExtensionBytes = new List<byte>();
            fileExtensionBytes.AddRange(Encoding.UTF8.GetBytes(FileExtension));
            for (int i = fileExtensionBytes.Count; i < 8; i++)
                fileExtensionBytes.Add(new byte());
            //Add chunk info
            var chunkInfoBytes = new List<byte>();
            foreach (var chunk in ChunkList)
            {
                chunkInfoBytes.AddRange(chunk.GetBytes());
            }

            bytes.AddRange(_magicWordBytes);
            bytes.AddRange(BitConverter.GetBytes(MetadataSize));
            bytes.AddRange(fileExtensionBytes);
            bytes.AddRange(chunkInfoBytes);

            return bytes.ToArray();
        }

        internal void AddChunk(long compressedOffset, long decompressedOffset)
        {
            ChunkList.Add(new ChunkMetadata(compressedOffset, decompressedOffset));
        }
    }

    internal struct ChunkMetadata
    {
        internal readonly long CompressedOffset;
        internal readonly long UncompressedOffset;

        internal ChunkMetadata(long compressedOffset, long uncompressedOffset)
        {
            CompressedOffset = compressedOffset;
            UncompressedOffset = uncompressedOffset;
        }

        internal static int SizeOf()
        {
            return sizeof(long) + sizeof(long);
        }

        internal byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(CompressedOffset));
            bytes.AddRange(BitConverter.GetBytes(UncompressedOffset));
            return bytes.ToArray();
        }
    }
}
