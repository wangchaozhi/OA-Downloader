using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace OA_Downloader
{
    public partial class DownloadWindow : Window
    {
        private int totalFiles; // 总文件数
        private int downloadedFiles; // 已下载文件数
        
        private List<int> folderIds;

        // 限制并行下载数量为 2
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2);

        private static readonly HttpClient httpClient = new HttpClient();

        public DownloadWindow()
        {
            InitializeComponent();
        }
        
        // 构造函数，接受文件夹 ID 列表
        public DownloadWindow(List<int> selectedFolderIds, bool autoStartDownload = false)
        {
            InitializeComponent();
            folderIds = selectedFolderIds; // 保存传入的文件夹 ID 列表
            // 自动填充第一个文件夹 ID 到 TextBox
            if (folderIds.Count > 0)
            {
                idPath.Text = folderIds[0].ToString(); // 设置 TextBox 为第一个 ID
            }

            // 如果传递了 autoStartDownload 参数为 true，则自动点击下载按钮
            if (autoStartDownload)
            {
                
                StartDownloadProcessAsync(); // 开始异步下载处理
            }
        }
        private async Task StartDownloadProcessAsync()
        {
            while (folderIds.Count > 0)
            {
                // 更新当前要下载的文件夹 ID
                idPath.Text = folderIds[0].ToString();

                // 模拟点击下载按钮
                btnStart_Click(null, null);

                // 等待一段时间，确保下载任务已启动，并检查按钮状态
                while (!btnStart.IsEnabled)
                {
                    await Task.Delay(500); // 等待 500ms 直到按钮状态变为可用
                }

                // 如果下载完成，移除已下载的文件夹 ID
                folderIds.RemoveAt(0);
            }
            btnStart.IsEnabled = false;
            MessageBox.Show("所有文件夹下载完成！");
            // 获取当前程序运行目录
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            
            
            // 拼接 "OA_Downloader" 文件夹路径
            string oaDownloaderDirectory = Path.Combine(currentDirectory, "OA_Downloader");

            // 检查文件夹是否存在
            if (Directory.Exists(oaDownloaderDirectory))
            {
                // 打开 OA_Downloader 文件夹
                System.Diagnostics.Process.Start("explorer.exe", oaDownloaderDirectory);
            }
        }
        

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            string id = idPath.Text;

            if (string.IsNullOrEmpty(id))
            {
                MessageBox.Show("请输入id。");
                return;
            }

            // 禁用按钮以防止多次点击
            btnStart.IsEnabled = false;

            try
            {
                LogMessage($"正在发送网络请求请耐心等待");

                // 请求参数
                var requestData = new
                {
                    id = id
                };

                // 使用 RestClient 发起 POST 请求，只传递节点和参数
                string result = await RestClient.PostAsync("/project/getAllUrlByPathId", requestData);
                LogMessage($"请求成功");


                // 解析响应数据并进行下载
                await ParseAndDownloadFilesAsync(result);
            }
            catch (Exception ex)
            {
                LogMessage($"请求或下载时出错: {ex.Message}");
                MessageBox.Show($"请求失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 无论成功与否，完成后启用按钮
                btnStart.IsEnabled = true;
            }
        }

        private async Task ParseAndDownloadFilesAsync(string jsonData)
        {
            try
            {
                // 解析 JSON 响应
                JObject response = JObject.Parse(jsonData);
                // 将解析后的 JSON 保存为 response.json 文件
                string responseFilePath = Path.Combine(Environment.CurrentDirectory, "response.json");
                File.WriteAllText(responseFilePath, response.ToString());
                int code = (int)response["code"];
                string msg = (string)response["msg"];

                // 检查 code 是否为 200
                if (code == 200)
                {
                    // 解析 data 中的文件夹和图片列表
                    JObject data = (JObject)response["data"];


                    // 统计总的文件数
                    totalFiles = 0;
                    foreach (var folder in data)
                    {
                        JArray images = (JArray)folder.Value;
                        totalFiles += images.Count; // 计算所有文件的总数
                    }

                    // 初始化进度条
                    progressBar.Minimum = 0;
                    progressBar.Maximum = totalFiles;
                    downloadedFiles = 0;


                    // 并行处理所有文件夹和文件下载
                    var downloadTasks = new Task[data.Count];

                    int index = 0;
                    foreach (var folder in data)
                    {
                        string folderName = folder.Key; // 文件夹名称
                        JArray images = (JArray)folder.Value; // 图片 URL 列表

                        downloadTasks[index++] = CreateFolderAndDownloadImagesAsync(folderName, images);
                    }

                    // 等待所有文件夹和图片下载任务完成
                    await Task.WhenAll(downloadTasks);

                    // MessageBox.Show("所有图片下载完成！");
                }
                else
                {
                    // 错误处理
                    LogMessage($"错误: {msg}");
                    MessageBox.Show($"下载失败: {msg}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LogMessage("解析响应时出错：" + ex.Message);
                MessageBox.Show("解析响应时出错：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 异步创建文件夹并下载文件
        private async Task CreateFolderAndDownloadImagesAsync(string folderName, JArray images)
        {
            try
            {
                // 创建根目录为 OA_Downloader
                string rootDirectory = Path.Combine(Environment.CurrentDirectory, "OA_Downloader");
                if (!Directory.Exists(rootDirectory))
                {
                    Directory.CreateDirectory(rootDirectory);
                }

                // 创建文件夹
                string folderPath = Path.Combine(rootDirectory, folderName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    LogMessage($"创建文件夹: {folderPath}");
                }

                // 总下载文件数
                int totalFiles = images.Count;
                int filesDownloaded = 0;

                // 并行下载所有图片
                var downloadTasks = new Task[images.Count];
                for (int i = 0; i < images.Count; i++)
                {
                    string imageUrl = (string)images[i];
                    // 这里将 '#' 替换为 URI 编码 '%23'
                    string modifiedUri = imageUrl.Replace("#", "%23");
                    // downloadTasks[i] = DownloadImageAsync(folderPath, modifiedUri);
                    downloadTasks[i] = DownloadImageWithSemaphoreAsync(folderPath, modifiedUri);

                    // downloadTasks[i] = DownloadImageAsync(folderPath, modifiedUri, () =>
                    // {
                    //     filesDownloaded++;
                    //     // 更新进度条和百分比
                    //     UpdateProgress(filesDownloaded, totalFiles);
                    // });
                }

                // 等待所有图片下载任务完成
                await Task.WhenAll(downloadTasks);
            }
            catch (Exception ex)
            {
                LogMessage($"创建文件夹或下载图片时出错: {ex.Message}");
            }
        }

        // // 异步下载图片
        // private async Task DownloadImageAsync(string folderPath, string imageUrl)
        // {
        //     try
        //     {
        //         // 获取文件名
        //         string fileName = Path.GetFileName(imageUrl);
        //         string filePath = Path.Combine(folderPath, fileName);
        //
        //         using (HttpClient httpClient = new HttpClient())
        //         {
        //             byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
        //
        //             // 异步保存图片
        //             await File.WriteAllBytesAsync(filePath, imageBytes);
        //
        //             LogMessage($"下载成功: {fileName} -> {filePath}");
        //         }
        //         // 更新下载进度
        //         downloadedFiles++;
        //         progressBar.Value = downloadedFiles;
        //         UpdateProgress(downloadedFiles, totalFiles);
        //     }
        //     catch (Exception ex)
        //     {
        //         LogMessage($"下载图片失败: {imageUrl}, 错误信息: {ex.Message}");
        //     }
        // }


        // 异步下载图片，使用 SemaphoreSlim 限制并发数量
        private async Task DownloadImageWithSemaphoreAsync(string folderPath, string imageUrl)
        {
            await _semaphore.WaitAsync(); // 等待获取信号量
            try
            {
                // 获取文件名
                string fileName = Path.GetFileName(imageUrl);
                string filePath = Path.Combine(folderPath, fileName);
                
                bool success = false;
                int retryCount = 0;
                while (!success && retryCount < 3)
                {
                    try
                    {
                        byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                        await File.WriteAllBytesAsync(filePath, imageBytes);
                        success = true;
                        LogMessage($"下载成功: {fileName} -> {filePath}");
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        LogMessage($"下载图片失败: {imageUrl}, 错误信息: {ex.Message} (重试第 {retryCount} 次)");
                        if (retryCount >= 3)
                        {
                            LogMessage($"下载失败 {imageUrl}, 已放弃");
                            // 将下载失败的日志累加到 log.txt 中
                            LogFailureMessage($"下载失败 {imageUrl}, 已放弃");
                        }
                    }
                }
                
                // byte[] imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                //
                // // 异步保存图片
                // await File.WriteAllBytesAsync(filePath, imageBytes);
                //
                // LogMessage($"下载成功: {fileName} -> {filePath}");


                // // 更新下载进度
                // downloadedFiles++;
                // 更新下载进度，线程安全的递增
                Interlocked.Increment(ref downloadedFiles);
                progressBar.Value = downloadedFiles;
                UpdateProgress(downloadedFiles, totalFiles);
            }
            catch (Exception ex)
            {
                LogMessage($"下载图片失败: {imageUrl}, 错误信息: {ex.Message}");
            }
            finally
            {
                _semaphore.Release(); // 释放信号量
            }
        }
        
        
        
       


        private void UpdateProgress(int filesDownloaded, int totalFiles)
        {
            // 计算下载进度百分比并向上取整
            double progressValue = Math.Ceiling((double)filesDownloaded / totalFiles * 100);
            // 显示进度条
            progressBar.Visibility = Visibility.Visible;
            // // 更新进度条和百分比显示
            // progressBar.Value = progressValue;
            progressPercentage.Text = $"{progressValue}%";
        }


        private void LogMessage(string message)
        {
            txtLog.AppendText($"{DateTime.Now}: {message}{Environment.NewLine}");
            txtLog.ScrollToEnd();
        }
        
        private void LogFailureMessage(string message)
        {
            string logFilePath = Path.Combine(Environment.CurrentDirectory, "log.txt");
    
            // 将失败的日志累加到 log.txt 文件中
            File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
        }

    }
}