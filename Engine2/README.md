# Engine2 - N_m3u8DL-RE 封装库

Engine2 是一个基于强大的 N_m3u8DL-RE 工具的 C# 封装库，为 .NET 开发者提供了现代化、类型安全的 API 来下载各种流媒体内容。

## 🚀 核心功能

- ✅ **单URL下载** - 简洁的API支持单个流媒体URL下载
- ✅ **批量下载** - 高效的批量下载，支持并发处理多个URL
- ✅ **实时进度监控** - 详细的进度回调，包含速度、ETA、分片信息
- ✅ **灵活配置** - 丰富的配置选项，满足各种复杂场景
- ✅ **错误处理与重试** - 智能重试机制和详细的错误报告
- ✅ **多种日志方式** - 控制台、文件、组合日志等多种选择
- ✅ **异步编程** - 全面支持 async/await 模式

## 📺 支持的流媒体格式

- **HLS** (.m3u8) - HTTP Live Streaming，苹果公司开发的流媒体协议
- **DASH** (.mpd) - Dynamic Adaptive Streaming over HTTP，国际标准流媒体协议
- **MSS** (.ism) - Microsoft Smooth Streaming，微软平滑流媒体协议
- **HTTP Live** - 直播流录制，支持实时流媒体捕获

## 🚀 快速开始

### 1. 基础下载示例

```csharp
using Engine2.Core;
using Engine2.Interfaces;

// 创建下载引擎实例
var engine = new M3u8DownloadEngine();

// 配置下载参数
var options = new DownloadOptions
{
    OutputDirectory = @"D:\视频下载",        // 输出目录
    FileName = "我的视频",                   // 文件名
    ThreadCount = 8,                        // 下载线程数
    AutoSelectBest = true,                  // 自动选择最佳质量
    DeleteTempFiles = true                  // 完成后删除临时文件
};

// 执行下载任务
var result = await engine.DownloadAsync("https://example.com/video.m3u8", options);

// 检查下载结果
if (result.IsSuccess)
{
    Console.WriteLine($"✅ 下载成功！");
    Console.WriteLine($"📁 文件路径: {result.OutputFilePath}");
    Console.WriteLine($"📊 文件大小: {result.FileSize / (1024 * 1024):F1} MB");
    Console.WriteLine($"⏱️ 耗时: {result.Duration.TotalSeconds:F1} 秒");
}
else
{
    Console.WriteLine($"❌ 下载失败: {result.ErrorMessage}");
}
```

### 2. 实时进度监控

```csharp
// 创建进度监控回调
var progress = new Progress<ProgressInfo>(info =>
{
    Console.WriteLine($"📈 下载进度: {info.Percentage:F1}% " +
                     $"({info.CompletedSegments}/{info.TotalSegments} 分片) " +
                     $"🚀 速度: {info.Speed / (1024 * 1024):F1} MB/s " +
                     $"⏰ 剩余: {info.EstimatedTimeRemaining?.ToString(@"mm\:ss")}");
});

// 带进度监控的下载
var result = await engine.DownloadAsync(url, options, progress);
```

### 3. 批量下载管理

```csharp
// 准备批量下载的URL列表
var urls = new[]
{
    "https://example.com/电影1.m3u8",
    "https://example.com/电影2.m3u8",
    "https://example.com/电影3.m3u8"
};

// 批量下载进度回调
var batchProgress = new Progress<BatchProgressInfo>(info =>
{
    Console.WriteLine($"📦 批量进度: {info.OverallPercentage:F1}% " +
                     $"✅ 已完成: {info.CompletedTasks}/{info.TotalTasks} " +
                     $"❌ 失败: {info.FailedTasks}");

    // 显示当前任务进度
    if (info.CurrentTaskProgress != null)
    {
        Console.WriteLine($"  🎯 当前任务: {Path.GetFileName(info.CurrentTaskUrl)} " +
                         $"- {info.CurrentTaskProgress.Percentage:F1}%");
    }
});

// 执行批量下载
var results = await engine.DownloadBatchAsync(urls, options, batchProgress);

// 汇总结果
Console.WriteLine("\n📋 批量下载完成:");
foreach (var result in results)
{
    var status = result.IsSuccess ? "✅" : "❌";
    var fileName = Path.GetFileName(result.Url);
    Console.WriteLine($"{status} {fileName} -> {(result.IsSuccess ? "成功" : result.ErrorMessage)}");
}
```

## ⚙️ 配置选项详解

### DownloadOptions 核心参数

| 参数名称 | 数据类型 | 默认值 | 功能说明 |
|---------|---------|--------|----------|
| `OutputDirectory` | string | null | 📁 视频输出目录路径 |
| `FileName` | string | null | 📝 保存的文件名（不含扩展名） |
| `TempDirectory` | string | null | 🗂️ 临时文件存储目录 |
| `ThreadCount` | int | CPU核心数 | 🧵 并发下载线程数量 |
| `RetryCount` | int | 3 | 🔄 下载失败重试次数 |
| `HttpTimeoutSeconds` | int | 100 | ⏱️ HTTP请求超时时间（秒） |
| `MaxSpeed` | long? | null | 🚀 最大下载速度限制（字节/秒） |
| `AutoSelectBest` | bool | true | 🎯 自动选择最佳画质流 |
| `ConcurrentDownload` | bool | false | 🎵🎬 并发下载音频和视频 |
| `SkipMerge` | bool | false | 🔗 跳过分片合并步骤 |
| `DeleteTempFiles` | bool | true | 🗑️ 完成后删除临时文件 |
| `WriteMetaJson` | bool | true | 📄 输出流信息JSON文件 |

### 网络和认证配置

| 参数名称 | 数据类型 | 默认值 | 功能说明 |
|---------|---------|--------|----------|
| `Headers` | Dictionary<string,string> | 空字典 | 🔐 自定义HTTP请求头 |
| `ProxyUrl` | string | null | 🌐 代理服务器地址 |
| `UseSystemProxy` | bool | true | 🖥️ 使用系统代理设置 |
| `BaseUrl` | string | null | 🔗 基础URL地址 |
| `DecryptionKeys` | string[] | null | 🔑 视频解密密钥数组 |

### 工具和日志配置

| 参数名称 | 数据类型 | 默认值 | 功能说明 |
|---------|---------|--------|----------|
| `FFmpegPath` | string | null | 🎞️ FFmpeg可执行文件路径 |
| `LogLevel` | LogLevel | Info | 📝 日志输出级别 |

## 🔧 高级功能使用

### 1. 多样化日志配置

```csharp
// 📝 控制台日志 - 适合开发调试
var consoleLogger = new ConsoleLogger();

// 📄 文件日志 - 适合生产环境
var fileLogger = new FileLogger(@"D:\日志\下载记录.log");

// 📋 组合日志 - 同时输出到控制台和文件
var compositeLogger = new CompositeLogger(
    new ConsoleLogger(),
    new FileLogger(@"D:\日志\详细日志.log")
);

// 🔇 静默日志 - 不输出任何日志
var nullLogger = new NullLogger();

// 使用自定义日志创建引擎
var engine = new M3u8DownloadEngine(logger: compositeLogger);
```

### 2. 网络认证和代理配置

```csharp
var options = new DownloadOptions
{
    // 🔐 自定义请求头 - 用于身份验证
    Headers =
    {
        ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
        ["Referer"] = "https://video-site.com",
        ["Authorization"] = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ["Cookie"] = "session_id=abc123; user_pref=hd",
        ["X-Forwarded-For"] = "192.168.1.100"
    },

    // 🌐 代理服务器配置
    ProxyUrl = "http://127.0.0.1:8080",        // HTTP代理
    // ProxyUrl = "socks5://127.0.0.1:1080",   // SOCKS5代理
    UseSystemProxy = false,                     // 禁用系统代理

    // 🔑 解密相关配置
    DecryptionKeys = new[]
    {
        "1234567890abcdef1234567890abcdef",     // AES密钥
        "KID1:KEY1",                            // KID:KEY格式
        "abcdef1234567890abcdef1234567890"      // 备用密钥
    }
};
```

### 3. 流信息预览（无需下载）

```csharp
// 🔍 获取详细流信息
var streamInfo = await engine.GetStreamInfoAsync("https://example.com/video.m3u8");

Console.WriteLine($"📺 流媒体类型: {streamInfo.Type}");
Console.WriteLine($"🔴 是否直播流: {streamInfo.IsLive ? "是" : "否"}");
Console.WriteLine($"⏱️ 视频时长: {streamInfo.Duration?.ToString(@"hh\:mm\:ss")}");
Console.WriteLine($"🎬 视频流数量: {streamInfo.VideoStreams.Count}");
Console.WriteLine($"🎵 音频流数量: {streamInfo.AudioStreams.Count}");
Console.WriteLine($"📝 字幕流数量: {streamInfo.SubtitleStreams.Count}");

// 📊 显示视频流详细信息
Console.WriteLine("\n🎬 可用视频流:");
foreach (var video in streamInfo.VideoStreams)
{
    Console.WriteLine($"  📐 分辨率: {video.Width}x{video.Height}");
    Console.WriteLine($"  🎞️ 编码格式: {video.Codec}");
    Console.WriteLine($"  📊 码率: {video.Bandwidth / 1000} Kbps");
    Console.WriteLine($"  🎯 帧率: {video.FrameRate} fps");
    Console.WriteLine($"  🌍 语言: {video.Language ?? "未知"}");
    Console.WriteLine();
}

// 🎵 显示音频流详细信息
Console.WriteLine("🎵 可用音频流:");
foreach (var audio in streamInfo.AudioStreams)
{
    Console.WriteLine($"  🎼 编码格式: {audio.Codec}");
    Console.WriteLine($"  📊 码率: {audio.Bandwidth / 1000} Kbps");
    Console.WriteLine($"  🔊 采样率: {audio.SampleRate} Hz");
    Console.WriteLine($"  📢 声道数: {audio.Channels}");
    Console.WriteLine($"  🌍 语言: {audio.Language ?? "未知"}");
    Console.WriteLine();
}
```

## 🛡️ 错误处理和异常管理

### 1. 完整的错误处理示例

```csharp
try
{
    // 🎯 执行下载任务
    var result = await engine.DownloadAsync(url, options);

    if (result.IsSuccess)
    {
        Console.WriteLine($"✅ 下载成功完成！");
        Console.WriteLine($"📁 输出文件: {result.OutputFilePath}");
        Console.WriteLine($"📊 文件大小: {result.FileSize / (1024 * 1024):F2} MB");
        Console.WriteLine($"⏱️ 下载耗时: {result.Duration.TotalMinutes:F1} 分钟");
    }
    else
    {
        // ❌ 下载失败处理
        Console.WriteLine($"❌ 下载失败: {result.ErrorMessage}");

        if (result.Exception != null)
        {
            Console.WriteLine($"🔍 异常详情: {result.Exception.Message}");
            Console.WriteLine($"📋 堆栈跟踪: {result.Exception.StackTrace}");
        }

        // 📊 失败统计信息
        Console.WriteLine($"🔗 失败的URL: {result.Url}");
        Console.WriteLine($"⏱️ 尝试时长: {result.Duration.TotalSeconds:F1} 秒");
    }
}
catch (FileNotFoundException ex)
{
    Console.WriteLine("🚫 N_m3u8DL-RE 可执行文件未找到！");
    Console.WriteLine("💡 解决方案:");
    Console.WriteLine("   1. 下载 N_m3u8DL-RE 并添加到 PATH 环境变量");
    Console.WriteLine("   2. 或在创建引擎时指定完整路径:");
    Console.WriteLine("      var engine = new M3u8DownloadEngine(@\"C:\\Tools\\N_m3u8DL-RE.exe\");");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"⚠️ 参数配置错误: {ex.Message}");
    Console.WriteLine("💡 请检查 DownloadOptions 中的配置参数");
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"🔒 权限不足: {ex.Message}");
    Console.WriteLine("💡 请确保对输出目录有写入权限");
}
catch (DirectoryNotFoundException ex)
{
    Console.WriteLine($"📁 目录不存在: {ex.Message}");
    Console.WriteLine("💡 请确保输出目录路径正确且存在");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"🌐 网络请求失败: {ex.Message}");
    Console.WriteLine("💡 请检查网络连接和URL有效性");
}
catch (TaskCanceledException ex)
{
    Console.WriteLine($"⏰ 下载任务被取消或超时: {ex.Message}");
    Console.WriteLine("💡 可以尝试增加超时时间或检查网络状况");
}
catch (Exception ex)
{
    Console.WriteLine($"❓ 未知错误: {ex.Message}");
    Console.WriteLine($"🔍 异常类型: {ex.GetType().Name}");
    Console.WriteLine($"📋 详细信息: {ex}");
}
```

### 2. 批量下载的错误处理

```csharp
var results = await engine.DownloadBatchAsync(urls, options, batchProgress);

// 📊 统计下载结果
var successCount = results.Count(r => r.IsSuccess);
var failureCount = results.Count(r => !r.IsSuccess);

Console.WriteLine($"\n📈 批量下载完成统计:");
Console.WriteLine($"✅ 成功: {successCount} 个");
Console.WriteLine($"❌ 失败: {failureCount} 个");
Console.WriteLine($"📊 成功率: {(double)successCount / results.Count * 100:F1}%");

// 📋 详细失败信息
if (failureCount > 0)
{
    Console.WriteLine($"\n❌ 失败详情:");
    foreach (var failedResult in results.Where(r => !r.IsSuccess))
    {
        Console.WriteLine($"  🔗 URL: {failedResult.Url}");
        Console.WriteLine($"  ❌ 错误: {failedResult.ErrorMessage}");
        Console.WriteLine();
    }
}
```

## 📋 系统要求和依赖

### 🔧 必需组件

1. **N_m3u8DL-RE** - 核心下载引擎
   - 📥 下载地址: https://github.com/nilaoda/N_m3u8DL-RE/releases
   - 🔧 安装方式:
     - 方式一: 将可执行文件添加到系统 PATH 环境变量
     - 方式二: 通过构造函数指定完整路径
   - 💡 支持版本: v0.2.0 及以上版本

2. **.NET 运行时** - 应用程序框架
   - 🎯 最低要求: .NET 6.0
   - 🚀 推荐版本: .NET 8.0 或更高
   - 📥 下载地址: https://dotnet.microsoft.com/download

### 🔧 可选组件

3. **FFmpeg** - 音视频处理工具
   - 🎞️ 用途: 高级音视频合并和转换功能
   - 📥 下载地址: https://ffmpeg.org/download.html
   - 💡 如不安装，将使用 N_m3u8DL-RE 内置的合并功能

## 🚀 安装和部署指南

### 方法一: 直接集成到项目

1. 📁 将 `Engine2` 文件夹复制到您的项目根目录
2. 📝 在项目文件中添加项目引用:
   ```xml
   <ProjectReference Include="Engine2\Engine2.csproj" />
   ```
3. 🔧 在代码中添加命名空间引用:
   ```csharp
   using Engine2.Core;
   using Engine2.Interfaces;
   ```

### 方法二: 作为独立类库

1. 🏗️ 编译 Engine2 项目生成 DLL:
   ```bash
   dotnet build Engine2/Engine2.csproj --configuration Release
   ```
2. 📦 将生成的 DLL 添加到您的项目引用中
3. 🔧 确保 N_m3u8DL-RE.exe 可访问

### 环境配置检查

```csharp
// 🔍 检查 N_m3u8DL-RE 是否可用
try
{
    var engine = new M3u8DownloadEngine();
    Console.WriteLine("✅ N_m3u8DL-RE 引擎初始化成功");
}
catch (FileNotFoundException)
{
    Console.WriteLine("❌ 未找到 N_m3u8DL-RE 可执行文件");
    Console.WriteLine("💡 请从以下地址下载: https://github.com/nilaoda/N_m3u8DL-RE/releases");
}

// 🔍 检查 URL 支持性
var testUrl = "https://example.com/test.m3u8";
if (engine.IsUrlSupported(testUrl))
{
    Console.WriteLine($"✅ URL 格式受支持: {testUrl}");
}
else
{
    Console.WriteLine($"❌ URL 格式不受支持: {testUrl}");
}
```

## 📚 完整示例项目

查看 `Engine2/Examples/BasicUsageExample.cs` 文件，包含以下完整示例:

- 🎯 **简单下载示例** - 基础单文件下载
- 📦 **批量下载示例** - 多文件并发下载
- 🔍 **流信息获取示例** - 预览流媒体信息
- ⚙️ **高级配置示例** - 复杂场景配置

## 🤝 开源协议和贡献

### 📄 许可证
本项目基于 **MIT 许可证** 开源，允许自由使用、修改和分发。

### 🤝 贡献指南
我们欢迎各种形式的贡献：

- 🐛 **Bug 报告**: 在 Issues 中报告发现的问题
- 💡 **功能建议**: 提出新功能或改进建议
- 🔧 **代码贡献**: 提交 Pull Request 改进代码
- 📖 **文档完善**: 帮助改进文档和示例

### 🔗 相关链接

- 🏠 **项目主页**: https://github.com/iwxdeng/N_m3u8DL-RE
- 🔧 **N_m3u8DL-RE 原项目**: https://github.com/nilaoda/N_m3u8DL-RE
- 🎞️ **FFmpeg 官网**: https://ffmpeg.org/
- 📖 **.NET 官方文档**: https://docs.microsoft.com/dotnet/

---

## 💬 技术支持

如果您在使用过程中遇到问题，可以通过以下方式获取帮助：

1. 📖 查阅本文档和示例代码
2. 🔍 在 GitHub Issues 中搜索相似问题
3. 🆕 创建新的 Issue 描述您的问题
4. 💬 参与项目讨论和交流

**祝您使用愉快！** 🎉
