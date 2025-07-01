using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;
using System.IO;
using System.Globalization;

namespace CyberFileExplorerAvalonia.Engine
{
    public class SceneView : Control
    {
        private List<TowerVisual> _towers = new();

        // This method will show towers based on the path (drives, directories, or access denied).
        public void ShowTowersForPath(string? path)
        {
            _towers.Clear();

            double canvasWidth = Bounds.Width;
            if (canvasWidth <= 0) canvasWidth = 1600; // fallback default to 1600px if no width is set

            // If no path is provided, show available drives
            if (string.IsNullOrWhiteSpace(path))
            {
                var drives = DriveInfo.GetDrives();
                int count = drives.Length;
                float startX = (float)((canvasWidth - (count * 120)) / 2); // center drives horizontally

                for (int i = 0; i < count; i++)
                {
                    _towers.Add(new TowerVisual
                    {
                        Label = drives[i].Name,
                        X = startX + i * 120,
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
                float startX = (float)((canvasWidth - (count * 120)) / 2); // center directories horizontally

                for (int i = 0; i < count; i++)
                {
                    _towers.Add(new TowerVisual
                    {
                        Label = Path.GetFileName(dirs[i]),
                        X = startX + i * 120,
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
                    X = (float)(canvasWidth / 2 - 60), // center "Access Denied" label
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
            context.DrawRectangle(Brushes.Black, null, bounds); // Draw background as black

            // Iterate through all towers and draw them
            foreach (var tower in _towers)
            {
                var rect = new Rect(tower.X, 200, 100, 150); // Tower rectangle size and position
                var fill = tower.IsAccessDenied ? Brushes.Red : // Red for access denied
                           tower.IsDirectory ? Brushes.Cyan : // Cyan for directories
                           Brushes.Gray; // Gray for other files

                // Draw the tower rectangle
                context.DrawRectangle(fill, new Pen(Brushes.White, 1), rect);

                // Hardcoded max width for text
                double maxTextWidth = 100;

                // Create the text to display (formatted)
                var formattedText = new FormattedText(
                    tower.Label, // Text to display (folder/file name)
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Arial"), // Font used
                    14, // Font size
                    Brushes.White); // Text color

                // Center text within the tower using hardcoded width
                double textX = tower.X + 50;  // Center the text within the tower (max 100px width)
                double textY = 360; // Fixed Y-position for text below tower

                // Draw the text at calculated position
                context.DrawText(formattedText, new Point(textX, textY));
            }
        }

        // This class represents a tower (drive or directory)
        private class TowerVisual
        {
            public string Label { get; set; } = string.Empty; // Label for the tower (file or folder name)
            public float X { get; set; } // X-coordinate of the tower's position
            public bool IsDirectory { get; set; } // Whether the tower is a directory
            public bool IsAccessDenied { get; set; } // Whether access to the tower was denied (for "Access Denied")
        }
    }
}
