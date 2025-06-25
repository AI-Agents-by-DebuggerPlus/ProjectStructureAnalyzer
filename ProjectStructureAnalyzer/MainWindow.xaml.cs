using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Forms = System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Data;

namespace ProjectStructureAnalyzer
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // ... (весь код класса MainWindow остается без изменений) ...
        // ... (методы, конструктор, обработчики событий и т.д.) ...

        private readonly ObservableCollection<ProjectItem> projectItems;
        private string selectedPath;
        private readonly string logFilePath = "structure_log.txt";
        private readonly DirectoryAnalyzer directoryAnalyzer = new DirectoryAnalyzer();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(logFilePath))
            {
                File.Delete(logFilePath);
            }
            using (StreamWriter writer = File.CreateText(logFilePath))
            {
                writer.WriteLine($"Log started at {DateTime.Now}");
            }

            ApplySettings();

            projectItems = new ObservableCollection<ProjectItem>();
            ProjectTreeView.ItemsSource = projectItems;

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

        private void ApplySettings()
        {
            Log("--- Applying settings on MainWindow ---");
            try
            {
                string fontFamilyName = Properties.Settings.Default.ApplicationFontFamily;
                double fontSize = Properties.Settings.Default.ApplicationFontSize;

                Log($"Read settings: FontFamily='{fontFamilyName}', FontSize='{fontSize}'");

                if (!string.IsNullOrEmpty(fontFamilyName))
                {
                    this.FontFamily = new FontFamily(fontFamilyName);
                    Log($"Applied FontFamily: {fontFamilyName}");
                }
                else
                {
                    Log("FontFamily setting is empty or null. Skipping apply.");
                }

                if (fontSize > 0)
                {
                    this.FontSize = fontSize;
                    Log($"Applied FontSize: {fontSize}");
                }
                else
                {
                    Log("FontSize setting is zero or less. Skipping apply.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error applying settings: {ex.Message}");
            }
            Log("--- Finished applying settings ---");
        }

        protected void OnPropertyChanged(string propertyName)
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

                var rootItem = await directoryAnalyzer.AnalyzeDirectoryAsync(SelectedPath, SelectedPath);
                if (rootItem != null)
                {
                    projectItems.Add(rootItem);
                    UpdateStatistics();
                    ProjectTreeView.UpdateLayout();
                    ExpandTreeViewItems(ProjectTreeView);
                    ExportButton.IsEnabled = true;
                    StatusText.Text = "Анализ завершен успешно.";
                }
                else
                {
                    StatusText.Text = "Нет данных для отображения согласно фильтрам.";
                    Log("No items to display based on filters.");
                }
                Log("");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при анализе: {ex.Message}");
                StatusText.Text = "Ошибка при анализе.";
                Log($"Error during analysis: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            try
            {
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log error: {ex.Message}");
            }
        }

        private void UpdateStatistics()
        {
            if (projectItems.Count > 0)
            {
                var totalFiles = CountFiles(projectItems[0]);
                var totalFolders = CountFolders(projectItems[0]) - 1;

                FileCountText.Text = totalFiles.ToString();
                FolderCountText.Text = totalFolders.ToString();
            }
        }

        private int CountFiles(ProjectItem item)
        {
            int count = item.IsDirectory ? 0 : 1;
            foreach (var child in item.Children)
            {
                count += CountFiles(child);
            }
            return count;
        }

        private int CountFolders(ProjectItem item)
        {
            int count = item.IsDirectory ? 1 : 0;
            foreach (var child in item.Children)
            {
                count += CountFolders(child);
            }
            return count;
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024.0):F2} MB";
            return $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB";
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
            if (projectItems.Count == 0 || string.IsNullOrEmpty(SelectedPath))
            {
                MessageBox.Show("Сначала выполните анализ проекта.");
                return;
            }

            var projectName = new DirectoryInfo(SelectedPath).Name;
            var saveDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"{projectName}_structure.txt"
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

                    ExportStatusText.Text = $"Файл структуры сохранен:\n{saveDialog.FileName}";
                    ExportStatusText.Visibility = Visibility.Visible;
                    ExportStatusText.Foreground = new SolidColorBrush(Colors.Green);
                    StatusText.Text = "Экспорт завершен успешно.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}");
                    ExportStatusText.Text = $"Ошибка экспорта: {ex.Message}";
                    ExportStatusText.Foreground = new SolidColorBrush(Colors.Red);
                    ExportStatusText.Visibility = Visibility.Visible;
                }
            }
        }

        private void ExportProjectStructure(StreamWriter writer, ProjectItem item, int level)
        {
            var indent = new string(' ', level * 2);
            var prefix = item.IsDirectory ? "📁" : "📄";
            var info = item.IsDirectory ? $"({item.FileCount} файлов)" : $"({FormatFileSize(item.Size)})";
            writer.WriteLine($"{indent}{prefix} {item.Name} {info}");

            foreach (var child in item.Children.OrderBy(c => !c.IsDirectory).ThenBy(c => c.Name))
            {
                ExportProjectStructure(writer, child, level + 1);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                ApplySettings();
            }
        }

        private void ExpandTreeViewItems(ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {
                if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
                {
                    treeViewItem.IsExpanded = true;
                    ExpandTreeViewItems(treeViewItem);
                }
            }
        }

        private void ExpandTreeViewItems(TreeViewItem item)
        {
            item.IsExpanded = true;
            foreach (var subItem in item.Items)
            {
                if (item.ItemContainerGenerator.ContainerFromItem(subItem) is TreeViewItem subTreeViewItem)
                {
                    ExpandTreeViewItems(subTreeViewItem);
                }
            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Конвертер для отображения иконок
    public class DirectoryToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool isDirectory && isDirectory)
                return "📁";
            return "📄";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // --- ИЗМЕНЕНО: Возвращены недостающие свойства ---
    public class ProjectItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public long Size { get; set; }
        public string Extension { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public ObservableCollection<ProjectItem> Children { get; set; } = new ObservableCollection<ProjectItem>();
        public bool IsUserFolder { get; set; } // <-- ВОЗВРАЩЕНО
        public bool IsExpanded { get; set; }   // <-- ВОЗВРАЩЕНО
    }
}