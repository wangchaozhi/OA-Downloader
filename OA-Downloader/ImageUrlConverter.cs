using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace OA_Downloader
{
    public class ImageUrlConverter : IValueConverter
    {
        
        
        // Cache to store image URLs and the corresponding BitmapImage
        private static readonly Dictionary<string, BitmapImage> _imageCache = new Dictionary<string, BitmapImage>();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string imageUrl)
            {
                // Modify the URL
                imageUrl = imageUrl.Replace(":9000", ":9002") + "?mode=thumbnail";
                
                // Check if the image is already cached
                if (_imageCache.TryGetValue(imageUrl, out BitmapImage cachedImage))
                {
                    return cachedImage;
                }

                // If not cached, download and cache the image
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imageUrl, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    
                    // Cache the image for future use
                    _imageCache[imageUrl] = bitmap;

                    return bitmap;
                }
                catch (WebException)
                {
                    // Handle potential download issues
                    return null;
                }
            }

            return null;
        }
       

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
