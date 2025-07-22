using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Engine2.Interfaces
{
    /// <summary>
    /// 下载引擎接口
    /// </summary>
    public interface IDownloadEngine
    {
        /// <summary>
        /// 下载单个URL
        /// </summary>
        /// <param name="url">下载链接</param>
        /// <param name="options">下载选项</param>
        /// <param name="progressCallback">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>下载结果</returns>
        Task<DownloadResult> DownloadAsync(string url, DownloadOptions? options = null, 
            IProgress<ProgressInfo>? progressCallback = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 批量下载
        /// </summary>
        /// <param name="urls">下载链接列表</param>
        /// <param name="options">下载选项</param>
        /// <param name="progressCallback">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>下载结果列表</returns>
        Task<List<DownloadResult>> DownloadBatchAsync(IEnumerable<string> urls, DownloadOptions? options = null,
            IProgress<BatchProgressInfo>? progressCallback = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取流信息（不下载）
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="options">选项</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>流信息</returns>
        Task<StreamInfo> GetStreamInfoAsync(string url, DownloadOptions? options = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 验证URL是否支持
        /// </summary>
        /// <param name="url">链接</param>
        /// <returns>是否支持</returns>
        bool IsUrlSupported(string url);
    }

    /// <summary>
    /// 下载选项
    /// </summary>
    public class DownloadOptions
    {
        /// <summary>
        /// 输出目录
        /// </summary>
        public string? OutputDirectory { get; set; }

        /// <summary>
        /// 文件名（不包含扩展名）
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// 临时目录
        /// </summary>
        public string? TempDirectory { get; set; }

        /// <summary>
        /// 线程数
        /// </summary>
        public int ThreadCount { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// 重试次数
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// HTTP请求超时时间（秒）
        /// </summary>
        public int HttpTimeoutSeconds { get; set; } = 100;

        /// <summary>
        /// 自定义请求头
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new();

        /// <summary>
        /// 最大下载速度（字节/秒），null表示不限速
        /// </summary>
        public long? MaxSpeed { get; set; }

        /// <summary>
        /// 是否自动选择最佳流
        /// </summary>
        public bool AutoSelectBest { get; set; } = true;

        /// <summary>
        /// 是否跳过合并
        /// </summary>
        public bool SkipMerge { get; set; } = false;

        /// <summary>
        /// 是否删除临时文件
        /// </summary>
        public bool DeleteTempFiles { get; set; } = true;

        /// <summary>
        /// 是否并发下载
        /// </summary>
        public bool ConcurrentDownload { get; set; } = false;

        /// <summary>
        /// FFmpeg路径
        /// </summary>
        public string? FFmpegPath { get; set; }

        /// <summary>
        /// 代理设置
        /// </summary>
        public string? ProxyUrl { get; set; }

        /// <summary>
        /// 是否使用系统代理
        /// </summary>
        public bool UseSystemProxy { get; set; } = true;

        /// <summary>
        /// 解密密钥
        /// </summary>
        public string[]? DecryptionKeys { get; set; }

        /// <summary>
        /// 基础URL
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Info;

        /// <summary>
        /// 是否写入元数据JSON
        /// </summary>
        public bool WriteMetaJson { get; set; } = true;
    }

    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Off
    }

    /// <summary>
    /// 下载结果
    /// </summary>
    public class DownloadResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 输出文件路径
        /// </summary>
        public string? OutputFilePath { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// 下载的URL
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 下载耗时
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// 流信息
        /// </summary>
        public StreamInfo? StreamInfo { get; set; }
    }

    /// <summary>
    /// 进度信息
    /// </summary>
    public class ProgressInfo
    {
        /// <summary>
        /// 当前进度百分比 (0-100)
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// 已下载字节数
        /// </summary>
        public long DownloadedBytes { get; set; }

        /// <summary>
        /// 总字节数
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// 下载速度（字节/秒）
        /// </summary>
        public long Speed { get; set; }

        /// <summary>
        /// 剩余时间
        /// </summary>
        public TimeSpan? EstimatedTimeRemaining { get; set; }

        /// <summary>
        /// 当前状态描述
        /// </summary>
        public string? StatusMessage { get; set; }

        /// <summary>
        /// 已完成的分片数
        /// </summary>
        public int CompletedSegments { get; set; }

        /// <summary>
        /// 总分片数
        /// </summary>
        public int TotalSegments { get; set; }
    }

    /// <summary>
    /// 批量下载进度信息
    /// </summary>
    public class BatchProgressInfo
    {
        /// <summary>
        /// 总体进度百分比 (0-100)
        /// </summary>
        public double OverallPercentage { get; set; }

        /// <summary>
        /// 已完成的任务数
        /// </summary>
        public int CompletedTasks { get; set; }

        /// <summary>
        /// 总任务数
        /// </summary>
        public int TotalTasks { get; set; }

        /// <summary>
        /// 当前任务进度
        /// </summary>
        public ProgressInfo? CurrentTaskProgress { get; set; }

        /// <summary>
        /// 当前任务URL
        /// </summary>
        public string? CurrentTaskUrl { get; set; }

        /// <summary>
        /// 失败的任务数
        /// </summary>
        public int FailedTasks { get; set; }
    }

    /// <summary>
    /// 流信息
    /// </summary>
    public class StreamInfo
    {
        /// <summary>
        /// 流类型
        /// </summary>
        public StreamType Type { get; set; }

        /// <summary>
        /// 是否为直播流
        /// </summary>
        public bool IsLive { get; set; }

        /// <summary>
        /// 时长
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// 可用的视频流
        /// </summary>
        public List<VideoStreamSpec> VideoStreams { get; set; } = new();

        /// <summary>
        /// 可用的音频流
        /// </summary>
        public List<AudioStreamSpec> AudioStreams { get; set; } = new();

        /// <summary>
        /// 可用的字幕流
        /// </summary>
        public List<SubtitleStreamSpec> SubtitleStreams { get; set; } = new();

        /// <summary>
        /// 原始URL
        /// </summary>
        public string? OriginalUrl { get; set; }
    }

    /// <summary>
    /// 流类型
    /// </summary>
    public enum StreamType
    {
        HLS,
        DASH,
        MSS,
        HTTP_LIVE,
        Unknown
    }

    /// <summary>
    /// 视频流规格
    /// </summary>
    public class VideoStreamSpec
    {
        public string? Id { get; set; }
        public string? Codec { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Bandwidth { get; set; }
        public double FrameRate { get; set; }
        public string? Language { get; set; }
        public string? Url { get; set; }
    }

    /// <summary>
    /// 音频流规格
    /// </summary>
    public class AudioStreamSpec
    {
        public string? Id { get; set; }
        public string? Codec { get; set; }
        public int Bandwidth { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public string? Language { get; set; }
        public string? Url { get; set; }
    }

    /// <summary>
    /// 字幕流规格
    /// </summary>
    public class SubtitleStreamSpec
    {
        public string? Id { get; set; }
        public string? Language { get; set; }
        public string? Name { get; set; }
        public string? Format { get; set; }
        public string? Url { get; set; }
    }
}
