using System;
using System.IO;
using System.IO.Compression;

namespace MultithreadedCompression
{
    internal sealed class Compressor : BaseProcessor
    {
        internal Compressor(string source, string destination) : base(source, Helper.GetCompressedFileNameWithExtension(destination))
        {
        }

        internal override int Run()
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //Initializing
                if (string.Equals(SourceFileName, DestinationFileName))
                    throw new WrongCallException("Source and destination files can not be the same.");
                long sourceLength = new FileInfo(SourceFileName).Length;
                base.Metadata = new Metadata(SourceFileName, sourceLength);
                var compressThreadCount = Helper.GetCompressThreadsCount(sourceLength);
                var queueLength = Helper.GetMaxTaskQueueLength(compressThreadCount);

                PositionToRead = new SharingPosition(0, sourceLength, Settings.ChunkSizeBytes);
                QueueToWrite = new SharingTaskQueue<Task>(compressThreadCount, queueLength);

                //Start work
                base.StartStatusThread("Compressing", 100 / Math.Ceiling((double)sourceLength / Settings.ChunkSizeBytes));
                base.StartProcessThreads(compressThreadCount, "ReaderCompressor", StartReadAndCompress);
                StartWrite();

                //Finish work
                ExecutionStatus.FinishStatus(true);
                base.JoinProcessThreads();
                base.JoinStatusThread();

                watch.Stop();

                Console.WriteLine($"Compressed {SourceFileName} to {DestinationFileName} in {watch.Elapsed.ToString("g")}");
                return 0;
            }
            catch (WrongCallException ex)
            {
                base.AbortAllThreads();
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                base.AbortAllThreads();
                Console.WriteLine("Error while compressing: " + ex.Message);
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
            //var threadName = Thread.CurrentThread.Name ?? "Main";
            //Console.WriteLine($"{threadName} start");
            using (var destinationStream = new FileStream(DestinationFileName, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    destinationStream.Seek(Metadata.MetadataSize, SeekOrigin.Begin);

                    while (true)
                    {
                        var task = QueueToWrite.Dequeue();
                        if (task == null)
                            break;
                        if (task.RethrowException != null)
                            throw new Exception(task.RethrowException);
                        //Console.WriteLine($"{threadName} writing {task.UncompressedOffset}");

                        Metadata.AddChunk(destinationStream.Position, task.UncompressedOffset);
                        destinationStream.Write(task.Buffer, 0, task.Buffer.Length);
                        ExecutionStatus.IncrementStatus();
                    }
                    destinationStream.Seek(0, SeekOrigin.Begin);
                    destinationStream.Write(Metadata.GetBytes(), 0, Metadata.MetadataSize);
                }
                catch (Exception ex)
                {
                    destinationStream.Flush(false);
                    throw new Exception(ex.Message);
                }
            }
            //Console.WriteLine($"{threadName} end");
        }

        private void StartReadAndCompress()
        {
            //var threadName = Thread.CurrentThread.Name ?? "";
            //Console.WriteLine($"{threadName} start");
            using (var sourceStream = new FileStream(SourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                try
                {
                    var maxPosition = sourceStream.Length;
                    byte[] buffer = new byte[Settings.ChunkSizeBytes];

                    while (true)
                    {
                        var currentPosition = PositionToRead.GetLastPositionAndIncrement();
                        if (currentPosition < 0)
                            break;
                        //Console.WriteLine($"{threadName} processing {currentPosition}");

                        var chunkSize = (int)Math.Min(Settings.ChunkSizeBytes, maxPosition - currentPosition);
                        sourceStream.Seek(currentPosition, SeekOrigin.Begin);
                        sourceStream.Read(buffer, 0, chunkSize);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress))
                                gs.Write(buffer, 0, chunkSize);

                            QueueToWrite.Enqueue(new Task(currentPosition, ms.ToArray()));
                        }
                    }
                }
                catch (OutOfMemoryException)
                {
                    QueueToWrite.Enqueue(new Task("There is no enough memory to continue"));
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
    }
}