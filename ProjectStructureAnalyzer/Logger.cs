using System;
using System.IO;

namespace ProjectStructureAnalyzer
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");

        public static void ClearLogFile()
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    File.WriteAllText(LogFilePath, string.Empty);
                }
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки очистки файла логов
                Console.WriteLine($"Error clearing log file: {ex.Message}");
            }
        }

        public static void LogInfo(string message)
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO] {message}\n";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Игнорируем ошибки записи лога
            }
        }

        public static void LogError(string message, Exception ex)
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR] {message}\n{ex.ToString()}\n\n";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Игнорируем ошибки записи лога
            }
        }
    }
}