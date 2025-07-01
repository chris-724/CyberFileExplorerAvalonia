using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace CyberFileExplorerAvalonia.Engine
{
    public class SceneView : Control
    {
        private readonly List<TowerVisual> _towers = new();

        private double _cameraX = 0;
        private double _cameraZ = -500;
        private double _cameraYaw = 0;

        private TowerVisual? _selectedTower;

        private const double Fov = 800;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            const double moveSpeed = 20;
            const double turnSpeed = 0.05;

            switch (e.Key)
            {
                case Key.W:
                    _cameraX += Math.Sin(_cameraYaw) * moveSpeed;
                    _cameraZ += Math.Cos(_cameraYaw) * moveSpeed;
                    break;
                case Key.S:
                    _cameraX -= Math.Sin(_cameraYaw) * moveSpeed;
                    _cameraZ -= Math.Cos(_cameraYaw) * moveSpeed;
                    break;
                case Key.A:
                    _cameraX -= Math.Cos(_cameraYaw) * moveSpeed;
                    _cameraZ += Math.Sin(_cameraYaw) * moveSpeed;
                    break;
                case Key.D:
                    _cameraX += Math.Cos(_cameraYaw) * moveSpeed;
                    _cameraZ -= Math.Sin(_cameraYaw) * moveSpeed;
                    break;
                case Key.Left:
                    _cameraYaw -= turnSpeed;
                    break;
                case Key.Right:
                    _cameraYaw += turnSpeed;
                    break;
            }

            InvalidateVisual();
            base.OnKeyDown(e);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            var point = e.GetPosition(this);
            if (_selectedTower != null)
                _selectedTower.IsSelected = false;

            _selectedTower = HitTest(point);
            if (_selectedTower != null)
                _selectedTower.IsSelected = true;

            InvalidateVisual();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            var point = e.GetPosition(this);
            if (HitTest(point) == null && _selectedTower != null)
            {
                _selectedTower.IsSelected = false;
                _selectedTower = null;
                InvalidateVisual();
            }
            else
            {
                InvalidateVisual();
            }
        }

        // This method will show towers based on the path (drives, directories, or access denied).
        public void ShowTowersForPath(string? path)
        {
            _towers.Clear();
            _selectedTower = null;

            double canvasWidth = Bounds.Width;
            if (canvasWidth <= 0) canvasWidth = 1600;

            // If no path is provided, show available drives
            if (string.IsNullOrWhiteSpace(path))
            {
                var drives = DriveInfo.GetDrives();
                int count = drives.Length;
                float startX = -(count - 1) * 100f;

                for (int i = 0; i < count; i++)
                {
                    _towers.Add(new TowerVisual
                    {
                        Label = drives[i].Name,
                        X = startX + i * 200,
                        Z = 0,
                        IsDirectory = true,
                        IsAccessDenied = false
                    });
                }
            }
            // If a valid directory path is given, show directories
            else if (Directory.Exists(path))
            {
                var dirs = Directory.GetDirectories(path);
                int count = dirs.Length;
                float startX = -(count - 1) * 100f;

                for (int i = 0; i < count; i++)
                {
                    _towers.Add(new TowerVisual
                    {
                        Label = Path.GetFileName(dirs[i]),
                        X = startX + i * 200,
                        Z = 0,
                        IsDirectory = true,
                        IsAccessDenied = false
                    });
                }
            }
            // If path is invalid, show access denied
            else
            {
                _towers.Add(new TowerVisual
                {
                    Label = "Access Denied",
                    X = 0,
                    Z = 0,
                    IsDirectory = false,
                    IsAccessDenied = true
                });
            }

            // Trigger a redraw after updating the towers list
            InvalidateVisual();
        }

        // This method handles the actual rendering of towers and labels
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            context.DrawRectangle(Brushes.Black, null, bounds);

            foreach (var tower in _towers)
            {
                var proj = Project(tower.X, tower.Z);
                if (proj == null)
                    continue;

                var (pos, scale) = proj.Value;
                double width = 100 * scale;
                double height = 150 * scale;
                var rect = new Rect(pos.X - width / 2, bounds.Center.Y - height, width, height);

                var fill = tower.IsAccessDenied ? Brushes.Red :
                           tower.IsDirectory ? Brushes.Cyan : Brushes.Gray;

                if (tower.IsSelected)
                    fill = Brushes.Lime;

                context.DrawRectangle(fill, new Pen(Brushes.White, 1), rect);

                var formattedText = new FormattedText(
                    tower.Label,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    14 * scale,
                    tower.IsSelected ? Brushes.Yellow : Brushes.White);

                double approxWidth = tower.Label.Length * 7 * scale;
                double textX = rect.X + rect.Width / 2 - approxWidth / 2;
                double textY = rect.Y - 20 * scale;
                context.DrawText(formattedText, new Point(textX, textY));
            }
        }

        private (Point point, double scale)? Project(double x, double z)
        {
            double dx = x - _cameraX;
            double dz = z - _cameraZ;

            double sin = Math.Sin(_cameraYaw);
            double cos = Math.Cos(_cameraYaw);

            double px = dx * cos - dz * sin;
            double pz = dx * sin + dz * cos;

            if (pz <= 1)
                return null;

            double scale = Fov / pz;
            double sx = Bounds.Width / 2 + px * scale;
            return (new Point(sx, 0), scale);
        }

        private TowerVisual? HitTest(Point point)
        {
            foreach (var tower in _towers)
            {
                var proj = Project(tower.X, tower.Z);
                if (proj == null)
                    continue;

                var (pos, scale) = proj.Value;
                double width = 100 * scale;
                double height = 150 * scale;
                var rect = new Rect(pos.X - width / 2, Bounds.Center.Y - height, width, height);
                if (rect.Contains(point))
                    return tower;
            }
            return null;
        }

        // This class represents a tower (drive or directory)
        private class TowerVisual
        {
            public string Label { get; set; } = string.Empty; // Label for the tower (file or folder name)
            public float X { get; set; } // X-coordinate of the tower's position
            public float Z { get; set; } // Z-coordinate of the tower's position
            public bool IsDirectory { get; set; } // Whether the tower is a directory
            public bool IsAccessDenied { get; set; } // Whether access to the tower was denied (for "Access Denied")
            public bool IsSelected { get; set; }
        }
    }
}
