using System;
using System.IO;

namespace ProjectStructureAnalyzer
{
    public static class Logger
    {
        private static string logFilePath = "error_log.txt";

        public static void SetLogFilePath(string path)
        {
            logFilePath = path;
        }

        public static void LogError(string message, Exception ex)
        {
            try
            {
                string logMessage = $"{DateTime.Now}: ERROR - {message}\n{ex?.ToString()}\n";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }

        public static void LogInfo(string message)
        {
            try
            {
                string logMessage = $"{DateTime.Now}: INFO - {message}\n";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }

        public static void ClearLogFile()
        {
            try
            {
                File.WriteAllText(logFilePath, string.Empty);
            }
            catch
            {
                // Игнорируем ошибки
            }
        }
    }
}