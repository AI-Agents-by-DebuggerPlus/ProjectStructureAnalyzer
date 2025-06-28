using System;
using System.IO;

namespace ProjectStructureAnalyzer
{
    public static class Logger
    {
#if DEBUG
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");
#endif

        public static void ClearLogFile()
        {
#if DEBUG
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
#endif
        }

        public static void LogInfo(string message)
        {
#if DEBUG
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [INFO] {message}\n";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Игнорируем ошибки записи лога
            }
#endif
        }

        public static void LogError(string message, Exception ex)
        {
#if DEBUG
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [ERROR] {message}\n{ex.ToString()}\n\n";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Игнорируем ошибки записи лога
            }
#endif
        }
    }
}