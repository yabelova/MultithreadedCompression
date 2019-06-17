using System;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultithreadedCompression;

namespace MultithreadedCompressionTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestCompressTxt()
        {
            var path = Environment.CurrentDirectory;
            Settings.SetSettings(2, 1024*1024);
            File.Delete(path + "\\ResultCompressedTxt.gz");
            Program.Compress(path + "\\FileForCompress.txt", path + "\\ResultCompressedTxt");
            Assert.IsTrue(File.Exists(path + "\\ResultCompressedTxt" + Settings.CompressedExtension));
        }

        [TestMethod]
        public void TestDecompressTxt()
        {
            var path = Environment.CurrentDirectory;
            Settings.SetSettings(2, 1024 * 1024);
            File.Delete(path + "\\ResultDecompressedTxt.txt");
            Program.Decompress(path + "\\ResultCompressedTxt.gz", path + "\\ResultDecompressedTxt");
            Assert.IsTrue(File.Exists(path + "\\ResultDecompressedTxt.txt"));
        }

        [TestMethod]
        public void TestCompressDll()
        {
            var path = Environment.CurrentDirectory;
            Settings.SetSettings(2, 1024 * 1024);
            File.Delete(path + "\\ResultCompressedDll.gz");
            Program.Compress(path + "\\MultithreadedCompressionTest.dll", path + "\\ResultCompressedDll");
            Assert.IsTrue(File.Exists(path + "\\ResultCompressedDll" + Settings.CompressedExtension));
        }

        [TestMethod]
        public void TestDecompressDll()
        {
            var path = Environment.CurrentDirectory;
            Settings.SetSettings(2, 1024 * 1024);
            File.Delete(path + "\\ResultDecompressedDll.dll");
            Program.Decompress(path + "\\ResultCompressedDll.gz", path + "\\ResultDecompressedDll");
            Assert.IsTrue(File.Exists(path + "\\ResultDecompressedDll.dll"));
        }
    }
}
//foreach (var chunk in metadata.ChunkList)
//{
//    sourceStream.Seek(chunk.CompressedOffset, SeekOrigin.Begin);
//    byte[] chunkBytes = new byte[chunk.CompressedSize];
//    sourceStream.Read(chunkBytes, 0, chunk.CompressedSize);
//    byte[] chunkBytes1 = new byte[Settings.ChunkSizeBytes];

//    using (MemoryStream chunkStream = new MemoryStream(chunkBytes))
//    {
//        using (GZipStream zipStream = new GZipStream(chunkStream, CompressionMode.Decompress))
//        {
//            try
//            {
//                zipStream.Read(chunkBytes1, 0, Settings.ChunkSizeBytes);
//            }
//            catch (Exception ex)
//            {

//            }
//        }
//        destinationStream.Seek(chunk.DecompressedOffset, SeekOrigin.Begin);
//        destinationStream.Write(chunkBytes1, 0, chunkBytes1.Length);
//    }
//}