using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace OA_Downloader;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 启用数据绑定错误的调试输出
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
    }
}