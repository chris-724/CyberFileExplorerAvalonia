using System.Collections.ObjectModel;

namespace CyberFileExplorerAvalonia.Models;

public class Tower
{
    public string DirectoryName { get; set; }
    public ObservableCollection<FileItem> Files { get; set; } = new();
}
