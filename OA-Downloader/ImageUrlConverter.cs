using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace OA_Downloader
{
    public class ImageUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // if (value is string imageUrl)
            // {
            //     // 将9000端口替换为9002端口
            //     return imageUrl.Replace(":9000", ":9002");
            // }
            // return value;
            
            if (value is string imageUrl)
            {
                // 替换端口号
                imageUrl = imageUrl.Replace(":9000", ":9002")+ "?mode=thumbnail";
                
                // 加载图片
                return imageUrl;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        
        
        private Task<BitmapImage> LoadImageAsync(string imageUrl)
        {
            try
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(imageUrl, UriKind.Absolute);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 缓存图片数据
                bitmapImage.EndInit();
            
                return Task.FromResult(bitmapImage);
            }
            catch (Exception ex)
            {
                // 可以记录错误日志
                return Task.FromResult<BitmapImage>(null);
            }
        }
    }
}
