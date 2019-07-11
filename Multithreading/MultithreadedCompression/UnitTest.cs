using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MultithreadedCompression
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestCompressTxt()
        {
            var path = Environment.CurrentDirectory + "\\test";
            Settings.SetSettings(2, 1024);
            File.Delete(path + "\\ResultCompressedTxt.gz");
            Program.Compress(path + "\\FileForCompress.docx", path + "\\ResultCompressedTxt");
            Assert.IsTrue(File.Exists(path + "\\ResultCompressedTxt" + Settings.CompressedExtension));
        }

        [TestMethod]
        public void TestDecompressTxt()
        {
            var path = Environment.CurrentDirectory + "\\test";
            Settings.SetSettings(2, 1024 * 1024);
            File.Delete(path + "\\ResultDecompressedTxt.docx");
            Program.Decompress(path + "\\ResultCompressedTxt.gz", path + "\\ResultDecompressedTxt");
            Assert.IsTrue(File.Exists(path + "\\ResultDecompressedTxt.docx"));
        }

        [TestMethod]
        public void TestCompressDll()
        {
            var path = Environment.CurrentDirectory+"\\test";
            Settings.SetSettings(2, 1024 * 1024);
            File.Delete(path + "\\ResultCompressedDll.gz");
            Program.Compress(path + "\\FileForCompress.dll", path + "\\ResultCompressedDll");
            Assert.IsTrue(File.Exists(path + "\\ResultCompressedDll" + Settings.CompressedExtension));
        }

        [TestMethod]
        public void TestDecompressDll()
        {
            var path = Environment.CurrentDirectory + "\\test";
            Settings.SetSettings(2, 1024 * 1024);
            File.Delete(path + "\\ResultDecompressedDll.dll");
            Program.Decompress(path + "\\ResultCompressedDll.gz", path + "\\ResultDecompressedDll");
            Assert.IsTrue(File.Exists(path + "\\ResultDecompressedDll.dll"));
        }

        [TestMethod]
        public void TestCompressJpg()
        {
            var path = Environment.CurrentDirectory + "\\test";
            Settings.SetSettings(2, 1024 * 1024);
            File.Delete(path + "\\ResultCompressedJpg.gz");
            Program.Compress(path + "\\FileForCompress.jpg", path + "\\ResultCompressedJpg");
            Assert.IsTrue(File.Exists(path + "\\ResultCompressedJpg" + Settings.CompressedExtension));
        }

        [TestMethod]
        public void TestDecompressJpg()
        {
            var path = Environment.CurrentDirectory + "\\test";
            Settings.SetSettings(2, 1024 * 1024);
            File.Delete(path + "\\ResultDecompressedJpg.jpg");
            Program.Decompress(path + "\\ResultCompressedJpg.gz", path + "\\ResultDecompressedJpg");
            Assert.IsTrue(File.Exists(path + "\\ResultDecompressedJpg.jpg"));
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