using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace OA_Downloader;

public class FileManagerViewModel : INotifyPropertyChanged
{
    
    
    
    private Stack<int> historyStack = new Stack<int>(); // 记录文件夹的导航历史
    private Stack<int> forwardStack = new Stack<int>(); // 记录返回后可以前进的路径

    
    // private int currentFolderId;
    
    public ObservableCollection<Folder> Folders { get; set; }
    public ObservableCollection<ImageInfo> Images { get; set; }
    
    private readonly ImageManager _imageManager = new ImageManager();
  
    // 属性来绑定到按钮的启用状态
    private bool _canGoBack;
    public bool CanGoBack
    {
        get => _canGoBack;
        set
        {
            _canGoBack = value;
            OnPropertyChanged(nameof(CanGoBack));
        }
    }

    private bool _canGoForward;
    public bool CanGoForward
    {
        get => _canGoForward;
        set
        {
            _canGoForward = value;
            OnPropertyChanged(nameof(CanGoForward));
        }
    }
   

    public async Task LoadImagesAsync()
    {
        // 批量下载图片
        await _imageManager.DownloadImagesAsync(Images.ToList());

        // // 更新每个 ImageInfo 对象的 Image 属性
        // foreach (var imageInfo in Images)
        // {
        //     if (_imageManager.ImageCache.TryGetValue(imageInfo.Id, out var bitmapImage))
        //     {
        //         imageInfo.Image = bitmapImage;
        //     }
        // }
    }
    


    private bool _isSelectionEnabled;

    public bool IsSelectionEnabled
    {
        get => _isSelectionEnabled;
        set
        {
            _isSelectionEnabled = value;
            OnPropertyChanged(nameof(IsSelectionEnabled));
            UpdateFoldersSelectionEnabled(_isSelectionEnabled); // 更新每个文件夹的 IsSelectionEnabled
            UpdateImagesSelectionEnabled(_isSelectionEnabled); // 更新每个图片的 IsSelectionEnabled
        }
    }


    public FileManagerViewModel()
    {
        // // 加载文件夹数据
        // Folders = new ObservableCollection<Folder>(
        //     LoadFoldersFromJson(@"E:\RiderProjects\OA-Downloader\OA-Downloader\filemanager.json"));
        Images = new ObservableCollection<ImageInfo>(); // 空的图片集合，等双击文件夹后加载
        Folders = new ObservableCollection<Folder>(); // 空的图片集合，等双击文件夹后加载
        
        // 将根目录（ID 0）初始化推入历史栈
        historyStack.Push(0);
        
        
        // 更新按钮状态
        UpdateNavigationButtons();
        
        // 加载根目录内容
        LoadImagesFromFolder(0);
    }

    public List<Folder> LoadFoldersFromJson(string jsonFilePath)
    {
        // 读取 JSON 文件
        string json = File.ReadAllText(jsonFilePath);

        // 反序列化 JSON 为 Root 对象
        Root root = JsonConvert.DeserializeObject<Root>(json);

        // 返回 uploadPath 中的文件夹列表
        return root.Data.UploadPath ?? new List<Folder>();
    }


    // 双击文件夹时加载图片
    public async Task LoadImagesFromFolder(int folderId)
    {
        var requestData = new { id = folderId };
        try
        {
            string result = await RestClient.PostAsync("/project/uploadPathList", requestData);
            Root root = JsonConvert.DeserializeObject<Root>(result);
            // 更新历史栈，表示进入了一个新文件夹
            if (historyStack.Peek() != folderId)
            {
                historyStack.Push(folderId); // 将新文件夹ID推入历史栈
                forwardStack.Clear(); // 每次进入新文件夹时清空前进栈
            }
            // 更新按钮状态
            UpdateNavigationButtons();
            
            if (root != null && root.Data.UploadPath != null)
            {
                Folders.Clear();
                foreach (var folder in root.Data.UploadPath)
                {
                    folder.IsSelectionEnabled = IsSelectionEnabled;
                    Folders.Add(folder);
                }
            }

            if (root != null && root.Data.MinioPath != null)
            {
                // 清空现有的图片集合
                Images.Clear();
                foreach (var image in root.Data.MinioPath)
                {
                    Images.Add(image);
                }
                // 触发异步加载图片缩略图
                    await LoadImagesAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载图片失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    
    // 返回上级文件夹
    public async Task GoBackAsync()
    {
        if (historyStack.Count > 1) // 确保栈中至少有两个元素，避免根目录被弹出
        {
            forwardStack.Push(historyStack.Pop()); // 当前 folderId 推入前进栈
            int previousFolderId = historyStack.Peek(); // 获取上一级的 folderId
            await LoadImagesFromFolder(previousFolderId); // 加载上级文件夹内容
        }
        // 更新按钮状态
        UpdateNavigationButtons();
    }

    // 前进到下一级文件夹
    public async Task GoForwardAsync()
    {
        if (forwardStack.Count > 0) // 如果前进栈有记录
        {
            int nextFolderId = forwardStack.Pop(); // 获取前进的 folderId
            historyStack.Push(nextFolderId); // 当前文件夹推入历史栈
            await LoadImagesFromFolder(nextFolderId); // 加载前进的文件夹内容
        }
        // 更新按钮状态
        UpdateNavigationButtons();
    }
    
    // 更新前进和后退按钮的状态
    private void UpdateNavigationButtons()
    {
        CanGoBack = historyStack.Count > 1;
        CanGoForward = forwardStack.Count > 0;
    }

    // 更新每个文件夹的 IsSelectionEnabled
    private void UpdateFoldersSelectionEnabled(bool isEnabled)
    {
        foreach (var folder in Folders)
        {
            folder.IsSelectionEnabled = isEnabled;
        }
    }


    private void UpdateImagesSelectionEnabled(bool isEnabled)
    {
        foreach (var image in Images)
        {
            image.IsSelectionEnabled = isEnabled;
        }
    }

    public void ToggleSelection(bool isEnabled)
    {
        IsSelectionEnabled = isEnabled;
        OnPropertyChanged(nameof(IsSelectionEnabled));
    }

    public void PrintSelectedIds()
    {
        var selectedIds = Folders.Where(f => f.IsSelected).Select(f => f.Id).ToArray();
        Console.WriteLine("Selected IDs: " + string.Join(", ", selectedIds));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class Root
{
    public int Code { get; set; }

    public string Msg { get; set; }
    public Data Data { get; set; }
}

public class Data
{
    public List<Folder> UploadPath { get; set; }
    public List<ImageInfo> MinioPath { get; set; }
}

public class Folder : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string PathName { get; set; }
    public int PathLevel { get; set; }
    public int ParentId { get; set; }
    public DateTime? CreateTime { get; set; }
    public string CreateBy { get; set; }
    public string LastOperator { get; set; }
    public DateTime? LastOperateTime { get; set; }
    private bool _isSelectionEnabled;

    public bool IsSelectionEnabled
    {
        get { return _isSelectionEnabled; }
        set
        {
            if (_isSelectionEnabled != value)
            {
                _isSelectionEnabled = value;
                OnPropertyChanged(nameof(IsSelectionEnabled)); // 通知 UI 属性值变化
            }
        }
    }

    private bool _isSelected;

    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;


    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ImageInfo : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string ImageName { get; set; } // 图片名
    
    private static readonly HttpClient httpClient = new HttpClient();
    
    private BitmapImage _image;
    public string _imageUrl { get; set; } // 图片 URL
    public string UserName { get; set; }
    public DateTime? CreateTime { get; set; }
    private bool _isSelectionEnabled;
    
    
    public string ImageUrl
    {
        get => _imageUrl;
        set
        {
            _imageUrl = value;
            OnPropertyChanged(nameof(ImageUrl));
            // // 当设置 ImageUrl 时，自动调用异步加载图片
            // LoadImageAsync().ConfigureAwait(false); // 这里不等待，因为是异步的
        }
    }
    
    public BitmapImage Image
    {
        get => _image;
        set
        {
            _image = value;
            OnPropertyChanged(nameof(Image));
        }
    }
    
    
    // public BitmapImage Image
    // {
    //     get => _image;
    //     private set
    //     {
    //         _image = value;
    //         OnPropertyChanged(nameof(Image)); // 通知 UI 更新图片
    //     }
    // }


    public bool IsSelectionEnabled
    {
        get { return _isSelectionEnabled; }
        set
        {
            if (_isSelectionEnabled != value)
            {
                _isSelectionEnabled = value;
                OnPropertyChanged(nameof(IsSelectionEnabled)); // 通知 UI 属性值变化
            }
        }
    }

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }
    
    
    public async Task LoadImageAsync()
    {
        try
        {
            // 替换端口号并加上 ?mode=thumbnail
            var imageUrl = _imageUrl.Replace(":9000", ":9002") + "?mode=thumbnail";

            // 使用 HttpClient 异步加载图片
            var response = await httpClient.GetAsync(imageUrl);
            response.EnsureSuccessStatusCode();

            byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
            using (var ms = new System.IO.MemoryStream(imageBytes))
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 缓存图像
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 避免跨线程访问问题

                // 更新图像
                Image = bitmapImage;
            }
        }
        catch (Exception ex)
        {
            // 错误处理：加载失败可以使用占位符图像
            Image = new BitmapImage(new Uri("pack://application:,,,/Images/placeholder.png"));
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}