using System;
using System.IO;
using System.Threading.Tasks;
using Engine2.Core;
using Engine2.Interfaces;
using Engine2.Utils;

namespace Engine2.Examples
{
    /// <summary>
    /// 中文使用示例 - 展示Engine2的各种功能
    /// </summary>
    public class ChineseUsageExample
    {
        /// <summary>
        /// 🎯 简单下载示例
        /// </summary>
        public static async Task 简单下载示例()
        {
            Console.WriteLine("🚀 开始简单下载示例...\n");

            // 创建下载引擎
            var engine = new M3u8DownloadEngine();

            // 配置下载选项
            var options = new DownloadOptions
            {
                OutputDirectory = @"D:\视频下载",        // 输出目录
                FileName = "测试视频",                   // 文件名
                ThreadCount = 8,                        // 8个下载线程
                RetryCount = 5,                         // 失败重试5次
                AutoSelectBest = true,                  // 自动选择最佳质量
                DeleteTempFiles = true,                 // 完成后删除临时文件
                WriteMetaJson = true                    // 输出流信息JSON
            };

            // 进度监控回调
            var progress = new Progress<ProgressInfo>(info =>
            {
                Console.WriteLine($"📈 下载进度: {info.Percentage:F1}% " +
                                $"({info.CompletedSegments}/{info.TotalSegments} 分片) " +
                                $"🚀 速度: {info.Speed / (1024 * 1024):F1} MB/s");
                
                if (info.EstimatedTimeRemaining.HasValue)
                {
                    Console.WriteLine($"⏰ 预计剩余时间: {info.EstimatedTimeRemaining.Value:mm\\:ss}");
                }
            });

            try
            {
                // 执行下载
                var result = await engine.DownloadAsync(
                    "https://example.com/video.m3u8", 
                    options, 
                    progress);

                // 检查结果
                if (result.IsSuccess)
                {
                    Console.WriteLine("\n✅ 下载成功完成！");
                    Console.WriteLine($"📁 文件路径: {result.OutputFilePath}");
                    Console.WriteLine($"📊 文件大小: {result.FileSize / (1024 * 1024):F1} MB");
                    Console.WriteLine($"⏱️ 下载耗时: {result.Duration.TotalMinutes:F1} 分钟");
                }
                else
                {
                    Console.WriteLine($"\n❌ 下载失败: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n💥 发生异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 📦 批量下载示例
        /// </summary>
        public static async Task 批量下载示例()
        {
            Console.WriteLine("📦 开始批量下载示例...\n");

            var engine = new M3u8DownloadEngine();

            // 准备下载列表
            var 下载列表 = new[]
            {
                "https://example.com/电影1.m3u8",
                "https://example.com/电影2.m3u8",
                "https://example.com/电影3.m3u8",
                "https://example.com/纪录片1.m3u8",
                "https://example.com/纪录片2.m3u8"
            };

            var options = new DownloadOptions
            {
                OutputDirectory = @"D:\批量下载",
                ThreadCount = 6,
                RetryCount = 3,
                AutoSelectBest = true,
                ConcurrentDownload = false,             // 顺序下载，避免服务器压力过大
                MaxSpeed = 5 * 1024 * 1024             // 限速5MB/s
            };

            // 批量下载进度监控
            var batchProgress = new Progress<BatchProgressInfo>(info =>
            {
                Console.WriteLine($"📊 总体进度: {info.OverallPercentage:F1}% " +
                                $"✅ 已完成: {info.CompletedTasks}/{info.TotalTasks} " +
                                $"❌ 失败: {info.FailedTasks}");

                if (info.CurrentTaskProgress != null && !string.IsNullOrEmpty(info.CurrentTaskUrl))
                {
                    var 当前文件名 = Path.GetFileName(info.CurrentTaskUrl);
                    Console.WriteLine($"  🎯 当前任务: {当前文件名} - {info.CurrentTaskProgress.Percentage:F1}%");
                }
                Console.WriteLine();
            });

            try
            {
                // 执行批量下载
                var results = await engine.DownloadBatchAsync(下载列表, options, batchProgress);

                // 统计结果
                var 成功数量 = 0;
                var 失败数量 = 0;

                Console.WriteLine("📋 批量下载完成，详细结果:");
                Console.WriteLine(new string('=', 50));

                foreach (var result in results)
                {
                    var 文件名 = Path.GetFileName(result.Url);
                    if (result.IsSuccess)
                    {
                        成功数量++;
                        Console.WriteLine($"✅ {文件名}");
                        Console.WriteLine($"   📁 保存位置: {result.OutputFilePath}");
                        Console.WriteLine($"   📊 文件大小: {result.FileSize / (1024 * 1024):F1} MB");
                    }
                    else
                    {
                        失败数量++;
                        Console.WriteLine($"❌ {文件名}");
                        Console.WriteLine($"   💥 失败原因: {result.ErrorMessage}");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine($"📈 批量下载统计:");
                Console.WriteLine($"✅ 成功: {成功数量} 个");
                Console.WriteLine($"❌ 失败: {失败数量} 个");
                Console.WriteLine($"📊 成功率: {(double)成功数量 / results.Count * 100:F1}%");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 批量下载异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 🔍 流信息获取示例
        /// </summary>
        public static async Task 流信息获取示例()
        {
            Console.WriteLine("🔍 开始流信息获取示例...\n");

            var engine = new M3u8DownloadEngine();

            try
            {
                // 获取流信息（不下载）
                var streamInfo = await engine.GetStreamInfoAsync("https://example.com/video.m3u8");

                Console.WriteLine("📺 流媒体信息详情:");
                Console.WriteLine(new string('=', 40));
                Console.WriteLine($"🎬 流类型: {streamInfo.Type}");
                Console.WriteLine($"🔴 是否直播: {(streamInfo.IsLive ? "是" : "否")}");
                Console.WriteLine($"⏱️ 视频时长: {streamInfo.Duration?.ToString(@"hh\:mm\:ss") ?? "未知"}");
                Console.WriteLine($"🔗 原始URL: {streamInfo.OriginalUrl}");

                // 视频流信息
                if (streamInfo.VideoStreams.Count > 0)
                {
                    Console.WriteLine($"\n🎬 视频流信息 (共 {streamInfo.VideoStreams.Count} 个):");
                    Console.WriteLine(new string('-', 30));
                    
                    for (int i = 0; i < streamInfo.VideoStreams.Count; i++)
                    {
                        var video = streamInfo.VideoStreams[i];
                        Console.WriteLine($"视频流 #{i + 1}:");
                        Console.WriteLine($"  📐 分辨率: {video.Width}x{video.Height}");
                        Console.WriteLine($"  🎞️ 编码: {video.Codec ?? "未知"}");
                        Console.WriteLine($"  📊 码率: {video.Bandwidth / 1000:F0} Kbps");
                        Console.WriteLine($"  🎯 帧率: {video.FrameRate:F1} fps");
                        Console.WriteLine($"  🌍 语言: {video.Language ?? "未指定"}");
                        Console.WriteLine();
                    }
                }

                // 音频流信息
                if (streamInfo.AudioStreams.Count > 0)
                {
                    Console.WriteLine($"🎵 音频流信息 (共 {streamInfo.AudioStreams.Count} 个):");
                    Console.WriteLine(new string('-', 30));
                    
                    for (int i = 0; i < streamInfo.AudioStreams.Count; i++)
                    {
                        var audio = streamInfo.AudioStreams[i];
                        Console.WriteLine($"音频流 #{i + 1}:");
                        Console.WriteLine($"  🎼 编码: {audio.Codec ?? "未知"}");
                        Console.WriteLine($"  📊 码率: {audio.Bandwidth / 1000:F0} Kbps");
                        Console.WriteLine($"  🔊 采样率: {audio.SampleRate} Hz");
                        Console.WriteLine($"  📢 声道: {audio.Channels}");
                        Console.WriteLine($"  🌍 语言: {audio.Language ?? "未指定"}");
                        Console.WriteLine();
                    }
                }

                // 字幕流信息
                if (streamInfo.SubtitleStreams.Count > 0)
                {
                    Console.WriteLine($"📝 字幕流信息 (共 {streamInfo.SubtitleStreams.Count} 个):");
                    Console.WriteLine(new string('-', 30));
                    
                    for (int i = 0; i < streamInfo.SubtitleStreams.Count; i++)
                    {
                        var subtitle = streamInfo.SubtitleStreams[i];
                        Console.WriteLine($"字幕流 #{i + 1}:");
                        Console.WriteLine($"  📝 格式: {subtitle.Format ?? "未知"}");
                        Console.WriteLine($"  🌍 语言: {subtitle.Language ?? "未指定"}");
                        Console.WriteLine($"  📋 名称: {subtitle.Name ?? "未命名"}");
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 获取流信息失败: {ex.Message}");
            }
        }

        /// <summary>
        /// ⚙️ 高级配置示例
        /// </summary>
        public static async Task 高级配置示例()
        {
            Console.WriteLine("⚙️ 开始高级配置示例...\n");

            // 创建组合日志（控制台 + 文件）
            var logger = new CompositeLogger(
                new ConsoleLogger(),
                new FileLogger(@"D:\日志\下载日志.log")
            );

            // 指定N_m3u8DL-RE路径并使用自定义日志
            var engine = new M3u8DownloadEngine(@"C:\Tools\N_m3u8DL-RE.exe", logger);

            var options = new DownloadOptions
            {
                OutputDirectory = @"D:\高级下载",
                FileName = "高级配置测试",
                ThreadCount = 16,                       // 16线程并发
                RetryCount = 10,                        // 重试10次
                HttpTimeoutSeconds = 180,               // 3分钟超时
                MaxSpeed = 20 * 1024 * 1024,           // 限速20MB/s

                // 🔐 自定义请求头
                Headers = 
                {
                    ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
                    ["Referer"] = "https://video-website.com",
                    ["Authorization"] = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                    ["Cookie"] = "session_id=abc123; user_preference=hd_quality",
                    ["X-Forwarded-For"] = "192.168.1.100"
                },

                // 🌐 网络配置
                ProxyUrl = "http://127.0.0.1:8080",    // HTTP代理
                UseSystemProxy = false,                 // 不使用系统代理
                BaseUrl = "https://cdn.example.com",    // 基础URL

                // 🔑 解密配置
                DecryptionKeys = new[] 
                { 
                    "1234567890abcdef1234567890abcdef",
                    "KID1:KEY1",
                    "abcdef1234567890abcdef1234567890"
                },

                // 🎞️ 工具路径
                FFmpegPath = @"C:\Tools\ffmpeg.exe",

                // ⚙️ 高级选项
                ConcurrentDownload = true,              // 并发下载音视频
                SkipMerge = false,                      // 不跳过合并
                DeleteTempFiles = true,                 // 删除临时文件
                WriteMetaJson = true,                   // 输出元数据
                LogLevel = LogLevel.Debug               // 调试级别日志
            };

            // 详细进度监控
            var progress = new Progress<ProgressInfo>(info =>
            {
                var 进度条 = 生成进度条(info.Percentage);
                Console.WriteLine($"📈 {进度条} {info.Percentage:F1}%");
                Console.WriteLine($"📊 分片进度: {info.CompletedSegments}/{info.TotalSegments}");
                Console.WriteLine($"🚀 下载速度: {info.Speed / (1024 * 1024):F2} MB/s");
                Console.WriteLine($"📥 已下载: {info.DownloadedBytes / (1024 * 1024):F1} MB");
                
                if (info.EstimatedTimeRemaining.HasValue)
                {
                    Console.WriteLine($"⏰ 剩余时间: {info.EstimatedTimeRemaining.Value:mm\\:ss}");
                }
                Console.WriteLine($"📝 状态: {info.StatusMessage}");
                Console.WriteLine(new string('-', 50));
            });

            try
            {
                var result = await engine.DownloadAsync(
                    "https://protected-site.com/premium-video.m3u8",
                    options,
                    progress);

                if (result.IsSuccess)
                {
                    Console.WriteLine("🎉 高级配置下载成功完成！");
                    Console.WriteLine($"📁 输出文件: {result.OutputFilePath}");
                    Console.WriteLine($"📊 文件大小: {result.FileSize / (1024 * 1024):F1} MB");
                    Console.WriteLine($"⏱️ 总耗时: {result.Duration.TotalMinutes:F1} 分钟");
                    Console.WriteLine($"🚀 平均速度: {result.FileSize / result.Duration.TotalSeconds / (1024 * 1024):F2} MB/s");
                }
                else
                {
                    Console.WriteLine($"❌ 高级配置下载失败: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 高级配置下载异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 生成ASCII进度条
        /// </summary>
        private static string 生成进度条(double percentage)
        {
            const int 总长度 = 30;
            int 已完成长度 = (int)(percentage / 100.0 * 总长度);
            int 未完成长度 = 总长度 - 已完成长度;

            return "[" + new string('█', 已完成长度) + new string('░', 未完成长度) + "]";
        }

        /// <summary>
        /// 🎯 主程序入口 - 运行所有示例
        /// </summary>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("🎬 Engine2 中文使用示例");
            Console.WriteLine("=".PadRight(50, '='));
            Console.WriteLine();

            try
            {
                // 运行各种示例
                await 简单下载示例();
                Console.WriteLine("\n" + "=".PadRight(50, '=') + "\n");

                await 批量下载示例();
                Console.WriteLine("\n" + "=".PadRight(50, '=') + "\n");

                await 流信息获取示例();
                Console.WriteLine("\n" + "=".PadRight(50, '=') + "\n");

                await 高级配置示例();

                Console.WriteLine("\n🎉 所有示例运行完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 程序运行异常: {ex.Message}");
            }

            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }
    }
}
