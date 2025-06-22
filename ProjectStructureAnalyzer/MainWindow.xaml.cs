using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;

namespace ProjectStructureAnalyzer
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ProjectItem> projectItems;
        private string selectedPath;

        public MainWindow()
        {
            InitializeComponent();
            projectItems = new ObservableCollection<ProjectItem>();
            ProjectTreeView.ItemsSource = projectItems;

            // Загрузка последней выбранной директории
            selectedPath = Properties.Settings.Default.LastSelectedPath;
            if (!string.IsNullOrEmpty(selectedPath) && Directory.Exists(selectedPath))
            {
                PathTextBox.Text = selectedPath;
                AnalyzeButton.IsEnabled = true;
                StatusText.Text = "Последняя папка загружена. Нажмите 'Анализировать'.";
            }
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                selectedPath = dialog.SelectedPath;
                PathTextBox.Text = selectedPath;
                AnalyzeButton.IsEnabled = true;
                StatusText.Text = "Папка выбрана. Нажмите 'Анализировать' для начала анализа.";

                // Сохранение последней выбранной директории
                Properties.Settings.Default.LastSelectedPath = selectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                MessageBox.Show("Выберите корректную папку для анализа.");
                return;
            }

            try
            {
                StatusText.Text = "Анализ структуры проекта...";
                projectItems.Clear();
                VisualizationCanvas.Children.Clear();

                var rootItem = await AnalyzeDirectoryAsync(selectedPath);
                if (rootItem != null)
                {
                    projectItems.Add(rootItem);
                    CreateVisualization(rootItem);
                    UpdateStatistics();
                    ExportButton.IsEnabled = true;
                    StatusText.Text = "Анализ завершен успешно.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при анализе: {ex.Message}");
                StatusText.Text = "Ошибка при анализе.";
            }
        }

        private async Task<ProjectItem> AnalyzeDirectoryAsync(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            var item = new ProjectItem
            {
                Name = dirInfo.Name,
                FullPath = path,
                IsDirectory = true,
                Children = new ObservableCollection<ProjectItem>()
            };

            try
            {
                foreach (var dir in dirInfo.GetDirectories().Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden)))
                {
                    var childItem = await AnalyzeDirectoryAsync(dir.FullName);
                    if (childItem != null)
                    {
                        item.Children.Add(childItem);
                    }
                }

                foreach (var file in dirInfo.GetFiles().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)))
                {
                    item.Children.Add(new ProjectItem
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        IsDirectory = false,
                        Size = file.Length,
                        Extension = file.Extension.ToLower()
                    });
                }

                item.FileCount = CountFiles(item);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }

            return item;
        }

        private int CountFiles(ProjectItem item)
        {
            if (!item.IsDirectory) return 1;

            int count = 0;
            foreach (var child in item.Children)
            {
                count += CountFiles(child);
            }
            return count;
        }

        private void CreateVisualization(ProjectItem rootItem)
        {
            VisualizationCanvas.Children.Clear();

            var startX = 50;
            var startY = 50;
            var levelHeight = 100;

            DrawProjectStructure(rootItem, startX, startY, 0, levelHeight);
        }

        private double DrawProjectStructure(ProjectItem item, double x, double y, int level, double levelHeight)
        {
            var nodeWidth = 120;
            var nodeHeight = 60;
            var spacing = 20;

            Brush fillBrush = item.IsDirectory ?
                new SolidColorBrush(Colors.LightSkyBlue) :
                new SolidColorBrush(Colors.LightPink);

            var rect = new Rectangle
            {
                Width = nodeWidth,
                Height = nodeHeight,
                Fill = fillBrush,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                RadiusX = 5,
                RadiusY = 5
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            VisualizationCanvas.Children.Add(rect);

            var text = new TextBlock
            {
                Text = item.Name.Length > 15 ? item.Name.Substring(0, 12) + "..." : item.Name,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Width = nodeWidth,
                TextWrapping = TextWrapping.Wrap
            };

            Canvas.SetLeft(text, x);
            Canvas.SetTop(text, y + 10);
            VisualizationCanvas.Children.Add(text);

            if (item.IsDirectory && item.FileCount > 0)
            {
                var countText = new TextBlock
                {
                    Text = $"({item.FileCount} файлов)",
                    FontSize = 8,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    Width = nodeWidth
                };
                Canvas.SetLeft(countText, x);
                Canvas.SetTop(countText, y + 35);
                VisualizationCanvas.Children.Add(countText);
            }
            else if (!item.IsDirectory)
            {
                var sizeText = new TextBlock
                {
                    Text = FormatFileSize(item.Size),
                    FontSize = 8,
                    Foreground = Brushes.Gray,
                    TextAlignment = TextAlignment.Center,
                    Width = nodeWidth
                };
                Canvas.SetLeft(sizeText, x);
                Canvas.SetTop(sizeText, y + 35);
                VisualizationCanvas.Children.Add(sizeText);
            }

            var currentX = x;
            var childY = y + levelHeight;

            if (item.IsDirectory && item.Children.Count > 0)
            {
                var childrenToShow = item.Children.Take(10).ToList();

                foreach (var child in childrenToShow)
                {
                    var line = new Line
                    {
                        X1 = x + nodeWidth / 2,
                        Y1 = y + nodeHeight,
                        X2 = currentX + nodeWidth / 2,
                        Y2 = childY,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 1
                    };
                    VisualizationCanvas.Children.Add(line);

                    DrawProjectStructure(child, currentX, childY, level + 1, levelHeight);
                    currentX += nodeWidth + spacing;
                }

                if (item.Children.Count > 10)
                {
                    var moreRect = new Rectangle
                    {
                        Width = nodeWidth,
                        Height = nodeHeight / 2,
                        Fill = Brushes.LightGray,
                        Stroke = Brushes.Gray,
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 5, 5 }
                    };
                    Canvas.SetLeft(moreRect, currentX);
                    Canvas.SetTop(moreRect, childY + 15);
                    VisualizationCanvas.Children.Add(moreRect);

                    var moreText = new TextBlock
                    {
                        Text = $"... еще {item.Children.Count - 10}",
                        FontSize = 9,
                        FontStyle = FontStyles.Italic,
                        TextAlignment = TextAlignment.Center,
                        Width = nodeWidth
                    };
                    Canvas.SetLeft(moreText, currentX);
                    Canvas.SetTop(moreText, childY + 20);
                    VisualizationCanvas.Children.Add(moreText);
                }
            }

            return Math.Max(currentX, x + nodeWidth);
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024 * 1024)} MB";
            return $"{bytes / (1024 * 1024 * 1024)} GB";
        }

        private void UpdateStatistics()
        {
            if (projectItems.Count > 0)
            {
                var totalFiles = CountFiles(projectItems[0]);
                var totalFolders = CountFolders(projectItems[0]);

                FileCountText.Text = totalFiles.ToString();
                FolderCountText.Text = totalFolders.ToString();
            }
        }

        private int CountFolders(ProjectItem item)
        {
            if (!item.IsDirectory) return 0;

            int count = 1; // Сама папка
            foreach (var child in item.Children)
            {
                if (child.IsDirectory)
                    count += CountFolders(child);
            }
            return count;
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem treeViewItem && treeViewItem.DataContext is ProjectItem selectedItem)
            {
                StatusText.Text = $"Выбран: {selectedItem.FullPath}";
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = "project_structure.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new StreamWriter(saveDialog.FileName))
                    {
                        writer.WriteLine($"Структура проекта: {selectedPath}");
                        writer.WriteLine($"Дата анализа: {DateTime.Now}");
                        writer.WriteLine(new string('=', 50));

                        if (projectItems.Count > 0)
                        {
                            ExportProjectStructure(writer, projectItems[0], 0);
                        }
                    }

                    MessageBox.Show("Структура проекта успешно экспортирована!");
                    StatusText.Text = "Экспорт завершен успешно.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}");
                }
            }
        }

        private void ExportProjectStructure(StreamWriter writer, ProjectItem item, int level)
        {
            var indent = new string(' ', level * 2);
            var prefix = item.IsDirectory ? "📁" : "📄";
            var info = item.IsDirectory ? $" ({item.FileCount} файлов)" : $" ({FormatFileSize(item.Size)})";

            writer.WriteLine($"{indent}{prefix} {item.Name}{info}");

            foreach (var child in item.Children)
            {
                ExportProjectStructure(writer, child, level + 1);
            }
        }

        private void VisualizationScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                var scaleTransform = VisualizationCanvas.RenderTransform as ScaleTransform ?? new ScaleTransform();
                var scale = scaleTransform.ScaleX;
                scale += e.Delta > 0 ? 0.1 : -0.1;
                scale = Math.Max(0.1, Math.Min(2.0, scale));
                scaleTransform.ScaleX = scale;
                scaleTransform.ScaleY = scale;
            }
        }
    }

    public class ProjectItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public string Extension { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public ObservableCollection<ProjectItem> Children { get; set; } = new ObservableCollection<ProjectItem>();
    }
}