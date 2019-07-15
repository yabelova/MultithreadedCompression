using System;
using System.IO;
using System.IO.Compression;

namespace MultithreadedCompression
{
    internal sealed class Decompressor : BaseProcessor
    {
        internal Decompressor(string source, string destination) : base(source, destination)
        {
        }

        internal override int Run()
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //Initializing
                ParseMetadataFromSource();
                DestinationFileName = Helper.GetDecompressedFileNameWithExtension(DestinationFileName, Metadata.FileExtension);
                if (string.Equals(SourceFileName, DestinationFileName))
                    throw new WrongCallException("Source and destination files can not be the same.");
                var decompressThreadCount = Helper.GetDecompressThreadsCount(Metadata.ChunkList.Count);
                var queueLength = Helper.GetMaxTaskQueueLength(decompressThreadCount);

                base.PositionToRead = new SharingPosition(0, Metadata.ChunkList.Count, 1);
                base.QueueToWrite = new SharingTaskQueue<Task>(decompressThreadCount, queueLength);

                //Start work
                base.StartStatusThread("Decompressing", (double)100 / Metadata.ChunkList.Count);
                base.StartProcessThreads(decompressThreadCount, "ReaderDecompressor", StartReadAndDecompress);
                StartWrite();

                //Finish work
                ExecutionStatus.FinishStatus(true);
                base.JoinProcessThreads();
                base.JoinStatusThread();

                watch.Stop();

                Console.WriteLine($"Decompressed {SourceFileName} to {DestinationFileName} in {watch.Elapsed.ToString("g")}");
                return 0;
            }
            catch (WrongCallException ex)
            {
                base.AbortAllThreads();
                Console.WriteLine(ex.Message);
            }
            catch (WrongSourceFileException ex)
            {
                base.AbortAllThreads();
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                base.AbortAllThreads();
                Console.WriteLine("Error while decompressing: " + ex.Message);
            }
            finally
            {
                if (ExecutionStatus != null && ExecutionStatus.GetStatus() < 100)
                    File.Delete(DestinationFileName);
            }
            return 1;
        }

        private void StartWrite()
        {
            // var threadName = Thread.CurrentThread.Name ?? "Main";
            //Console.WriteLine($"{threadName} start");
            using (var destinationStream = new FileStream(DestinationFileName, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    while (true)
                    {
                        var task = QueueToWrite.Dequeue();
                        if (task == null)
                            break;
                        if (task.RethrowException != null)
                            throw new Exception(task.RethrowException);
                        //Console.WriteLine($"{threadName} writing {task.UncompressedOffset}");

                        destinationStream.Seek(task.UncompressedOffset, SeekOrigin.Begin);
                        destinationStream.Write(task.Buffer, 0, task.Buffer.Length);
                        ExecutionStatus.IncrementStatus();
                    }
                }
                catch (Exception ex)
                {
                    destinationStream.Flush(false);
                    throw new Exception(ex.Message);
                }
            }
            //Console.WriteLine($"{threadName} end");
        }

        private void StartReadAndDecompress()
        {
            //var threadName = Thread.CurrentThread.Name ?? "Main";
            //Console.WriteLine($"{threadName} start");
            using (var sourceStream = new FileStream(SourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    while (true)
                    {
                        var currentPosition = (int)PositionToRead.GetLastPositionAndIncrement();
                        if (currentPosition < 0)
                            break;
                        var chunk = Metadata.ChunkList[currentPosition];
                        //Console.WriteLine($"{threadName} processing {chunk.CompressedOffset}");

                        sourceStream.Seek(chunk.CompressedOffset, SeekOrigin.Begin);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (GZipStream gs = new GZipStream(sourceStream, CompressionMode.Decompress, true))
                                gs.CopyTo(ms, Settings.ChunkSizeBytes);
                            QueueToWrite.Enqueue(new Task(chunk.UncompressedOffset, ms.ToArray()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    QueueToWrite.Enqueue(new Task(ex.Message));
                }
                finally
                {
                    QueueToWrite.FinishEnqueue();
                }
            }
            //Console.WriteLine($"{threadName} end");
        }

        private void ParseMetadataFromSource()
        {
            using (FileStream sourceStream = File.OpenRead(SourceFileName))
                try
                {
                    //Read metadata size
                    var metadataSizeBytes = new byte[4];
                    sourceStream.Seek(4, SeekOrigin.Begin);
                    sourceStream.Read(metadataSizeBytes, 0, 4);
                    var metadataSize = BitConverter.ToInt32(metadataSizeBytes, 0);
                    //Read metadata
                    sourceStream.Seek(0, SeekOrigin.Begin);
                    var metadataBytes = new byte[metadataSize];
                    sourceStream.Read(metadataBytes, 0, metadataSize);
                    //Parse metadata
                    Metadata = new Metadata(metadataBytes, metadataSize);
                }
                catch (WrongSourceFileException ex)
                {
                    throw new WrongSourceFileException(ex.Message);
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new WrongSourceFileException($"File header has been corrupted or file has not been compressed by this compressor.");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
        }
    }
}