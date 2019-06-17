using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Threading;

namespace MultithreadedCompression
{
    internal class Program
    {
        internal static int Main(string[] args)
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

                Settings.SetSettings(2, 1024);

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

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs _args)
        {

            //
            //Console.WriteLine("\nCancelling...");
            //_args.Cancel = true;
            //zipper.Cancel();

        }

        internal static void Compress(string source, string destination)
        {
            var compressor = new Compressor();
            compressor.Compress(source, destination);

        }


        internal static void Decompress(string source, string destination)
        {
            var compressor = new Compressor();
            compressor.Decompress(source, destination);

        }
    }




}
