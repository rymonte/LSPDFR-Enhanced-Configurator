using System;
using System.IO;
using System.Threading;

namespace LSPDFREnhancedConfigurator.Services
{
    /// <summary>
    /// Log level enum for controlling verbosity
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,    // All logging including UI events
        Debug = 1,    // Debug diagnostics + Info + Warn + Error
        Info = 2,     // Info + Warn + Error (DEFAULT for production)
        Warn = 3,     // Warn + Error only
        Error = 4     // Error only
    }

    /// <summary>
    /// Simple file-based logger with configurable log levels
    /// Thread-safe logging to log.txt
    /// </summary>
    public static class Logger
    {
        private static readonly string LogFilePath;
        private static readonly object LockObject = new object();
        private static bool _isInitialized = false;

        /// <summary>
        /// Current log level - controls which messages are logged
        /// </summary>
        public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Info;

        static Logger()
        {
            // Place log.txt next to the executable
            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            LogFilePath = Path.Combine(appDirectory, "log.txt");
        }

        /// <summary>
        /// Initialize the logger and create/clear log file
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                lock (LockObject)
                {
                    // Create new log file for this session
                    File.WriteAllText(LogFilePath, "");
                    _isInitialized = true;

                    Info("=== LSPDFR Enhanced Configurator ===");
                    Info($"Session started: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    Info($"Log file: {LogFilePath}");
                    Info("");
                }
            }
            catch (Exception ex)
            {
                // Can't log if we can't write to log file
                // Write to console as fallback
                Console.WriteLine($"Failed to initialize log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Log a TRACE level message (highest verbosity - UI events)
        /// </summary>
        public static void Trace(string message)
        {
            if (CurrentLogLevel <= LogLevel.Trace)
                Log("TRACE", message);
        }

        /// <summary>
        /// Log a DEBUG level message (diagnostics and counts)
        /// </summary>
        public static void Debug(string message)
        {
            if (CurrentLogLevel <= LogLevel.Debug)
                Log("DEBUG", message);
        }

        /// <summary>
        /// Log an INFO level message
        /// </summary>
        public static void Info(string message)
        {
            if (CurrentLogLevel <= LogLevel.Info)
                Log("INFO", message);
        }

        /// <summary>
        /// Log a WARN level message
        /// </summary>
        public static void Warn(string message)
        {
            if (CurrentLogLevel <= LogLevel.Warn)
                Log("WARN", message);
        }

        /// <summary>
        /// Log an ERROR level message
        /// </summary>
        public static void Error(string message)
        {
            if (CurrentLogLevel <= LogLevel.Error)
                Log("ERROR", message);
        }

        /// <summary>
        /// Log an ERROR level message with exception details
        /// </summary>
        public static void Error(string message, Exception ex)
        {
            Log("ERROR", $"{message}\n  Exception: {ex.GetType().Name}\n  Message: {ex.Message}\n  StackTrace: {ex.StackTrace}");
        }

        /// <summary>
        /// Core logging method - thread-safe write to file
        /// </summary>
        private static void Log(string level, string message)
        {
            if (!_isInitialized)
                Initialize();

            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var threadId = Thread.CurrentThread.ManagedThreadId;
                var logEntry = $"[{timestamp}] [{level}] [Thread-{threadId}] {message}";

                lock (LockObject)
                {
                    File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                }

                // Also write to debug output (visible in Visual Studio)
                System.Diagnostics.Debug.WriteLine(logEntry);
            }
            catch
            {
                // Silently fail - don't let logging errors crash the app
            }
        }

        /// <summary>
        /// Log separator line for readability
        /// </summary>
        public static void Separator()
        {
            Log("INFO", "------------------------------------------------------------");
        }

        /// <summary>
        /// Log section header
        /// </summary>
        public static void Section(string sectionName)
        {
            Separator();
            Info($">>> {sectionName}");
            Separator();
        }

        /// <summary>
        /// Get the log file path
        /// </summary>
        public static string GetLogFilePath()
        {
            return LogFilePath;
        }

        /// <summary>
        /// Close the current log session
        /// </summary>
        public static void Close()
        {
            try
            {
                Info("");
                Info($"Session ended: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Info("=== End of Log ===");
            }
            catch
            {
                // Silently fail
            }
        }
    }
}
