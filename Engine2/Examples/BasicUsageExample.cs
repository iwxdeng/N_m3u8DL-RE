using System;
using System.Threading.Tasks;
using Engine2.Core;
using Engine2.Interfaces;
using Engine2.Utils;

namespace Engine2.Examples
{
    /// <summary>
    /// 基本使用示例
    /// </summary>
    public class BasicUsageExample
    {
        /// <summary>
        /// 简单下载示例
        /// </summary>
        public static async Task SimpleDownloadExample()
        {
            // 创建下载引擎
            var engine = new M3u8DownloadEngine();

            // 配置下载选项
            var options = new DownloadOptions
            {
                OutputDirectory = @"C:\Downloads",
                FileName = "my_video",
                ThreadCount = 8,
                RetryCount = 3,
                AutoSelectBest = true,
                DeleteTempFiles = true
            };

            // 进度回调
            var progress = new Progress<ProgressInfo>(info =>
            {
                Console.WriteLine($"Progress: {info.Percentage:F1}% - {info.StatusMessage}");
            });

            try
            {
                // 执行下载
                var result = await engine.DownloadAsync(
                    "https://example.com/video.m3u8", 
                    options, 
                    progress);

                if (result.IsSuccess)
                {
                    Console.WriteLine($"Download completed successfully!");
                    Console.WriteLine($"Output file: {result.OutputFilePath}");
                    Console.WriteLine($"File size: {result.FileSize / (1024 * 1024)} MB");
                    Console.WriteLine($"Duration: {result.Duration.TotalSeconds:F1} seconds");
                }
                else
                {
                    Console.WriteLine($"Download failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// 批量下载示例
        /// </summary>
        public static async Task BatchDownloadExample()
        {
            var engine = new M3u8DownloadEngine();

            var urls = new[]
            {
                "https://example.com/video1.m3u8",
                "https://example.com/video2.m3u8",
                "https://example.com/video3.m3u8"
            };

            var options = new DownloadOptions
            {
                OutputDirectory = @"C:\Downloads\Batch",
                ThreadCount = 4,
                AutoSelectBest = true
            };

            // 批量下载进度回调
            var batchProgress = new Progress<BatchProgressInfo>(info =>
            {
                Console.WriteLine($"Batch Progress: {info.OverallPercentage:F1}% " +
                                $"({info.CompletedTasks}/{info.TotalTasks} completed, " +
                                $"{info.FailedTasks} failed)");
                
                if (info.CurrentTaskProgress != null)
                {
                    Console.WriteLine($"  Current: {info.CurrentTaskUrl} - {info.CurrentTaskProgress.Percentage:F1}%");
                }
            });

            try
            {
                var results = await engine.DownloadBatchAsync(urls, options, batchProgress);

                Console.WriteLine("\nBatch download completed:");
                foreach (var result in results)
                {
                    if (result.IsSuccess)
                    {
                        Console.WriteLine($"✓ {result.Url} -> {result.OutputFilePath}");
                    }
                    else
                    {
                        Console.WriteLine($"✗ {result.Url} -> Failed: {result.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Batch download error: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取流信息示例
        /// </summary>
        public static async Task GetStreamInfoExample()
        {
            var engine = new M3u8DownloadEngine();

            try
            {
                var streamInfo = await engine.GetStreamInfoAsync("https://example.com/video.m3u8");

                Console.WriteLine($"Stream Type: {streamInfo.Type}");
                Console.WriteLine($"Is Live: {streamInfo.IsLive}");
                Console.WriteLine($"Duration: {streamInfo.Duration}");
                Console.WriteLine($"Video Streams: {streamInfo.VideoStreams.Count}");
                Console.WriteLine($"Audio Streams: {streamInfo.AudioStreams.Count}");
                Console.WriteLine($"Subtitle Streams: {streamInfo.SubtitleStreams.Count}");

                // 显示视频流详情
                foreach (var video in streamInfo.VideoStreams)
                {
                    Console.WriteLine($"  Video: {video.Width}x{video.Height} {video.Codec} {video.Bandwidth}bps");
                }

                // 显示音频流详情
                foreach (var audio in streamInfo.AudioStreams)
                {
                    Console.WriteLine($"  Audio: {audio.Codec} {audio.SampleRate}Hz {audio.Channels}ch {audio.Language}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get stream info: {ex.Message}");
            }
        }

        /// <summary>
        /// 高级配置示例
        /// </summary>
        public static async Task AdvancedConfigurationExample()
        {
            // 使用自定义日志
            var logger = new CompositeLogger(
                new ConsoleLogger(),
                new FileLogger(@"C:\Logs\download.log")
            );

            // 指定N_m3u8DL-RE路径
            var engine = new M3u8DownloadEngine(@"C:\Tools\N_m3u8DL-RE.exe", logger);

            var options = new DownloadOptions
            {
                OutputDirectory = @"C:\Downloads",
                FileName = "advanced_video",
                ThreadCount = 16,
                RetryCount = 5,
                HttpTimeoutSeconds = 120,
                MaxSpeed = 10 * 1024 * 1024, // 10 MB/s
                
                // 自定义请求头
                Headers = 
                {
                    ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                    ["Referer"] = "https://example.com",
                    ["Authorization"] = "Bearer your-token-here"
                },

                // 代理设置
                ProxyUrl = "http://127.0.0.1:8080",
                UseSystemProxy = false,

                // FFmpeg路径
                FFmpegPath = @"C:\Tools\ffmpeg.exe",

                // 解密密钥
                DecryptionKeys = new[] { "your-decryption-key" },

                // 其他选项
                ConcurrentDownload = true,
                SkipMerge = false,
                DeleteTempFiles = true,
                WriteMetaJson = true,
                LogLevel = LogLevel.Debug
            };

            var progress = new Progress<ProgressInfo>(info =>
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {info.Percentage:F1}% " +
                                $"({info.CompletedSegments}/{info.TotalSegments} segments) " +
                                $"{info.Speed / (1024 * 1024):F1} MB/s " +
                                $"ETA: {info.EstimatedTimeRemaining?.ToString(@"mm\:ss")}");
            });

            try
            {
                var result = await engine.DownloadAsync(
                    "https://example.com/protected-video.m3u8",
                    options,
                    progress);

                if (result.IsSuccess)
                {
                    Console.WriteLine("Advanced download completed successfully!");
                }
                else
                {
                    Console.WriteLine($"Advanced download failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Advanced download error: {ex.Message}");
            }
        }
    }
}
