using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LiveCharts;
using LiveCharts.Wpf;

namespace OA_Downloader;

public partial class FileManagerWindow : Window
{
    private bool isListView = true;

    public FileManagerWindow()
    {
        InitializeComponent();
        this.DataContext = new FileManagerViewModel(); // 绑定数据
    }
    
    
    private async void DownloadSelectedFolders(object sender, RoutedEventArgs e)
    {
        // 获取 ViewModel 中选中的文件夹
        var viewModel = (FileManagerViewModel)this.DataContext;

        // 获取选中的文件夹 ID
        var selectedFolderIds = viewModel.Folders
            .Where(folder => folder.IsSelected)
            .Select(folder => folder.Id)
            .ToList();

        if (selectedFolderIds.Count == 0)
        {
            MessageBox.Show("请选择至少一个文件夹进行下载");
            return;
        }

        // 打开 DownloadWindow 窗口
        DownloadWindow downloadWindow = new DownloadWindow(selectedFolderIds, true);
        downloadWindow.Show();
    }


    
    
    
    // private async void DownloadSelectedFolders(object sender, RoutedEventArgs e)
    // {
    //     // 获取 ViewModel 中选中的文件夹
    //     var viewModel = (FileManagerViewModel)this.DataContext;
    //
    //     // 获取选中的文件夹 ID
    //     var selectedFolderIds = viewModel.Folders
    //         .Where(folder => folder.IsSelected)
    //         .Select(folder => folder.Id)
    //         .ToList();
    //
    //     if (selectedFolderIds.Count == 0)
    //     {
    //         MessageBox.Show("请选择至少一个文件夹进行下载");
    //         return;
    //     }
    //
    //     try
    //     {
    //         // 发送选中的文件夹 ID 数组到接口
    //         var requestData = new
    //         {
    //             folderIds = selectedFolderIds
    //         };
    //
    //         string result = await RestClient.PostAsync("/project/downloadFolders", requestData);
    //         // MessageBox.Show("下载请求已发送");
    //         MessageBox.Show(result);
    //
    //         // 处理接口响应
    //         // 例如：您可以在此处处理下载后的结果，或者更新 UI 等
    //     }
    //     catch (Exception ex)
    //     {
    //         MessageBox.Show($"下载请求失败: {ex.Message}");
    //     }
    // }
    
    private async void GoBack_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as FileManagerViewModel; // 将 DataContext 转换为 FileManagerViewModel
        if (viewModel != null)
        {
            await viewModel.GoBackAsync(); // 调用 ViewModel 的 GoBackAsync 方法
        }
    }

    private async void GoForward_Click(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as FileManagerViewModel; // 将 DataContext 转换为 FileManagerViewModel
        if (viewModel != null)
        {
            await viewModel.GoForwardAsync(); // 调用 ViewModel 的 GoForwardAsync 方法
        }
    }

    // private async void GoBack_Click(object sender, RoutedEventArgs e)
    // {
    //     await DataContext.GoBackAsync(); // 触发返回上级
    // }
    //
    // private async void GoForward_Click(object sender, RoutedEventArgs e)
    // {
    //     await DataContext.GoForwardAsync(); // 触发前进到下一级
    // }

    

    // private void ToggleView_Click(object sender, RoutedEventArgs e)
    // {
    //     if (isListView)
    //     {
    //         ListViewMode.Visibility = Visibility.Collapsed;
    //         TileViewMode.Visibility = Visibility.Visible;
    //     }
    //     else
    //     {
    //         ListViewMode.Visibility = Visibility.Visible;
    //         TileViewMode.Visibility = Visibility.Collapsed;
    //     }
    //     isListView = !isListView;
    // }
    private void ToggleView_Click(object sender, RoutedEventArgs e)
    {
        if (ListViewMode.Visibility == Visibility.Visible)
        {
            ListViewMode.Visibility = Visibility.Collapsed;
            TileViewMode.Visibility = Visibility.Visible;
        }
        else
        {
            ListViewMode.Visibility = Visibility.Visible;
            TileViewMode.Visibility = Visibility.Collapsed;
        }
    }

    
    // private void EnableSelection_Checked(object sender, RoutedEventArgs e)
    // {
    //     ((FileManagerViewModel)this.DataContext).ToggleSelection(true);
    // }
    //
    // private void EnableSelection_Unchecked(object sender, RoutedEventArgs e)
    // {
    //     ((FileManagerViewModel)this.DataContext).ToggleSelection(false);
    // }
    
    private void EnableSelection_Checked(object sender, RoutedEventArgs e)
    {
        ((FileManagerViewModel)this.DataContext).IsSelectionEnabled = true;
    }

    private void EnableSelection_Unchecked(object sender, RoutedEventArgs e)
    {
        ((FileManagerViewModel)this.DataContext).IsSelectionEnabled = false;
    }

    
    
    
    

}


