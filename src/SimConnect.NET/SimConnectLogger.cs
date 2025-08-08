// <copyright file="SimConnectLogger.cs" company="BARS">
// Copyright (c) BARS. All rights reserved.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimConnect.NET
{
    /// <summary>
    /// Lightweight, modular logger for SimConnect.NET with asynchronous, file-based logging.
    /// Writes to %LOCALAPPDATA%\SimConnect.NET\log.log by default and supports severity levels.
    /// </summary>
    public sealed class SimConnectLogger : IDisposable
    {
        private const string DefaultFolderName = "SimConnect.NET";
        private const string DefaultFileName = "log.log";

        private static readonly Lazy<SimConnectLogger> LazyInstance = new(() => new SimConnectLogger());

        private readonly BlockingCollection<(DateTime TimestampUtc, LogLevel Level, string Message)> queue = new(new ConcurrentQueue<(DateTime, LogLevel, string)>());
        private readonly CancellationTokenSource cts = new();
        private readonly Task worker;
        private ILogSink? sink;
        private bool disposed;

        private SimConnectLogger()
        {
            try
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var folder = Path.Combine(localAppData, DefaultFolderName);
                Directory.CreateDirectory(folder);
                var filePath = Path.Combine(folder, DefaultFileName);
                this.sink = new FileLogSink(filePath);
            }
            catch (Exception ex)
            {
                // Fall back to debug output only if file sink fails.
                System.Diagnostics.Debug.WriteLine($"SimConnectLogger: Failed to initialize file sink: {ex.Message}");
                this.sink = new DebugLogSink();
            }

            this.worker = Task.Factory.StartNew(this.ProcessQueue, TaskCreationOptions.LongRunning);

            AppDomain.CurrentDomain.ProcessExit += (_, __) => this.Dispose();
            AppDomain.CurrentDomain.DomainUnload += (_, __) => this.Dispose();
        }

        /// <summary>
        /// Log severity levels.
        /// </summary>
        public enum LogLevel
        {
            /// <summary>Debug logs.</summary>
            Debug = 1,

            /// <summary>Informational logs.</summary>
            Info = 2,

            /// <summary>Warning logs.</summary>
            Warning = 3,

            /// <summary>Error logs.</summary>
            Error = 4,
        }

        /// <summary>
        /// Abstraction for a log sink target.
        /// </summary>
        private interface ILogSink : IDisposable
        {
            /// <summary>
            /// Writes a full line to the sink.
            /// </summary>
            /// <param name="line">The line to write.</param>
            void WriteLine(string line);
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static SimConnectLogger Instance => LazyInstance.Value;

        /// <summary>
        /// Gets or sets the minimum log level to write. Defaults to <see cref="LogLevel.Debug"/>.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Gets or sets a value indicating whether to also write messages to <see cref="System.Diagnostics.Debug"/> output.
        /// </summary>
        public bool AlsoWriteToDebug { get; set; }

        /// <summary>
        /// Configures the logger.
        /// </summary>
        /// <param name="minimumLevel">Minimum level to log.</param>
        /// <param name="logFilePath">Optional path to log file. If null, defaults to %LOCALAPPDATA%\SimConnect.NET\log.log.</param>
        /// <param name="alsoWriteToDebug">Whether to also write to Debug output.</param>
        public static void Configure(LogLevel minimumLevel = LogLevel.Debug, string? logFilePath = null, bool alsoWriteToDebug = false)
        {
            var logger = Instance;
            logger.MinimumLevel = minimumLevel;
            logger.AlsoWriteToDebug = alsoWriteToDebug;

            if (logFilePath != null)
            {
                try
                {
                    var directory = Path.GetDirectoryName(logFilePath);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    var newSink = new FileLogSink(logFilePath);
                    var oldSink = logger.ExchangeSink(newSink);
                    oldSink?.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SimConnectLogger.Configure: Failed to set file sink: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Debug(string message) => Instance.Enqueue(LogLevel.Debug, message);

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Info(string message) => Instance.Enqueue(LogLevel.Info, message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void Warning(string message) => Instance.Enqueue(LogLevel.Warning, message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">Optional exception.</param>
        public static void Error(string message, Exception? exception = null)
        {
            if (exception == null)
            {
                Instance.Enqueue(LogLevel.Error, message);
            }
            else
            {
                var full = new StringBuilder(message).Append(':').Append(' ').Append(exception.Message).ToString();
                Instance.Enqueue(LogLevel.Error, full);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            try
            {
                this.queue.CompleteAdding();
                this.cts.Cancel();
            }
            catch
            {
                // ignored
            }

            try
            {
                this.worker.Wait(2000);
            }
            catch
            {
                // ignored
            }

            try
            {
                this.sink?.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        private static string Format(DateTime timestampUtc, LogLevel level, string message)
        {
            return $"{timestampUtc:O} [{level}] {message}";
        }

        private ILogSink? ExchangeSink(ILogSink? newSink)
        {
            return Interlocked.Exchange(ref this.sink, newSink);
        }

        private void Enqueue(LogLevel level, string message)
        {
            if (this.disposed)
            {
                return;
            }

            if (level < this.MinimumLevel)
            {
                return;
            }

            try
            {
                this.queue.Add((DateTime.UtcNow, level, message));
            }
            catch
            {
                // ignored
            }
        }

        private void ProcessQueue()
        {
            try
            {
                foreach (var item in this.queue.GetConsumingEnumerable(this.cts.Token))
                {
                    var line = Format(item.TimestampUtc, item.Level, item.Message);

                    try
                    {
                        this.sink?.WriteLine(line);
                    }
                    catch (Exception ex)
                    {
                        // If sink fails at runtime, try to swap to debug sink.
                        System.Diagnostics.Debug.WriteLine($"SimConnectLogger: Write failed: {ex.Message}");
                        this.ExchangeSink(new DebugLogSink());
                    }

                    if (this.AlsoWriteToDebug)
                    {
                        System.Diagnostics.Debug.WriteLine(line);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SimConnectLogger worker crashed: {ex.Message}");
            }
        }

        private sealed class FileLogSink : ILogSink
        {
            private readonly StreamWriter writer;
            private readonly object gate = new();

            /// <summary>
            /// Initializes a new instance of the <see cref="FileLogSink"/> class.
            /// </summary>
            /// <param name="filePath">Target log file path.</param>
            public FileLogSink(string filePath)
            {
                // Open for append, allow readers to open the file concurrently.
                var fileStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                this.writer = new StreamWriter(fileStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
                {
                    AutoFlush = false,
                    NewLine = Environment.NewLine,
                };
            }

            /// <inheritdoc />
            public void WriteLine(string line)
            {
                lock (this.gate)
                {
                    this.writer.WriteLine(line);
                    this.writer.Flush();
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {
                lock (this.gate)
                {
                    this.writer.Dispose();
                }
            }
        }

        private sealed class DebugLogSink : ILogSink
        {
            /// <inheritdoc />
            public void WriteLine(string line)
            {
                System.Diagnostics.Debug.WriteLine(line);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                // no-op
            }
        }
    }
}
