using System.Windows;
using System.Windows.Controls;

namespace OA_Downloader;

public class FolderImageTemplateSelector : DataTemplateSelector
{
    public DataTemplate FolderTemplate { get; set; }
    public DataTemplate ImageTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is Folder)
        {
            return FolderTemplate;
        }
        else if (item is ImageInfo)
        {
            return ImageTemplate;
        }
        return base.SelectTemplate(item, container);
    }
}
