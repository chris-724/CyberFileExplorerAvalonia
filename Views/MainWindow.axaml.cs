using Avalonia.Controls;
using Avalonia.Interactivity;
using CyberFileExplorerAvalonia.Engine;
using CyberFileExplorerAvalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;

namespace CyberFileExplorerAvalonia.Views
{
    public partial class MainWindow : Window
    {
        private readonly Stack<string> _navigationHistory = new();
        private readonly MainWindowViewModel _viewModel;
        private SceneView? _sceneView;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            // Safely assign scene and back button handlers
            _sceneView = this.FindControl<SceneView>("SceneCanvas");

            var backButton = this.FindControl<Button>("BackButton");
            if (backButton is not null)
                backButton.Click += OnBackClicked;

            // Initialize to drive view
            LoadPath(null);
        }

        private void LoadPath(string? path)
        {
            if (_sceneView is null)
                return;

            if (string.IsNullOrWhiteSpace(path))
            {
                _viewModel.CurrentPath = "This PC";
                _sceneView.ShowTowersForPath(""); // Show drives
                return;
            }

            if (!Directory.Exists(path))
            {
                _viewModel.CurrentPath = "Access Denied";
                _sceneView.ShowTowersForPath(""); // Fallback to drive view
                return;
            }

            _viewModel.CurrentPath = path;
            _sceneView.ShowTowersForPath(path);
        }

        private void OnBackClicked(object? sender, RoutedEventArgs e)
        {
            if (_navigationHistory.Count > 0)
            {
                var previousPath = _navigationHistory.Pop();
                LoadPath(previousPath);
            }
            else
            {
                LoadPath(null);
            }
        }

        public void NavigateTo(string targetPath)
        {
            if (!string.IsNullOrWhiteSpace(_viewModel.CurrentPath) &&
                Directory.Exists(_viewModel.CurrentPath))
            {
                _navigationHistory.Push(_viewModel.CurrentPath);
            }

            LoadPath(targetPath);
        }
    }
}
