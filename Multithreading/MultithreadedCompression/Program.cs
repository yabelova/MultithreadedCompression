using System;
using System.IO;
using System.Linq;

namespace MultithreadedCompression
{
    sealed class Program
    {
        static int Main(string[] args)
        {
            var result = Run(args);
            //Console.WriteLine("Press any key to exit");
            //Console.ReadKey();
            return result;
        }

        internal static int Run(string[] args)
        {
            if (CheckParameters(args))
            {
                return args[0] == "compress"
                      ? Compress(args[1], args[2])
                      : Decompress(args[1], args[2]);
            }
            return 1;
        }

        internal static bool CheckParameters(string[] args)
        {
            try
            {
                if (args.Count() != 3)
                    throw new WrongCallException("Wrong number of parameters.");

                var command = args[0];
                var source = args[1];
                var destination = args[2];

                if (command != "compress" && command != "decompress")
                    throw new WrongCallException($"Wrong first parameter '{command}'.");
                if (!File.Exists(source))
                    throw new WrongSourceFileException($"File '{source}' is not found.");
                //if (command == "compress" && source.EndsWith(Settings.CompressedFileExtension))
                //    throw new WrongSourceFileException($"File '{source}' is already compressed.");
                if (command == "decompress" && !source.EndsWith(Settings.CompressedFileExtension))
                    throw new WrongSourceFileException($"File '{source}' can not be decompressed. It should have extension *{Settings.CompressedFileExtension}.");

                return true;
            }
            catch (WrongCallException ex)
            {
                Console.WriteLine($"Error parameters: {ex.Message}\n" +
                                  "The correct call for compression is 'GZipTest.exe compress [source_file_name] [result_file_name]'.\n" +
                                  "The correct call for decompression is 'GZipTest.exe decompress [source_file_name] [result_file_name]'.");
            }
            catch (WrongSourceFileException ex)
            {
                Console.WriteLine($"Error source file: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }

        internal static int Compress(string source, string destination)
        {
            return new Compressor(source, destination).Run();
        }

        internal static int Decompress(string source, string destination)
        {
            return new Decompressor(source, destination).Run();
        }
    }
}