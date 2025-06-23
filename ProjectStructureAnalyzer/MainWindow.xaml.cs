using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;

namespace ProjectStructureAnalyzer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly ObservableCollection<ProjectItem> projectItems;
        private string selectedPath;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            projectItems = new ObservableCollection<ProjectItem>();
            ProjectTreeView.ItemsSource = projectItems;

            // Загрузка последней выбранной директории
            SelectedPath = Properties.Settings.Default.LastSelectedPath;
            if (!string.IsNullOrEmpty(SelectedPath) && Directory.Exists(SelectedPath))
            {
                AnalyzeButton.IsEnabled = true;
                StatusText.Text = "Последняя папка загружена. Нажмите 'Анализировать'.";
            }
            else
            {
                SelectedPath = "";
                AnalyzeButton.IsEnabled = false;
                StatusText.Text = "Выберите папку для анализа.";
            }

            DataContext = this;
        }

        public string SelectedPath
        {
            get => selectedPath;
            set
            {
                if (selectedPath != value)
                {
                    selectedPath = value;
                    OnPropertyChanged(nameof(SelectedPath));
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                SelectedPath = dialog.SelectedPath;
                AnalyzeButton.IsEnabled = true;
                StatusText.Text = "Папка выбрана. Нажмите 'Анализировать' для начала анализа.";

                Properties.Settings.Default.LastSelectedPath = SelectedPath;
                Properties.Settings.Default.Save();
            }
        }

        private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedPath) || !Directory.Exists(SelectedPath))
            {
                MessageBox.Show("Выберите корректную папку для анализа.");
                return;
            }

            try
            {
                StatusText.Text = "Анализ структуры проекта...";
                projectItems.Clear();
                VisualizationCanvas.Children.Clear();

                var rootItem = await AnalyzeDirectoryAsync(SelectedPath);
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
                Children = new ObservableCollection<ProjectItem>(),
                IsUserFolder = !IsSystemFolder(dirInfo.Name)
            };

            var folderFilters = (Properties.Settings.Default.FolderFilters ?? "").Split(',').Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f)).ToList();
            var fileFilters = (Properties.Settings.Default.FileFilters ?? "").Split(',').Select(f => f.Trim().ToLower()).Where(f => !string.IsNullOrEmpty(f)).ToList();

            try
            {
                foreach (var dir in dirInfo.GetDirectories().Where(d => !d.Attributes.HasFlag(FileAttributes.Hidden)))
                {
                    if (folderFilters.Count == 0 || folderFilters.Contains(dir.Name))
                    {
                        var childItem = await AnalyzeDirectoryAsync(dir.FullName);
                        if (childItem != null)
                        {
                            item.Children.Add(childItem);
                        }
                    }
                }

                foreach (var file in dirInfo.GetFiles().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)))
                {
                    if (fileFilters.Count == 0 || fileFilters.Contains(file.Extension.ToLower()))
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
                }

                item.FileCount = CountFiles(item);
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }

            return item;
        }

        private bool IsSystemFolder(string folderName)
        {
            string[] systemFolders = { "bin", "obj", "Debug", "Release", ".vs", "packages" };
            return systemFolders.Contains(folderName.ToLower());
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
            var levelHeight = 30;

            DrawProjectStructure(rootItem, startX, startY, 0, levelHeight);
        }

        private double DrawProjectStructure(ProjectItem item, double x, double y, int level, double levelHeight)
        {
            var nodeWidth = 150;
            var nodeHeight = 20;
            var spacing = 20;

            // Иконка для папки или файла
            string iconPath = item.IsDirectory ? "/Images/folder.png" : "/Images/file.png";
            var image = new Image
            {
                Width = 16,
                Height = 16,
                Source = LoadImage(iconPath)
            };
            Canvas.SetLeft(image, x);
            Canvas.SetTop(image, y);
            VisualizationCanvas.Children.Add(image);

            var text = new TextBlock
            {
                Text = item.Name,
                FontSize = 12,
                Margin = new Thickness(20, 0, 0, 0)
            };
            Canvas.SetLeft(text, x);
            Canvas.SetTop(text, y - 5);
            VisualizationCanvas.Children.Add(text);

            var currentX = x;
            var childY = y + levelHeight;

            if (item.IsDirectory && item.Children.Count > 0)
            {
                var verticalLine = new Line
                {
                    X1 = x + 8,
                    Y1 = y + nodeHeight,
                    X2 = x + 8,
                    Y2 = childY + (item.Children.Count * levelHeight) - 5,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                VisualizationCanvas.Children.Add(verticalLine);

                foreach (var child in item.Children)
                {
                    var horizontalLine = new Line
                    {
                        X1 = x + 8,
                        Y1 = childY - 5,
                        X2 = currentX + 8,
                        Y2 = childY - 5,
                        Stroke = Brushes.LightGray,
                        StrokeThickness = 1
                    };
                    VisualizationCanvas.Children.Add(horizontalLine);

                    DrawProjectStructure(child, currentX + 20, childY, level + 1, levelHeight);
                    currentX += nodeWidth + spacing;
                    childY += levelHeight;
                }
            }

            return Math.Max(currentX, x + nodeWidth);
        }

        private BitmapImage LoadImage(string path)
        {
            try
            {
                return new BitmapImage(new Uri(path, UriKind.Relative));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения {path}: {ex.Message}");
                return new BitmapImage();
            }
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

            int count = 1;
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
                StatusText.Text = $"Выбран: {selectedItem.Name}";
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
                        writer.WriteLine($"Структура проекта: {SelectedPath}");
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

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
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
        public bool IsUserFolder { get; set; }
    }
}