using System.Windows;
using System.Windows.Media.Imaging;

namespace OA_Downloader;

public partial class ImageWindow : Window
{
    public ImageWindow(string imageUrl)
    {
        InitializeComponent();
        // 加载图片到窗口
        DisplayedImage.Source = new BitmapImage(new Uri(imageUrl));
    }
}