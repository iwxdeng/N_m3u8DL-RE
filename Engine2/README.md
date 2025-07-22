# Engine2 - N_m3u8DL-RE Wrapper Library

Engine2 是一个基于 N_m3u8DL-RE 的 C# 封装库，提供了简单易用的 API 来下载 HLS、DASH、MSS 等流媒体内容。

## 功能特性

- ✅ **简单的URL下载功能** - 支持单个流媒体URL的下载
- ✅ **批量下载功能** - 支持多个URL的批量下载
- ✅ **进度监控功能** - 实时监控下载进度和状态
- ✅ **自定义配置功能** - 丰富的配置选项满足各种需求
- ✅ **错误处理和重试功能** - 自动重试和详细的错误信息

## 支持的流媒体格式

- **HLS** (.m3u8) - HTTP Live Streaming
- **DASH** (.mpd) - Dynamic Adaptive Streaming over HTTP
- **MSS** (.ism) - Microsoft Smooth Streaming
- **HTTP Live** - 直播流录制

## 快速开始

### 1. 基本使用

```csharp
using Engine2.Core;
using Engine2.Interfaces;

// 创建下载引擎
var engine = new M3u8DownloadEngine();

// 配置下载选项
var options = new DownloadOptions
{
    OutputDirectory = @"C:\Downloads",
    FileName = "my_video",
    ThreadCount = 8,
    AutoSelectBest = true
};

// 执行下载
var result = await engine.DownloadAsync("https://example.com/video.m3u8", options);

if (result.IsSuccess)
{
    Console.WriteLine($"下载成功: {result.OutputFilePath}");
}
else
{
    Console.WriteLine($"下载失败: {result.ErrorMessage}");
}
```

### 2. 带进度监控的下载

```csharp
// 进度回调
var progress = new Progress<ProgressInfo>(info =>
{
    Console.WriteLine($"进度: {info.Percentage:F1}% - {info.StatusMessage}");
});

var result = await engine.DownloadAsync(url, options, progress);
```

### 3. 批量下载

```csharp
var urls = new[]
{
    "https://example.com/video1.m3u8",
    "https://example.com/video2.m3u8",
    "https://example.com/video3.m3u8"
};

var batchProgress = new Progress<BatchProgressInfo>(info =>
{
    Console.WriteLine($"批量进度: {info.OverallPercentage:F1}% " +
                     $"({info.CompletedTasks}/{info.TotalTasks})");
});

var results = await engine.DownloadBatchAsync(urls, options, batchProgress);
```

## 配置选项

### DownloadOptions 主要参数

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `OutputDirectory` | string | null | 输出目录 |
| `FileName` | string | null | 文件名（不含扩展名） |
| `ThreadCount` | int | CPU核心数 | 下载线程数 |
| `RetryCount` | int | 3 | 重试次数 |
| `HttpTimeoutSeconds` | int | 100 | HTTP超时时间（秒） |
| `MaxSpeed` | long? | null | 最大下载速度（字节/秒） |
| `AutoSelectBest` | bool | true | 自动选择最佳流 |
| `ConcurrentDownload` | bool | false | 并发下载音视频 |
| `DeleteTempFiles` | bool | true | 删除临时文件 |
| `Headers` | Dictionary | 空 | 自定义请求头 |
| `ProxyUrl` | string | null | 代理URL |
| `FFmpegPath` | string | null | FFmpeg路径 |
| `DecryptionKeys` | string[] | null | 解密密钥 |

## 高级用法

### 自定义日志

```csharp
// 组合日志（同时输出到控制台和文件）
var logger = new CompositeLogger(
    new ConsoleLogger(),
    new FileLogger(@"C:\Logs\download.log")
);

var engine = new M3u8DownloadEngine(logger: logger);
```

### 自定义请求头和代理

```csharp
var options = new DownloadOptions
{
    Headers = 
    {
        ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
        ["Referer"] = "https://example.com",
        ["Authorization"] = "Bearer your-token"
    },
    ProxyUrl = "http://127.0.0.1:8080",
    UseSystemProxy = false
};
```

### 获取流信息（不下载）

```csharp
var streamInfo = await engine.GetStreamInfoAsync("https://example.com/video.m3u8");

Console.WriteLine($"流类型: {streamInfo.Type}");
Console.WriteLine($"是否直播: {streamInfo.IsLive}");
Console.WriteLine($"时长: {streamInfo.Duration}");
Console.WriteLine($"视频流数量: {streamInfo.VideoStreams.Count}");
Console.WriteLine($"音频流数量: {streamInfo.AudioStreams.Count}");
```

## 错误处理

```csharp
try
{
    var result = await engine.DownloadAsync(url, options);
    
    if (!result.IsSuccess)
    {
        Console.WriteLine($"下载失败: {result.ErrorMessage}");
        
        if (result.Exception != null)
        {
            Console.WriteLine($"异常详情: {result.Exception}");
        }
    }
}
catch (FileNotFoundException ex)
{
    Console.WriteLine("N_m3u8DL-RE 可执行文件未找到");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"参数错误: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"未知错误: {ex.Message}");
}
```

## 依赖要求

1. **N_m3u8DL-RE** - 核心下载引擎
   - 下载地址: https://github.com/nilaoda/N_m3u8DL-RE/releases
   - 需要将可执行文件放在 PATH 环境变量中，或通过构造函数指定路径

2. **FFmpeg** (可选)
   - 用于音视频合并和转换
   - 下载地址: https://ffmpeg.org/download.html

3. **.NET 6.0+** - 运行时要求

## 安装和部署

1. 将 Engine2 文件夹复制到您的项目中
2. 添加对 Engine2 命名空间的引用
3. 确保 N_m3u8DL-RE.exe 在系统 PATH 中或指定完整路径
4. （可选）安装 FFmpeg 用于高级功能

## 示例项目

查看 `Engine2/Examples/BasicUsageExample.cs` 文件获取完整的使用示例。

## 许可证

本项目基于 MIT 许可证开源。

## 贡献

欢迎提交 Issue 和 Pull Request 来改进这个项目。

## 相关链接

- [N_m3u8DL-RE 原项目](https://github.com/nilaoda/N_m3u8DL-RE)
- [FFmpeg 官网](https://ffmpeg.org/)
