using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;

namespace OA_Downloader
{
    public partial class ListViewControl : UserControl
    {
        private GridViewColumn _selectColumn;

        public ListViewControl()
        {
            InitializeComponent();
            
            // 创建 "Select" 列，使用资源中的 DataTemplate
            _selectColumn = new GridViewColumn
            {
                Header = "Select",
                CellTemplate = (DataTemplate)this.Resources["SelectColumnTemplate"] // 使用模板
            };

            // 监听 DataContext 的变化事件
            this.DataContextChanged += ListViewControl_DataContextChanged;
        }

        // 双击事件处理函数
        private async void ListViewMode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 如果是双击或多次点击，忽略
            if (e.ClickCount > 1) return;
            // 获取单击的行
            if (ListViewMode.SelectedItem is Folder selectedFolder)
            {
                int folderId = selectedFolder.Id;

                // 获取 ViewModel 并加载图片
                if (DataContext is FileManagerViewModel viewModel)
                {
                    await viewModel.LoadImagesFromFolder(folderId);
                }
            }
        }
        
        
        private void ImageListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1) return; // 防止双击或多次点击

            // 获取选中的图片
            if (ImageListView.SelectedItem is ImageInfo selectedImage)
            {
                // 创建并展示新的 ImageWindow，传入选中的图片URL
                ImageWindow imageWindow = new ImageWindow(selectedImage.ImageUrl);
                imageWindow.ShowDialog(); // 显示图片窗口并等待用户关闭
            }
        }
        
        

        // // 双击事件处理函数
        // private async void ListViewMode_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        // {
        //     // 获取双击的行
        //     if (ListViewMode.SelectedItem is Folder selectedFolder)
        //     {
        //         int folderId = selectedFolder.Id;
        //
        //         // 生成请求数据
        //         var requestData = new
        //         {
        //             id = folderId
        //         };
        //
        //         try
        //         {
        //             // 发送 POST 请求到 "/project/getAllUrlByPathId"
        //             string result = await RestClient.PostAsync("/project/uploadPathList", requestData);
        //             // 解析返回的 JSON 数据
        //             var jsonResponse = JObject.Parse(result);
        //             int code = (int)jsonResponse["code"];
        //             if (code == 200 && jsonResponse["data"] != null && jsonResponse["data"]["uploadPath"] != null)
        //             {
        //                 var uploadPaths = jsonResponse["data"]["uploadPath"].ToObject<List<Folder>>();
        //
        //                 var viewModel = DataContext as FileManagerViewModel;
        //                 if (viewModel != null)
        //                 {
        //                     viewModel.Folders.Clear(); // 清空现有的文件夹列表
        //                     foreach (var folder in uploadPaths)
        //                     {
        //                         // 检查字段不为 null 时才赋值
        //                         folder.LastOperateTime = folder.LastOperateTime ?? DateTime.MinValue; // 使用默认值
        //                         folder.CreateTime = folder.CreateTime ?? DateTime.MinValue; // 使用默认值
        //
        //                         viewModel.Folders.Add(folder); // 添加新的文件夹列表
        //                     }
        //                 }
        //
        //                 // MessageBox.Show("数据已成功渲染到视图");
        //             }
        //             else
        //             {
        //                 MessageBox.Show("请求返回的 code 不为 200 或数据为空", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        //             }
        //         }
        //         catch (Exception ex)
        //         {
        //             MessageBox.Show($"请求失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        //         }
        //     }
        // }
        private void ListViewControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as FileManagerViewModel;
            if (viewModel != null)
            {
                viewModel.PropertyChanged += ViewModel_PropertyChanged;
                UpdateSelectColumnVisibility(); // 初始化时根据 IsSelectionEnabled 设置列是否显示
            }
        }
      

        // 监听 IsSelectionEnabled 的变化
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileManagerViewModel.IsSelectionEnabled))
            {
                UpdateSelectColumnVisibility(); // 更新列的显示状态
            }
        }

        // 根据 IsSelectionEnabled 来动态控制列的显示与隐藏
        public void UpdateSelectColumnVisibility()
        {
            var viewModel = this.DataContext as FileManagerViewModel;

            if (viewModel != null && viewModel.IsSelectionEnabled)
            {
                if (!GridViewMode.Columns.Contains(_selectColumn))
                {
                    GridViewMode.Columns.Insert(0, _selectColumn); // 动态添加列到 GridView
                }
            }
            else
            {
                if (GridViewMode.Columns.Contains(_selectColumn))
                {
                    GridViewMode.Columns.Remove(_selectColumn); // 动态移除列
                }
            }
        }
    }
}