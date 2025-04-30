using System.Diagnostics;
using System.IO;
using System.Windows;
using Newtonsoft.Json;

namespace OA_Downloader;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        LoadConfig();
        // 启用数据绑定错误的调试输出
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
    }
    
    private void LoadConfig()
    {
        try
        {
            // 假设 config.json 位于项目根目录或输出目录
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

            AppConfig config;
            if (!File.Exists(configPath))
            {
                // 如果文件不存在，创建默认配置
                config = new AppConfig
                {
                    BaseApi = "http://13313777163.kmdns.net:8090/officeAutomation",
                    MinioAddress =  "http://13313777163.kmdns.net:9000"
                };
                // 序列化并写入文件
                string jsonContent = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, jsonContent);
            }
            else
            {
                // 读取现有文件
                string jsonContent = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<AppConfig>(jsonContent);

                // 检查是否反序列化成功
                if (config == null)
                {
                    MessageBox.Show("配置文件内容无效！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                    return;
                }
            }

            // 赋值到全局变量
            GlobalConfig.BaseApi = config.BaseApi;
            GlobalConfig.MinioAddress = config.MinioAddress;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"加载或创建配置文件失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
}