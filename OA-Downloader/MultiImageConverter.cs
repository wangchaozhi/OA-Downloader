using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Net.Http;
using System.Collections.Generic;

namespace OA_Downloader
{
    public class MultiImageConverter : IMultiValueConverter
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly Dictionary<string, BitmapImage> _imageCache = new Dictionary<string, BitmapImage>();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] == null)
                return GetPlaceholderImage();

            // ImageUrl is the first binding
            if (values[0] is ImageInfo imageInfo)
            {
                // Check if the Image already exists (second binding)
                if (values[1] is BitmapImage image && image != null)
                {
                    return image; // Return cached Image if exists
                }

                // Modify ImageUrl to get the thumbnail
                string imageUrl = imageInfo.ImageUrl.Replace(":9000", ":9002") + "?mode=thumbnail";

                // Check the cache
                if (_imageCache.TryGetValue(imageUrl, out BitmapImage cachedImage))
                {
                    return cachedImage; // Return cached Image if found
                }

                // Return a placeholder and start async download of the image
                Task.Run(() => LoadImageAsync(imageUrl, imageInfo));

                return GetPlaceholderImage(); // Return the placeholder while downloading
            }

            return GetPlaceholderImage(); // If there's no valid input, return placeholder
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private async Task LoadImageAsync(string imageUrl, ImageInfo imageInfo)
        {
            try
            {
                // Download image data
                byte[] imageData = await httpClient.GetByteArrayAsync(imageUrl);

                // Ensure UI thread update
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    using (var ms = new System.IO.MemoryStream(imageData))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();

                        if (bitmap.CanFreeze)
                        {
                            bitmap.Freeze(); // Ensure cross-thread access
                        }

                        // Cache the image
                        _imageCache[imageUrl] = bitmap;

                        // Update the Image property in ImageInfo, triggering UI update
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
            placeholder.Freeze(); // Ensure cross-thread access
            return placeholder;
        }
    }
}
