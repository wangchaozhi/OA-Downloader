
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OA_Downloader;
public class ImageManager
{
    private static readonly HttpClient httpClient = new HttpClient();

    public ConcurrentDictionary<int, BitmapImage> ImageCache { get; } = new ConcurrentDictionary<int, BitmapImage>();

    public async Task DownloadImagesAsync(List<ImageInfo> images)
    {
        // 创建任务列表，收集 ImageUrl 和 Id
        var tasks = images.Select(async image =>
        {
            try
            {
                // 首先检查图片是否已经缓存
                if (ImageCache.TryGetValue(image.Id, out var cachedImage))
                {
                    // 如果缓存中已经有图片，则直接返回缓存的图片
                    image.Image = cachedImage;
                    return; // 直接返回，跳过下载
                }

                // 替换端口号并加上 ?mode=thumbnail
                string modifiedUrl = image.ImageUrl.Replace(":9000", ":9002") + "?mode=thumbnail";

                // 下载图片数据流
                byte[] imageData = await httpClient.GetByteArrayAsync(modifiedUrl);

                // 将 byte[] 转换为 BitmapImage
                BitmapImage bitmapImage = ConvertToBitmapImage(imageData);

                // 缓存图片
                ImageCache.TryAdd(image.Id, bitmapImage);

                // 实时更新 UI —— 在主线程上进行图片的显示
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 将下载的图片显示在 UI 上
                    image.Image = bitmapImage;
                });
            }
            catch (Exception ex)
            {
                // 处理错误，记录日志或者提供占位符图片
                Console.WriteLine($"下载图片失败: {ex.Message}");
            }
        }).ToList();

        // 并行执行所有下载任务
        await Task.WhenAll(tasks);
    }
    // public async Task DownloadImagesAsync(List<ImageInfo> images)
    // {
    //     // 创建任务列表，收集 ImageUrl 和 Id
    //     var tasks = images.Select(async image =>
    //     {
    //         try
    //         {
    //             // 替换端口号并加上 ?mode=thumbnail
    //             string modifiedUrl = image.ImageUrl.Replace(":9000", ":9002") + "?mode=thumbnail";
    //
    //             // 下载图片数据流
    //             byte[] imageData = await httpClient.GetByteArrayAsync(modifiedUrl);
    //
    //             // 将 byte[] 转换为 BitmapImage
    //             BitmapImage bitmapImage = ConvertToBitmapImage(imageData);
    //
    //             // 缓存图片
    //             ImageCache.TryAdd(image.Id, bitmapImage);
    //
    //             // 实时更新 UI —— 在主线程上进行图片的显示
    //             Application.Current.Dispatcher.Invoke(() =>
    //             {
    //                 // 将下载的图片显示在 UI 上
    //                 image.Image = bitmapImage;
    //             });
    //         }
    //         catch (Exception ex)
    //         {
    //             // 处理错误，记录日志或者提供占位符图片
    //             Console.WriteLine($"下载图片失败: {ex.Message}");
    //         }
    //     }).ToList();
    //
    //     // 并行执行所有下载任务
    //     await Task.WhenAll(tasks);
    // }

    
    // public async Task DownloadImagesAsync(List<ImageInfo> images)
    // {
    //     // 使用 SemaphoreSlim 限制并发任务数量为 10
    //     using (SemaphoreSlim semaphore = new SemaphoreSlim(20))
    //     {
    //         // 创建任务列表，收集 ImageUrl 和 Id
    //         var tasks = images.Select(async image =>
    //         {
    //             await semaphore.WaitAsync(); // 等待信号量
    //
    //             try
    //             {
    //                 // 替换端口号并加上 ?mode=thumbnail
    //                 string modifiedUrl = image.ImageUrl.Replace(":9000", ":9002") + "?mode=thumbnail";
    //
    //                 // 下载图片数据流
    //                 byte[] imageData = await httpClient.GetByteArrayAsync(modifiedUrl);
    //
    //                 // 将 byte[] 转换为 BitmapImage
    //                 BitmapImage bitmapImage = ConvertToBitmapImage(imageData);
    //
    //                 // 将图片缓存到 ImageCache 中
    //                 ImageCache.TryAdd(image.Id, bitmapImage);
    //             }
    //             catch (Exception ex)
    //             {
    //                 // 处理错误，记录日志或者提供占位符图片
    //                 Console.WriteLine($"下载图片失败: {ex.Message}");
    //             }
    //             finally
    //             {
    //                 semaphore.Release(); // 释放信号量
    //             }
    //         }).ToList();
    //
    //         // 并行执行所有下载任务
    //         await Task.WhenAll(tasks);
    //     }
    // }
    //
    // public async Task DownloadImagesAsync(List<ImageInfo> images)
    // {
    //     // 创建任务列表，收集 ImageUrl 和 Id
    //     var tasks = images.Select(image => Task.Run(async () =>
    //     {
    //         try
    //         {
    //             // 替换端口号并加上 ?mode=thumbnail
    //             string modifiedUrl = image.ImageUrl.Replace(":9000", ":9002") + "?mode=thumbnail";
    //
    //             // 下载图片数据流
    //             byte[] imageData = await httpClient.GetByteArrayAsync(modifiedUrl);
    //
    //             // 将 byte[] 转换为 BitmapImage
    //             BitmapImage bitmapImage = ConvertToBitmapImage(imageData);
    //
    //             // 将图片缓存到 ImageCache 中
    //             ImageCache.TryAdd(image.Id, bitmapImage);
    //         }
    //         catch (Exception ex)
    //         {
    //             // 处理错误，记录日志或者提供占位符图片
    //             Console.WriteLine($"下载图片失败: {ex.Message}");
    //         }
    //     })).ToList();
    //
    //     // 并行执行所有下载任务
    //     await Task.WhenAll(tasks);
    // }

    private BitmapImage ConvertToBitmapImage(byte[] imageData)
    {
        BitmapImage bitmapImage = new BitmapImage();
        using (MemoryStream ms = new MemoryStream(imageData))
        {
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // 避免跨线程访问
        }
        return bitmapImage;
    }
}
