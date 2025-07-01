using System.Collections.ObjectModel;

namespace CyberFileExplorerAvalonia.ViewModels

{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _currentPath = "This PC";

        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                if (_currentPath != value)
                {
                    _currentPath = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
