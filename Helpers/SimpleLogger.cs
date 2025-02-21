using System;
using System.Diagnostics;
using System.IO;

namespace StudentManagementApp.Helpers
{
    public static class SimpleLogger
    {
        private static readonly object _lock = new object();
        // The log file is stored in the local application data folder.
        private static readonly string logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "app_log.txt");

        public static void Log(string message)
        {
            // Create a log entry with timestamp.
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            // Write to the Debug output.
            Debug.WriteLine(logEntry);
            // Append to the log file in a thread-safe way.
            lock (_lock)
            {
                try
                {
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error writing log: {ex.Message}");
                }
            }
        }

        public static void LogInfo(string message)
        {
            Log("INFO: " + message);
        }

        public static void LogWarning(string message)
        {
            Log("WARNING: " + message);
        }

        public static void LogError(string message)
        {
            Log("ERROR: " + message);
        }
    }
}
