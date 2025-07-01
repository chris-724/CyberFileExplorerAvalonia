using Avalonia.Media;

namespace CyberFileExplorerAvalonia.Models;

public class FileItem
{
    public string Name { get; set; }
    public string Extension { get; set; }
    public IBrush Color { get; set; }
}
