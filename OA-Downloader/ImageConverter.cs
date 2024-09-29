using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace OA_Downloader
{
    public class ImageConverter : IValueConverter
    {
        // Cache to store image URLs and the corresponding BitmapImage
        private readonly Dictionary<string, BitmapImage> _imageCache = new Dictionary<string, BitmapImage>();

        // Single HttpClient instance for reuse
        private static readonly HttpClient httpClient = new HttpClient();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ImageInfo imageInfo && !string.IsNullOrEmpty(imageInfo.ImageUrl))
            {
                // Modify the URL
                string imageUrl = imageInfo.ImageUrl.Replace(":9000", ":9002") + "?mode=thumbnail";

                // Check if the image is already cached
                if (_imageCache.TryGetValue(imageUrl, out BitmapImage cachedImage))
                {
                    return cachedImage;
                }

                // Return placeholder image immediately
                var placeholder = GetPlaceholderImage();

                // Start async load for the actual image
                Task.Run(() => LoadImageAsync(imageUrl, imageInfo));

                return placeholder; // Show placeholder while image is loading
            }

            return GetPlaceholderImage(); // Return a placeholder image if the input is not valid
        }

        private async Task LoadImageAsync(string imageUrl, ImageInfo imageInfo)
        {
            try
            {
                // 下载图片数据流
                byte[] imageData = await httpClient.GetByteArrayAsync(imageUrl);

                // 在UI线程上创建和设置BitmapImage
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    using (var ms = new System.IO.MemoryStream(imageData))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;  // 使用下载的字节流
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        // 确保图片可以跨线程访问
                        if (bitmap.CanFreeze)
                        {
                            bitmap.Freeze();
                        }

                        // 缓存图片
                        _imageCache[imageUrl] = bitmap;

                        // 更新 imageInfo 的 Image 属性，触发 UI 更新
                        imageInfo.Image = bitmap;
                    }
                });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        private BitmapImage GetPlaceholderImage()
        {
            var placeholder = new BitmapImage();
            placeholder.BeginInit();
            placeholder.UriSource = new Uri("pack://application:,,,/Resources/images/placeholder.webp", UriKind.Absolute);
            placeholder.CacheOption = BitmapCacheOption.OnLoad;
            placeholder.EndInit();
            placeholder.Freeze();
            return placeholder;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
