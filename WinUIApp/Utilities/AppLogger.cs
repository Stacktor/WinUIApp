using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WinUIApp.Utilities
{
    /// <summary>
    /// Application logger for debugging and error tracking
    /// </summary>
    public static class AppLogger
    {
        private static readonly object _lockObject = new object();
        private static string _logFilePath;

        static AppLogger()
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                _logFilePath = Path.Combine(localFolder, "app_log.txt");
            }
            catch
            {
                _logFilePath = null;
            }
        }

        /// <summary>
        /// Logs an informational message
        /// </summary>
        public static void LogInfo(string message, string source = null)
        {
            Log("INFO", message, source);
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public static void LogWarning(string message, string source = null)
        {
            Log("WARNING", message, source);
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        public static void LogError(string message, Exception ex = null, string source = null)
        {
            var fullMessage = ex != null ? $"{message}\nException: {ex.Message}\nStack Trace: {ex.StackTrace}" : message;
            Log("ERROR", fullMessage, source);
        }

        /// <summary>
        /// Logs a debug message (only in debug builds)
        /// </summary>
        [Conditional("DEBUG")]
        public static void LogDebug(string message, string source = null)
        {
            Log("DEBUG", message, source);
        }

        /// <summary>
        /// Logs a message to debug output and optionally to file
        /// </summary>
        private static void Log(string level, string message, string source)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var sourceInfo = string.IsNullOrEmpty(source) ? "" : $"[{source}] ";
                var logMessage = $"[{timestamp}] [{level}] {sourceInfo}{message}";

                // Always write to debug output
                Debug.WriteLine(logMessage);

                // Optionally write to file
                if (_logFilePath != null)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            lock (_lockObject)
                            {
                                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                            }
                        }
                        catch
                        {
                            // Silently ignore file logging errors
                        }
                    });
                }
            }
            catch
            {
                // Silently ignore logging errors to prevent cascading failures
            }
        }

        /// <summary>
        /// Clears the log file
        /// </summary>
        public static void ClearLog()
        {
            try
            {
                if (_logFilePath != null && File.Exists(_logFilePath))
                {
                    lock (_lockObject)
                    {
                        File.Delete(_logFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to clear log: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the log file path
        /// </summary>
        public static string GetLogFilePath() => _logFilePath;
    }
}
