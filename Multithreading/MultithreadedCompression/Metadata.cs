using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultithreadedCompression
{
    class Metadata
    {
        const string _magicWord = "gzip";
        internal int MetadataSize { get; }
        internal string FileExtension { get; }
        internal List<ChunkMetadata> ChunkList { get; }

        internal Metadata(string fileExtension, long sourceSize)
        {
            FileExtension = fileExtension;
            ChunkList = new List<ChunkMetadata>();
            MetadataSize = _magicWord.Length
                + sizeof(int)
                + 8
                + (int)Math.Ceiling((double)sourceSize / Settings.ChunkSizeBytes) * ChunkMetadata.SizeOf();
        }

        internal Metadata(byte[] bytes, int metadataSize)
        {
            var byteList = bytes.ToList();
            int offset = 0;
            if (Encoding.Default.GetString(byteList.GetRange(offset, _magicWord.Length).ToArray()) != _magicWord)
                throw new WrongSourceFileException("Source file have not been compressed by this compressor.");
            offset += _magicWord.Length;
            MetadataSize = metadataSize;
            offset += sizeof(int);
            FileExtension = Encoding.Default.GetString(byteList.GetRange(offset, 8).ToArray()).Replace("\0", "");
            offset += 8;
            ChunkList = new List<ChunkMetadata>();
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

        internal byte[] GetBytes()
        {
            var bytes = new List<byte>();
            //Add magic word
            var magicWordBytes = Encoding.Default.GetBytes(_magicWord);
            //Add file extension and fill empty bytes for length 8
            var fileExtensionBytes = new List<byte>();
            fileExtensionBytes.AddRange(Encoding.Default.GetBytes(FileExtension));
            for (int i = fileExtensionBytes.Count; i < 8; i++)
                fileExtensionBytes.Add(new byte());
            //Add chunk info
            var chunkInfoBytes = new List<byte>();
            foreach (var chunk in ChunkList)
            {
                chunkInfoBytes.AddRange(chunk.GetBytes());
            }

            bytes.AddRange(magicWordBytes);
            bytes.AddRange(BitConverter.GetBytes(MetadataSize));
            bytes.AddRange(fileExtensionBytes);
            bytes.AddRange(chunkInfoBytes);

            return bytes.ToArray();
        }

        internal void AddChunk(long compressedOffset, long decompressedOffset)
        {
            var chunk = new ChunkMetadata(compressedOffset, decompressedOffset);
            ChunkList.Add(chunk);
        }
    }

    internal class ChunkMetadata
    {
        internal long CompressedOffset { get; }
        internal long DecompressedOffset { get; }

        internal static int SizeOf()
        {
            return sizeof(long) + sizeof(long);
        }

        internal ChunkMetadata(long compressedOffset, long decompressedOffset)
        {
            CompressedOffset = compressedOffset;
            DecompressedOffset = decompressedOffset;
        }

        internal byte[] GetBytes()
        {
            var bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(CompressedOffset));
            bytes.AddRange(BitConverter.GetBytes(DecompressedOffset));
            return bytes.ToArray();
        }
    }
}
