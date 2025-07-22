using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Engine2.Interfaces;
using Engine2.Utils;

namespace Engine2.Core
{
    /// <summary>
    /// M3U8下载引擎实现
    /// </summary>
    public class M3u8DownloadEngine : IDownloadEngine
    {
        private readonly string _n_m3u8dl_re_path;
        private readonly ILogger _logger;

        public M3u8DownloadEngine(string? n_m3u8dl_re_path = null, ILogger? logger = null)
        {
            _n_m3u8dl_re_path = n_m3u8dl_re_path ?? FindN_m3u8DL_RE();
            _logger = logger ?? new ConsoleLogger();

            if (string.IsNullOrEmpty(_n_m3u8dl_re_path) || !File.Exists(_n_m3u8dl_re_path))
            {
                throw new FileNotFoundException("N_m3u8DL-RE executable not found. Please specify the path or ensure it's in PATH.");
            }
        }

        /// <summary>
        /// 下载单个URL
        /// </summary>
        public async Task<DownloadResult> DownloadAsync(string url, DownloadOptions? options = null,
            IProgress<ProgressInfo>? progressCallback = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));

            options ??= new DownloadOptions();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInfo($"Starting download: {url}");

                // 构建命令行参数
                var arguments = BuildArguments(url, options);
                
                // 执行下载
                var result = await ExecuteDownloadAsync(arguments, options, progressCallback, cancellationToken);
                
                stopwatch.Stop();
                result.Duration = stopwatch.Elapsed;
                result.Url = url;

                _logger.LogInfo($"Download completed in {result.Duration.TotalSeconds:F2} seconds");
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError($"Download failed: {ex.Message}");
                
                return new DownloadResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    Exception = ex,
                    Url = url,
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// 批量下载
        /// </summary>
        public async Task<List<DownloadResult>> DownloadBatchAsync(IEnumerable<string> urls, DownloadOptions? options = null,
            IProgress<BatchProgressInfo>? progressCallback = null, CancellationToken cancellationToken = default)
        {
            var urlList = urls.ToList();
            if (!urlList.Any())
                return new List<DownloadResult>();

            options ??= new DownloadOptions();
            var results = new List<DownloadResult>();
            var completedTasks = 0;
            var failedTasks = 0;

            _logger.LogInfo($"Starting batch download of {urlList.Count} URLs");

            foreach (var url in urlList)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    // 为每个URL创建单独的进度回调
                    IProgress<ProgressInfo>? singleProgress = null;
                    if (progressCallback != null)
                    {
                        singleProgress = new Progress<ProgressInfo>(progress =>
                        {
                            var batchProgress = new BatchProgressInfo
                            {
                                OverallPercentage = (completedTasks * 100.0 + progress.Percentage) / urlList.Count,
                                CompletedTasks = completedTasks,
                                TotalTasks = urlList.Count,
                                CurrentTaskProgress = progress,
                                CurrentTaskUrl = url,
                                FailedTasks = failedTasks
                            };
                            progressCallback.Report(batchProgress);
                        });
                    }

                    var result = await DownloadAsync(url, options, singleProgress, cancellationToken);
                    results.Add(result);

                    if (result.IsSuccess)
                        completedTasks++;
                    else
                        failedTasks++;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to download {url}: {ex.Message}");
                    results.Add(new DownloadResult
                    {
                        IsSuccess = false,
                        ErrorMessage = ex.Message,
                        Exception = ex,
                        Url = url
                    });
                    failedTasks++;
                }

                // 报告最终进度
                progressCallback?.Report(new BatchProgressInfo
                {
                    OverallPercentage = (completedTasks + failedTasks) * 100.0 / urlList.Count,
                    CompletedTasks = completedTasks,
                    TotalTasks = urlList.Count,
                    FailedTasks = failedTasks
                });
            }

            _logger.LogInfo($"Batch download completed. Success: {completedTasks}, Failed: {failedTasks}");
            return results;
        }

        /// <summary>
        /// 获取流信息
        /// </summary>
        public async Task<StreamInfo> GetStreamInfoAsync(string url, DownloadOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));

            options ??= new DownloadOptions();

            try
            {
                _logger.LogInfo($"Getting stream info: {url}");

                // 使用 --skip-download 参数只获取信息不下载
                var tempOptions = new DownloadOptions
                {
                    OutputDirectory = options.OutputDirectory,
                    TempDirectory = options.TempDirectory,
                    Headers = options.Headers,
                    BaseUrl = options.BaseUrl,
                    LogLevel = options.LogLevel
                };

                var arguments = BuildArguments(url, tempOptions, skipDownload: true);
                
                // 执行命令获取流信息
                var processResult = await ExecuteProcessAsync(arguments, cancellationToken);
                
                // 解析输出获取流信息
                var streamInfo = ParseStreamInfo(processResult.Output, url);
                
                _logger.LogInfo($"Stream info retrieved successfully");
                return streamInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get stream info: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 验证URL是否支持
        /// </summary>
        public bool IsUrlSupported(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            // 检查URL格式
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            // 检查协议
            if (uri.Scheme != "http" && uri.Scheme != "https" && uri.Scheme != "file")
                return false;

            // 检查文件扩展名或内容类型
            var path = uri.AbsolutePath.ToLowerInvariant();
            return path.Contains(".m3u8") || 
                   path.Contains(".mpd") || 
                   path.Contains(".ism") ||
                   path.Contains("manifest") ||
                   url.ToLowerInvariant().Contains("m3u8") ||
                   url.ToLowerInvariant().Contains("mpd");
        }

        /// <summary>
        /// 构建命令行参数
        /// </summary>
        private string BuildArguments(string url, DownloadOptions options, bool skipDownload = false)
        {
            var args = new List<string> { $"\"{url}\"" };

            // 基本参数
            if (!string.IsNullOrEmpty(options.OutputDirectory))
                args.Add($"--save-dir \"{options.OutputDirectory}\"");

            if (!string.IsNullOrEmpty(options.FileName))
                args.Add($"--save-name \"{options.FileName}\"");

            if (!string.IsNullOrEmpty(options.TempDirectory))
                args.Add($"--tmp-dir \"{options.TempDirectory}\"");

            if (!string.IsNullOrEmpty(options.BaseUrl))
                args.Add($"--base-url \"{options.BaseUrl}\"");

            // 线程和重试设置
            args.Add($"--thread-count {options.ThreadCount}");
            args.Add($"--download-retry-count {options.RetryCount}");
            args.Add($"--http-request-timeout {options.HttpTimeoutSeconds}");

            // 速度限制
            if (options.MaxSpeed.HasValue)
            {
                var speedMbps = options.MaxSpeed.Value / (1024.0 * 1024.0);
                args.Add($"--max-speed {speedMbps:F2}M");
            }

            // 请求头
            foreach (var header in options.Headers)
            {
                args.Add($"--header \"{header.Key}: {header.Value}\"");
            }

            // 代理设置
            if (!string.IsNullOrEmpty(options.ProxyUrl))
                args.Add($"--custom-proxy \"{options.ProxyUrl}\"");

            if (!options.UseSystemProxy)
                args.Add("--use-system-proxy false");

            // FFmpeg路径
            if (!string.IsNullOrEmpty(options.FFmpegPath))
                args.Add($"--ffmpeg-binary-path \"{options.FFmpegPath}\"");

            // 解密密钥
            if (options.DecryptionKeys != null)
            {
                foreach (var key in options.DecryptionKeys)
                {
                    args.Add($"--key \"{key}\"");
                }
            }

            // 其他选项
            if (options.AutoSelectBest)
                args.Add("--auto-select");

            if (options.SkipMerge)
                args.Add("--skip-merge");

            if (!options.DeleteTempFiles)
                args.Add("--del-after-done false");

            if (options.ConcurrentDownload)
                args.Add("--concurrent-download");

            if (!options.WriteMetaJson)
                args.Add("--write-meta-json false");

            if (skipDownload)
                args.Add("--skip-download");

            // 日志级别
            args.Add($"--log-level {options.LogLevel.ToString().ToUpperInvariant()}");

            // 禁用ANSI颜色以便解析输出
            args.Add("--no-ansi-color");

            return string.Join(" ", args);
        }

        /// <summary>
        /// 执行下载
        /// </summary>
        private async Task<DownloadResult> ExecuteDownloadAsync(string arguments, DownloadOptions options,
            IProgress<ProgressInfo>? progressCallback, CancellationToken cancellationToken)
        {
            var processResult = await ExecuteProcessAsync(arguments, cancellationToken, progressCallback);

            if (processResult.ExitCode == 0)
            {
                // 查找输出文件
                var outputFile = FindOutputFile(options);
                var fileSize = outputFile != null && File.Exists(outputFile) ? new FileInfo(outputFile).Length : 0;

                return new DownloadResult
                {
                    IsSuccess = true,
                    OutputFilePath = outputFile,
                    FileSize = fileSize
                };
            }
            else
            {
                return new DownloadResult
                {
                    IsSuccess = false,
                    ErrorMessage = processResult.Error
                };
            }
        }

        /// <summary>
        /// 执行进程
        /// </summary>
        private async Task<ProcessResult> ExecuteProcessAsync(string arguments, CancellationToken cancellationToken,
            IProgress<ProgressInfo>? progressCallback = null)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = _n_m3u8dl_re_path,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var output = new List<string>();
            var error = new List<string>();

            using var process = new Process { StartInfo = processInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    output.Add(e.Data);
                    ParseProgressFromOutput(e.Data, progressCallback);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    error.Add(e.Data);
                    _logger.LogError(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            return new ProcessResult
            {
                ExitCode = process.ExitCode,
                Output = string.Join(Environment.NewLine, output),
                Error = string.Join(Environment.NewLine, error)
            };
        }

        /// <summary>
        /// 从输出解析进度
        /// </summary>
        private void ParseProgressFromOutput(string output, IProgress<ProgressInfo>? progressCallback)
        {
            if (progressCallback == null || string.IsNullOrEmpty(output))
                return;

            try
            {
                // 解析进度信息（根据N_m3u8DL-RE的输出格式）
                // 这里需要根据实际输出格式进行调整
                if (output.Contains("%") && output.Contains("/"))
                {
                    // 示例: "50% (100/200) 1.5MB/s"
                    var progressInfo = new ProgressInfo
                    {
                        StatusMessage = output.Trim()
                    };

                    progressCallback.Report(progressInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to parse progress: {ex.Message}");
            }
        }

        /// <summary>
        /// 查找输出文件
        /// </summary>
        private string? FindOutputFile(DownloadOptions options)
        {
            var outputDir = options.OutputDirectory ?? Environment.CurrentDirectory;

            if (!Directory.Exists(outputDir))
                return null;

            // 查找最新创建的视频文件
            var videoExtensions = new[] { ".mp4", ".mkv", ".ts", ".m4v", ".avi" };
            var files = Directory.GetFiles(outputDir)
                .Where(f => videoExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();

            return files.FirstOrDefault();
        }

        /// <summary>
        /// 解析流信息
        /// </summary>
        private StreamInfo ParseStreamInfo(string output, string url)
        {
            var streamInfo = new StreamInfo
            {
                OriginalUrl = url,
                Type = DetermineStreamType(url)
            };

            // 这里需要根据N_m3u8DL-RE的实际输出格式来解析流信息
            // 由于输出格式可能比较复杂，这里提供一个基础实现

            return streamInfo;
        }

        /// <summary>
        /// 确定流类型
        /// </summary>
        private StreamType DetermineStreamType(string url)
        {
            var lowerUrl = url.ToLowerInvariant();

            if (lowerUrl.Contains(".m3u8"))
                return StreamType.HLS;
            else if (lowerUrl.Contains(".mpd"))
                return StreamType.DASH;
            else if (lowerUrl.Contains(".ism"))
                return StreamType.MSS;
            else
                return StreamType.Unknown;
        }

        /// <summary>
        /// 查找N_m3u8DL-RE可执行文件
        /// </summary>
        private string FindN_m3u8DL_RE()
        {
            // 常见的可执行文件名
            var executableNames = new[]
            {
                "N_m3u8DL-RE.exe",
                "N_m3u8DL-RE",
                "n_m3u8dl-re.exe",
                "n_m3u8dl-re"
            };

            // 首先在当前目录查找
            foreach (var name in executableNames)
            {
                if (File.Exists(name))
                    return Path.GetFullPath(name);
            }

            // 在PATH环境变量中查找
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                var paths = pathEnv.Split(Path.PathSeparator);
                foreach (var path in paths)
                {
                    foreach (var name in executableNames)
                    {
                        var fullPath = Path.Combine(path, name);
                        if (File.Exists(fullPath))
                            return fullPath;
                    }
                }
            }

            return string.Empty;
        }
    }

    /// <summary>
    /// 进程执行结果
    /// </summary>
    internal class ProcessResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
