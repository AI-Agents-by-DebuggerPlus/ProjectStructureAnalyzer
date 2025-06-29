using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectStructureAnalyzer
{
    public class MainViewModel : ObservableObject
    {
        private readonly DirectoryAnalyzer directoryAnalyzer = new DirectoryAnalyzer();
        private readonly JsonSettingsManager settingsManager;
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
        private List<string> folderFilters = new List<string>();
        private List<string> fileFilters = new List<string>();

        public event Action ShowFolderSelectionHint;

        public MainViewModel()
        {
            settingsManager = new JsonSettingsManager();
            projectItems = new ObservableCollection<ProjectItem>();
            LoadSettingsFromJson();
            InitializeViewModel();
        }

        #region Properties

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

        public List<string> FolderFilters
        {
            get => folderFilters;
            set => SetProperty(ref folderFilters, value);
        }

        public List<string> FileFilters
        {
            get => fileFilters;
            set => SetProperty(ref fileFilters, value);
        }

        public AppSettings AppSettings => settingsManager.Settings;

        #endregion

        #region Commands

        public ICommand SelectFolderCommand => new RelayCommand(SelectFolder);
        public ICommand AnalyzeCommand => new AsyncRelayCommand(AnalyzeAsync);
        public ICommand ExportCommand => new RelayCommand(Export);
        public ICommand SettingsCommand => new RelayCommand(Settings);

        #endregion

        #region Private Methods

        private void LoadSettingsFromJson()
        {
            try
            {
                settingsManager.LoadDefaultSettings();
                var uiSettings = settingsManager.Settings.UserInterface;
                Logger.LogInfo($"Font settings loaded from default.json: {uiSettings.FontFamily}, size {uiSettings.FontSize}");
                ApplyLoadedSettings();
                Logger.LogInfo("Settings loaded from default.json successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading settings from JSON", ex);
                StatusText = "Ошибка загрузки настроек. Используются значения из default.json.";
                settingsManager.SetDefaultSettings();
                ApplyLoadedSettings();
            }
        }

        private void ApplyLoadedSettings()
        {
            var settings = settingsManager.Settings;

            FolderFilters = new List<string>(settings.FilterSettings.FolderFilters);
            FileFilters = new List<string>(settings.FilterSettings.FileFilters);
            SelectedPath = settings.ApplicationSettings.LastSelectedPath;

            Logger.LogInfo($"Applied settings - Folder filters: {FolderFilters.Count}, File filters: {FileFilters.Count}");
        }

        private void InitializeViewModel()
        {
            if (!string.IsNullOrEmpty(SelectedPath) && Directory.Exists(SelectedPath))
            {
                AnalyzeButtonEnabled = true;
                StatusText = "Последняя папка загружена из настроек.\nНажмите 'Анализировать'.";
            }
            else
            {
                SelectedPath = "";
                AnalyzeButtonEnabled = false;
                StatusText = "Выберите папку для анализа.";
            }
            ExportStatusVisibility = Visibility.Collapsed;
        }

        private void SelectFolder()
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SelectedPath = dialog.SelectedPath;
                AnalyzeButtonEnabled = true;
                StatusText = "Папка выбрана. Нажмите 'Анализировать'.";
                Logger.LogInfo($"Selected folder: {dialog.SelectedPath}");
                settingsManager.UpdateLastSelectedPath(SelectedPath);
            }
        }

        private async Task AnalyzeAsync()
        {
            if (string.IsNullOrEmpty(SelectedPath))
            {
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

                var settings = settingsManager.Settings;
                directoryAnalyzer.FolderFilters = settings.FilterSettings.EnableFolderFilters ? FolderFilters : new List<string>();
                directoryAnalyzer.FileFilters = settings.FilterSettings.EnableFileFilters ? FileFilters : new List<string>();

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
                if (settingsWindow.ShowDialog() == true)
                {
                    ReloadSettings();
                }
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

        #endregion

        #region Public Methods

        public void UpdateFilters(List<string> newFolderFilters, List<string> newFileFilters)
        {
            FolderFilters = newFolderFilters ?? new List<string>();
            FileFilters = newFileFilters ?? new List<string>();
            settingsManager.UpdateFilterSettings(FolderFilters, FileFilters);
            Logger.LogInfo("Filters updated and saved to JSON settings");
        }

        public void UpdateInterfaceSettings(string fontFamily, double fontSize)
        {
            settingsManager.UpdateInterfaceSettings(fontFamily, fontSize);
            Logger.LogInfo($"Interface settings updated: Font={fontFamily}, Size={fontSize}");
        }

        public void ReloadSettings()
        {
            try
            {
                settingsManager.LoadSettings();
                ApplyLoadedSettings();
                StatusText = "Настройки перезагружены из JSON файла.";
                Logger.LogInfo("Settings reloaded from JSON");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error reloading settings", ex);
                StatusText = "Ошибка при перезагрузке настроек.";
            }
        }

        public void SaveCurrentSettings()
        {
            try
            {
                settingsManager.SaveUserSettings();
                StatusText = "Настройки сохранены.";
                Logger.LogInfo("Current settings saved to user file");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving current settings", ex);
                StatusText = "Ошибка при сохранении настроек.";
            }
        }

        public async Task ReanalyzeProjectAsync()
        {
            if (string.IsNullOrEmpty(SelectedPath))
            {
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
                StatusText = "Перезапуск анализа проекта с новыми настройками...";
                Logger.LogInfo($"Reanalyzing project at: {SelectedPath} due to settings change");
                await AnalyzeAsync();
                Logger.LogInfo("Project reanalysis completed successfully");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при перезапуске анализа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText = "Ошибка при перезапуске анализа.";
                Logger.LogError("Error reanalyzing project", ex);
            }
        }

        #endregion
    }
}