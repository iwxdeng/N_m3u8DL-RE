using System;
using System.IO;

namespace Engine2.Utils
{
    /// <summary>
    /// 日志接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录调试信息
        /// </summary>
        void LogDebug(string message);

        /// <summary>
        /// 记录信息
        /// </summary>
        void LogInfo(string message);

        /// <summary>
        /// 记录警告
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// 记录错误
        /// </summary>
        void LogError(string message);

        /// <summary>
        /// 记录错误和异常
        /// </summary>
        void LogError(string message, Exception exception);
    }

    /// <summary>
    /// 控制台日志实现
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public void LogDebug(string message)
        {
            Console.WriteLine($"[DEBUG] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }

        public void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }

        public void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
        }

        public void LogError(string message, Exception exception)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
            Console.WriteLine($"Exception: {exception}");
        }
    }

    /// <summary>
    /// 文件日志实现
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();

        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
            
            // 确保日志目录存在
            var directory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void LogDebug(string message)
        {
            WriteLog("DEBUG", message);
        }

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        public void LogError(string message, Exception exception)
        {
            WriteLog("ERROR", $"{message}\nException: {exception}");
        }

        private void WriteLog(string level, string message)
        {
            lock (_lockObject)
            {
                try
                {
                    var logEntry = $"[{level}] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, logEntry);
                }
                catch (Exception ex)
                {
                    // 如果写入文件失败，回退到控制台输出
                    Console.WriteLine($"[{level}] {DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
                    Console.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// 组合日志实现（同时输出到控制台和文件）
    /// </summary>
    public class CompositeLogger : ILogger
    {
        private readonly ILogger[] _loggers;

        public CompositeLogger(params ILogger[] loggers)
        {
            _loggers = loggers ?? throw new ArgumentNullException(nameof(loggers));
        }

        public void LogDebug(string message)
        {
            foreach (var logger in _loggers)
            {
                logger.LogDebug(message);
            }
        }

        public void LogInfo(string message)
        {
            foreach (var logger in _loggers)
            {
                logger.LogInfo(message);
            }
        }

        public void LogWarning(string message)
        {
            foreach (var logger in _loggers)
            {
                logger.LogWarning(message);
            }
        }

        public void LogError(string message)
        {
            foreach (var logger in _loggers)
            {
                logger.LogError(message);
            }
        }

        public void LogError(string message, Exception exception)
        {
            foreach (var logger in _loggers)
            {
                logger.LogError(message, exception);
            }
        }
    }

    /// <summary>
    /// 空日志实现（不输出任何日志）
    /// </summary>
    public class NullLogger : ILogger
    {
        public void LogDebug(string message) { }
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message) { }
        public void LogError(string message, Exception exception) { }
    }
}
