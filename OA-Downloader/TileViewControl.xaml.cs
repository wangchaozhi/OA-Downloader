using System.Windows.Controls;
using System.Windows.Input;

namespace OA_Downloader;

public partial class TileViewControl : UserControl
{
    public TileViewControl()
    {
        InitializeComponent();
    }
    
    
    
    private async void ListViewMode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // 如果是双击或多次点击，忽略
        if (e.ClickCount > 1) return;
        
        
        // 获取当前单击的项
        if (sender is Border border && border.DataContext is Folder selectedFolder)
        {
            int folderId = selectedFolder.Id;

            // 获取 ViewModel 并加载图片
            if (DataContext is FileManagerViewModel viewModel)
            {
                await viewModel.LoadImagesFromFolder(folderId);
            }
        }
       
    }
    
    
    
    // private void ImageTileView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    // {
    //     if (e.ClickCount > 1) return; // 防止双击或多次点击
    //
    //     // 获取选中的图片
    //     if (ImageListView.SelectedItem is ImageInfo selectedImage)
    //     {
    //         // 创建并展示新的 ImageWindow，传入选中的图片URL
    //         ImageWindow imageWindow = new ImageWindow(selectedImage.ImageUrl);
    //         imageWindow.ShowDialog(); // 显示图片窗口并等待用户关闭
    //     }
    // }
    
    
    private void ImageTileView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // 防止双击或多次点击
        if (e.ClickCount > 1) return;

        // 获取触发事件的 Border 元素
        if (sender is Border border && border.DataContext is ImageInfo selectedImage)
        {
            // 创建并展示新的 ImageWindow，传入选中的图片URL
            ImageWindow imageWindow = new ImageWindow(selectedImage.ImageUrl);
            imageWindow.ShowDialog(); // 显示图片窗口并等待用户关闭
        }
    }

}