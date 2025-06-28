using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Forms;

namespace ProjectStructureAnalyzer
{
    public class MainViewModel : ObservableObject
    {
        private readonly DirectoryAnalyzer directoryAnalyzer = new DirectoryAnalyzer();
        private ObservableCollection<ProjectItem> projectItems;
        private string? selectedPath;
        private string? statusText;
        private string? exportStatusText;
        private Visibility exportStatusVisibility;
        private bool analyzeButtonEnabled;
        private bool exportButtonEnabled;
        private int folderCount;
        private int fileCount;
        private Brush? exportStatusTextForeground;

        // Добавляем событие для отображения подсказки
        public event Action ShowFolderSelectionHint;

        public MainViewModel()
        {
            projectItems = new ObservableCollection<ProjectItem>();
            SelectedPath = Properties.Settings.Default.LastSelectedPath;
            if (!string.IsNullOrEmpty(SelectedPath) && Directory.Exists(SelectedPath))
            {
                AnalyzeButtonEnabled = true;
                StatusText = "Последняя папка загружена.\nНажмите 'Анализировать'.";
            }
            else
            {
                SelectedPath = "";
                AnalyzeButtonEnabled = false;
                StatusText = "Выберите папку для анализа.";
            }
            ExportStatusVisibility = Visibility.Collapsed;
        }

        public ObservableCollection<ProjectItem> ProjectItems
        {
            get => projectItems;
            private set => SetProperty(ref projectItems, value);
        }

        public string? SelectedPath
        {
            get => selectedPath;
            set => SetProperty(ref selectedPath, value, nameof(SelectedPath));
        }

        public string? StatusText
        {
            get => statusText;
            set => SetProperty(ref statusText, value);
        }

        public string? ExportStatusText
        {
            get => exportStatusText;
            set => SetProperty(ref exportStatusText, value);
        }

        public Visibility ExportStatusVisibility
        {
            get => exportStatusVisibility;
            set => SetProperty(ref exportStatusVisibility, value);
        }

        public bool AnalyzeButtonEnabled
        {
            get => analyzeButtonEnabled;
            set => SetProperty(ref analyzeButtonEnabled, value);
        }

        public bool ExportButtonEnabled
        {
            get => exportButtonEnabled;
            set => SetProperty(ref exportButtonEnabled, value);
        }

        public int FolderCount
        {
            get => folderCount;
            set => SetProperty(ref folderCount, value);
        }

        public int FileCount
        {
            get => fileCount;
            set => SetProperty(ref fileCount, value);
        }

        public Brush? ExportStatusTextForeground
        {
            get => exportStatusTextForeground;
            set => SetProperty(ref exportStatusTextForeground, value);
        }

        public ICommand SelectFolderCommand => new RelayCommand(SelectFolder);

        public ICommand AnalyzeCommand => new AsyncRelayCommand(AnalyzeAsync);

        public ICommand ExportCommand => new RelayCommand(Export);

        public ICommand SettingsCommand => new RelayCommand(Settings);

        private void SelectFolder()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SelectedPath = dialog.SelectedPath;
                AnalyzeButtonEnabled = true;
                StatusText = "Папка выбрана. Нажмите 'Анализировать'.";
                Logger.LogInfo($"Selected folder: {dialog.SelectedPath}");

                try
                {
                    Properties.Settings.Default.LastSelectedPath = SelectedPath;
                    Properties.Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error saving selected path", ex);
                    System.Windows.MessageBox.Show("Ошибка при сохранении выбранного пути.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task AnalyzeAsync()
        {
            // Проверяем, выбрана ли папка
            if (string.IsNullOrEmpty(SelectedPath))
            {
                // Вызываем событие для отображения подсказки
                ShowFolderSelectionHint?.Invoke();
                StatusText = "Сначала выберите папку для анализа.";
                return;
            }

            if (!Directory.Exists(SelectedPath))
            {
                System.Windows.MessageBox.Show("Выберите корректную папку для анализа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                StatusText = "Анализ структуры проекта...";
                ProjectItems.Clear();

                var rootItem = await directoryAnalyzer.AnalyzeDirectoryAsync(SelectedPath, SelectedPath);
                if (rootItem != null)
                {
                    ProjectItems.Add(rootItem);
                    UpdateStatistics();
                    ExportButtonEnabled = true;
                    StatusText = $"Анализ завершен. Путь: {SelectedPath}";
                }
                else
                {
                    StatusText = "Нет данных для отображения согласно фильтрам.";
                    Logger.LogInfo("No items to display based on filters.");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при анализе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Ошибка при анализе.";
                Logger.LogError("Error during analysis", ex);
            }
        }

        private void Export()
        {
            if (ProjectItems.Count == 0 || string.IsNullOrEmpty(SelectedPath))
            {
                System.Windows.MessageBox.Show("Сначала выполните анализ проекта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var projectName = new DirectoryInfo(SelectedPath).Name;
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"{projectName}_structure.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    ExportService.ExportProjectStructure(saveDialog.FileName, ProjectItems[0], SelectedPath);
                    ExportStatusText = $"Файл структуры сохранен:\n{saveDialog.FileName}";
                    ExportStatusVisibility = Visibility.Visible;
                    ExportStatusTextForeground = new SolidColorBrush(Colors.Green);
                    StatusText = "Экспорт завершен успешно.";
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    ExportStatusText = $"Ошибка экспорта: {ex.Message}";
                    ExportStatusTextForeground = new SolidColorBrush(Colors.Red);
                    ExportStatusVisibility = Visibility.Visible;
                }
            }
        }

        private void Settings()
        {
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWindow)
            {
                var settingsWindow = new SettingsWindow(mainWindow);
                settingsWindow.ShowDialog();
            }
            else
            {
                Logger.LogError("MainWindow is not available.", new InvalidOperationException("MainWindow is null."));
                System.Windows.MessageBox.Show("Ошибка: Главное окно недоступно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            if (ProjectItems.Count > 0)
            {
                FileCount = ProjectItems[0].FileCount;
                FolderCount = CountFolders(ProjectItems[0]) - 1;
            }
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
    }
}