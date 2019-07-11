using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.IO.Compression;

namespace MultithreadedCompression
{
    class Program
    {
        static void Main(string[] args)
        {
            var ts = DateTime.Now;
            //ReadFileStream();

            var pathIn = @"d:/11.txt";
            var pathOut = @"d:/new";
            Process(new string[] { "compress", pathIn, pathOut });

            Console.WriteLine(DateTime.Now - ts);

            ts = DateTime.Now;
            //ReadFileStream();

            pathIn = @"d:/new.gz";
            pathOut = @"d:/new.txt";
            Process(new string[] { "decompress", pathIn, pathOut });

            Console.WriteLine(DateTime.Now - ts);
            Console.ReadKey();
        }

        internal static int Process(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPress);

            try
            {
                if (args.Count() != 3)
                    throw new WrongCallException("Wrong number of parameters.");

                var command = args[0];
                var source = args[1];
                var destination = args[2];

                if (command != "compress" && command != "decompress")
                    throw new WrongCallException($"Wrong '{command}' parameter.");
                if (!File.Exists(source))
                    throw new WrongSourceFileException($"File '{source}' is not found.");
                if (command == "compress" && source.EndsWith(Settings.CompressedExtension))
                    throw new WrongSourceFileException($"File '{source}' is already compressed.");
                if (command == "decompress" && !source.EndsWith(Settings.CompressedExtension))
                    throw new WrongSourceFileException($"File '{source}' can not be decompressed.");

                // Console.ReadKey();

                Settings.SetSettings(7, 1024 * 1024);

                if (command == "compress")
                    Compress(source, destination);
                else
                    Decompress(source, destination);

                return 0;
            }
            catch (WrongCallException ex)
            {
                Console.WriteLine($"Error parameters: {ex.Message}\n" +
                                  "The correct call for compression is 'GZipTest.exe compress [source file name] [result file name]'.\n" +
                                  "The correct call for decompression is 'GZipTest.exe decompress [source file name] [result file name]'.");
                return 1;
            }
            catch (WrongSourceFileException ex)
            {
                Console.WriteLine($"Error source file: {ex.Message}\n");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            finally
            {
                // Console.ReadKey();
            }
        }

        internal static void Compress(string source, string destination)
        {
            var compressor = new Compressor();
            compressor.Compress(source, destination);
        }

        internal static void Decompress(string source, string destination)
        {
            var decompressor = new Decompressor();
            decompressor.Decompress(source, destination);
        }

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs _args)
        {
            //Console.WriteLine("\nCancelling...");
            //_args.Cancel = true;
            //zipper.Cancel();
        }



        ////--------------------------------------------------------//
        //private static int iLast = 0;
        //private static object locker = new object();

        //private static object lockerPos = new object();
        //private static object lockerRes = new object();

        //private static long posLast = 0;
        //private static readonly int bufferSize = 1024 * 1024;
        //private static List<byte[]> res;


        //private static void ReadFileStream()
        //{
        //    Console.WriteLine("main: start " + bufferSize);

        //    var pool = CreatePoolReadFileStream(7); //Environment.ProcessorCount );
        //    var pathOld = "old.txt";
        //    var pathNew = "new.gz";
        //    var fs = new FileStream(pathOld, FileMode.Open, FileAccess.Read);
        //    var nfs = new FileStream(pathNew, FileMode.Create, FileAccess.Write);
        //    res = new List<byte[]>();
        //    StartPoolReadFileStream(pool, pathOld);
        //    long resultByte = 0;
        //    int p = 1;
        //    while (pool.Any(x => x.IsAlive) || res.Count > 0)
        //    {
        //        byte[] str = null;

        //        lock (lockerRes)
        //        {
        //            if (res.Count > 0)
        //            {
        //                str = res[0];
        //                res.Remove(res[0]);
        //                p++;
        //            }
        //        }

        //        if (str != null)
        //        {
        //            resultByte += str.Length;
        //            nfs.Write(str, 0, str.Length);
        //            Console.WriteLine("main: process " + str.Length);
        //        }

        //    }

        //    Console.WriteLine("main: done " + resultByte + " " + fs.Length + " " + p);
        //    fs.Close();
        //    nfs.Close();
        //}

        //private static Thread[] CreatePoolReadFileStream(int size)
        //{
        //    Thread.CurrentThread.Name = "Thread 0";
        //    var pool = new Thread[size];
        //    for (int i = 0; i < size; i++)
        //        pool[i] = new Thread(MyThreadReadFileStream) { Name = "Thread " + (i + 1) };
        //    return pool;
        //}

        //private static void MyThreadReadFileStream(Object oStream)
        //{
        //    Console.WriteLine(Thread.CurrentThread.Name + " start");
        //    var path = oStream as string;
        //    var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        //    //var fs = oStream as FileStream;
        //    if (fs == null)
        //        return;
        //    var maxPos = fs.Length;
        //    while (true)
        //    {
        //        long curPos;
        //        lock (lockerPos)
        //        {
        //            curPos = posLast;
        //            posLast += bufferSize;
        //        }

        //        if (curPos >= maxPos) break;

        //        Console.WriteLine(Thread.CurrentThread.Name + " processing " + (curPos));
        //        byte[] buffer = new byte[bufferSize];
        //        fs.Seek(curPos, SeekOrigin.Begin);
        //        fs.Read(buffer, 0, bufferSize);

        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress, true))
        //            {
        //                gs.Write(buffer, 0, buffer.Length);
        //            }

        //            lock (lockerRes)
        //            {

        //                res.Add(ms.ToArray());
        //            }

        //            Console.WriteLine(Thread.CurrentThread.Name + " end " + (fs.Position - curPos - bufferSize));

        //        }
        //    }

        //    fs.Close();
        //}

        //private static void StartPoolReadFileStream(Thread[] pool, Object fs)
        //{
        //    foreach (var t in pool)
        //        t.Start(fs);
        //}

        //-----------------------------------------------------//
        /*
         private static void CalcArray()
         {
             Console.WriteLine("main: start");
 
             var pool = CreatePoolCalcArray(Environment.ProcessorCount - 1);
 
             var list = new List<int>();
             for (int i = 0; i < 100; i++)
                 list.Add(i);
 
             StartPoolCalcArray(pool, list);
             MyThreadCalcArray(list);
 
             Console.WriteLine("main: done");
             Console.WriteLine(string.Join(" ", list));
         }
         private static Thread[] CreatePoolCalcArray(int size)
         {
             Thread.CurrentThread.Name = "Thread 0";
             var pool = new Thread[size];
             for (int i = 0; i < size; i++)
                 pool[i] = new Thread(MyThreadCalcArray) {Name = "Thread " + (i + 1)};
             return pool;
         }
         private static void StartPoolCalcArray(Thread[] pool, List<int> list)
         {
             foreach (var t in pool)
                 t.Start(list);
         }
         private static void MyThreadCalcArray(Object oList)
         {
             Console.WriteLine(Thread.CurrentThread.Name + " start");
             var list = oList as List<int>;
             if (list == null)
                 return;
             while (true)
             {
                 int curI;
                 lock (locker)
                 {
                     curI = iLast;
                     iLast++;
                 }
 
                 if (curI >= list.Count) break;
 
                 Console.WriteLine(Thread.CurrentThread.Name + " processing " + (curI));
                 list[curI] *= 10;
                 Thread.Sleep(1000);
             }
         }
 */

    }
}