using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace ProjectStructureAnalyzer
{
    public partial class SettingsWindow : Window
    {
        private readonly MainWindow mainWindow;
        private bool _isLoading = false;
        private Dictionary<CheckBox, List<CheckBox>> _categoryMappings;

        public class SettingsProfile
        {
            public string Name { get; set; } = "";
            public UserInterfaceSettings UserInterface { get; set; } = new UserInterfaceSettings();
            public FilterSettings FilterSettings { get; set; } = new FilterSettings();
            public ApplicationSettings ApplicationSettings { get; set; } = new ApplicationSettings();
            public DateTime CreatedDate { get; set; } = DateTime.Now;
        }

        public class UserInterfaceSettings
        {
            public string FontFamily { get; set; } = "Segoe UI";
            public int FontSize { get; set; } = 13;
        }

        public class FilterSettings
        {
            public List<string> FolderFilters { get; set; } = new List<string>();
            public List<string> FileFilters { get; set; } = new List<string>();
        }

        public class ApplicationSettings
        {
            public string LastSelectedPath { get; set; } = "";
        }

        public SettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            InitializeCategoryMappings();
            LoadSettings();
        }

        private void InitializeCategoryMappings()
        {
            _categoryMappings = new Dictionary<CheckBox, List<CheckBox>>
            {
                [ExcludeSystemFilesCheckBox] = new List<CheckBox> { FolderVsCheckBox, FolderGitCheckBox, FolderVscodeCheckBox, FolderIdeaCheckBox, FileGitignoreCheckBox, FileGitattributesCheckBox, FileUserCheckBox, FileSuoCheckBox },
                [ExcludeBuildFilesCheckBox] = new List<CheckBox> { FolderBinCheckBox, FolderObjCheckBox, FolderDebugCheckBox, FolderReleaseCheckBox, FileSlnCheckBox, FileCsprojCheckBox, FileVbprojCheckBox, FileFsprojCheckBox },
                [ExcludeGeneratedFilesCheckBox] = new List<CheckBox> { FileDesignerCheckBox, FileGCsCheckBox, FileGICsCheckBox, FileAssemblyInfoCheckBox },
                [ExcludeConfigFilesCheckBox] = new List<CheckBox> { FileConfigCheckBox, FileJsonCheckBox, FileXmlCheckBox, FileYamlCheckBox },
                [ExcludeResourceFilesCheckBox] = new List<CheckBox> { FolderNodeModulesCheckBox, FolderPackagesCheckBox, FolderNugetCheckBox },
                [ExcludeDocumentationCheckBox] = new List<CheckBox> { FolderDocsCheckBox, FolderDocumentationCheckBox, FileMdCheckBox, FileTxtCheckBox, FileReadmeCheckBox }
            };
        }

        private void LoadSettings()
        {
            _isLoading = true;
            try
            {
                var settings = mainWindow.ViewModel.AppSettings;
                var uiSettings = settings.UserInterface;

                FontComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
                FontComboBox.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == uiSettings.FontFamily) ??
                                          Fonts.SystemFontFamilies.First(f => f.Source == "Segoe UI");

                FontSizeSlider.Value = uiSettings.FontSize > 0 ? uiSettings.FontSize : 13;

                LoadFolderFilters(settings.FilterSettings.FolderFilters.ToArray());
                LoadFileFilters(settings.FilterSettings.FileFilters.ToArray());

                UpdateCategoryCheckBoxes(null, null);
                PreviewSettingsChanged(null, null);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading settings", ex);
                MessageBox.Show("Ошибка при загрузке настроек.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void LoadFolderFilters(string[] folderFilters)
        {
            FolderVsCheckBox.IsChecked = folderFilters.Contains(".vs", StringComparer.OrdinalIgnoreCase);
            FolderGitCheckBox.IsChecked = folderFilters.Contains(".git", StringComparer.OrdinalIgnoreCase);
            FolderVscodeCheckBox.IsChecked = folderFilters.Contains(".vscode", StringComparer.OrdinalIgnoreCase);
            FolderIdeaCheckBox.IsChecked = folderFilters.Contains(".idea", StringComparer.OrdinalIgnoreCase);
            FolderBinCheckBox.IsChecked = folderFilters.Contains("bin", StringComparer.OrdinalIgnoreCase);
            FolderObjCheckBox.IsChecked = folderFilters.Contains("obj", StringComparer.OrdinalIgnoreCase);
            FolderDebugCheckBox.IsChecked = folderFilters.Contains("Debug", StringComparer.OrdinalIgnoreCase);
            FolderReleaseCheckBox.IsChecked = folderFilters.Contains("Release", StringComparer.OrdinalIgnoreCase);
            FolderSrcCheckBox.IsChecked = folderFilters.Contains("src", StringComparer.OrdinalIgnoreCase);
            FolderPropertiesCheckBox.IsChecked = folderFilters.Contains("Properties", StringComparer.OrdinalIgnoreCase);
            FolderNodeModulesCheckBox.IsChecked = folderFilters.Contains("node_modules", StringComparer.OrdinalIgnoreCase);
            FolderPackagesCheckBox.IsChecked = folderFilters.Contains("packages", StringComparer.OrdinalIgnoreCase);
            FolderNugetCheckBox.IsChecked = folderFilters.Contains(".nuget", StringComparer.OrdinalIgnoreCase);
            FolderDocsCheckBox.IsChecked = folderFilters.Contains("docs", StringComparer.OrdinalIgnoreCase);
            FolderDocumentationCheckBox.IsChecked = folderFilters.Contains("Documentation", StringComparer.OrdinalIgnoreCase);
            FolderTempCheckBox.IsChecked = folderFilters.Contains("temp", StringComparer.OrdinalIgnoreCase);
            FolderTmpCheckBox.IsChecked = folderFilters.Contains("tmp", StringComparer.OrdinalIgnoreCase);
            FolderCacheCheckBox.IsChecked = folderFilters.Contains("cache", StringComparer.OrdinalIgnoreCase);
        }

        private void LoadFileFilters(string[] fileFilters)
        {
            FileCsCheckBox.IsChecked = fileFilters.Contains(".cs", StringComparer.OrdinalIgnoreCase);
            FileXamlCheckBox.IsChecked = fileFilters.Contains(".xaml", StringComparer.OrdinalIgnoreCase);
            FileVbCheckBox.IsChecked = fileFilters.Contains(".vb", StringComparer.OrdinalIgnoreCase);
            FileSlnCheckBox.IsChecked = fileFilters.Contains(".sln", StringComparer.OrdinalIgnoreCase);
            FileCsprojCheckBox.IsChecked = fileFilters.Contains(".csproj", StringComparer.OrdinalIgnoreCase);
            FileVbprojCheckBox.IsChecked = fileFilters.Contains(".vbproj", StringComparer.OrdinalIgnoreCase);
            FileFsprojCheckBox.IsChecked = fileFilters.Contains(".fsproj", StringComparer.OrdinalIgnoreCase);
            FileConfigCheckBox.IsChecked = fileFilters.Contains(".config", StringComparer.OrdinalIgnoreCase);
            FileJsonCheckBox.IsChecked = fileFilters.Contains(".json", StringComparer.OrdinalIgnoreCase);
            FileXmlCheckBox.IsChecked = fileFilters.Contains(".xml", StringComparer.OrdinalIgnoreCase);
            FileYamlCheckBox.IsChecked = (fileFilters.Contains(".yaml", StringComparer.OrdinalIgnoreCase) || fileFilters.Contains(".yml", StringComparer.OrdinalIgnoreCase));
            FileDesignerCheckBox.IsChecked = fileFilters.Contains(".Designer.cs", StringComparer.OrdinalIgnoreCase);
            FileGCsCheckBox.IsChecked = fileFilters.Contains(".g.cs", StringComparer.OrdinalIgnoreCase);
            FileGICsCheckBox.IsChecked = fileFilters.Contains(".g.i.cs", StringComparer.OrdinalIgnoreCase);
            FileAssemblyInfoCheckBox.IsChecked = fileFilters.Contains("AssemblyInfo.cs", StringComparer.OrdinalIgnoreCase);
            FileGitignoreCheckBox.IsChecked = fileFilters.Contains(".gitignore", StringComparer.OrdinalIgnoreCase);
            FileGitattributesCheckBox.IsChecked = fileFilters.Contains(".gitattributes", StringComparer.OrdinalIgnoreCase);
            FileUserCheckBox.IsChecked = fileFilters.Contains(".user", StringComparer.OrdinalIgnoreCase);
            FileSuoCheckBox.IsChecked = fileFilters.Contains(".suo", StringComparer.OrdinalIgnoreCase);
            FileMdCheckBox.IsChecked = fileFilters.Contains(".md", StringComparer.OrdinalIgnoreCase);
            FileTxtCheckBox.IsChecked = fileFilters.Contains(".txt", StringComparer.OrdinalIgnoreCase);
            FileReadmeCheckBox.IsChecked = fileFilters.Contains("README.*", StringComparer.OrdinalIgnoreCase);
        }

        private void CategoryCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (_isLoading) return;

            var categoryCheckBox = sender as CheckBox;
            if (categoryCheckBox != null && _categoryMappings.ContainsKey(categoryCheckBox))
            {
                var isChecked = categoryCheckBox.IsChecked == true;
                foreach (var childCheckBox in _categoryMappings[categoryCheckBox])
                {
                    childCheckBox.IsChecked = isChecked;
                }
            }
        }

        private void UpdateCategoryCheckBoxes(object sender, RoutedEventArgs e)
        {
            foreach (var mapping in _categoryMappings)
            {
                var categoryCheckBox = mapping.Key;
                var childCheckBoxes = mapping.Value;

                var checkedCount = childCheckBoxes.Count(cb => cb.IsChecked == true);

                if (checkedCount == 0)
                {
                    categoryCheckBox.IsChecked = false;
                }
                else if (checkedCount == childCheckBoxes.Count)
                {
                    categoryCheckBox.IsChecked = true;
                }
                else
                {
                    categoryCheckBox.IsChecked = null;
                }
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveCurrentSettings();
                await mainWindow.ViewModel.ReanalyzeProjectAsync();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving settings", ex);
                MessageBox.Show("Ошибка при сохранении настроек.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Title = "Сохранить профиль настроек как...",
                    Filter = "Файлы профилей настроек (*.json)|*.json|Все файлы (*.*)|*.*",
                    DefaultExt = "json",
                    AddExtension = true,
                    InitialDirectory = GetProfilesDirectory()
                };

                if (dialog.ShowDialog() == true)
                {
                    var profile = CreateCurrentProfile();
                    profile.Name = Path.GetFileNameWithoutExtension(dialog.FileName);

                    var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(dialog.FileName, json);

                    Logger.LogInfo($"Settings profile saved to: {dialog.FileName}");
                    MessageBox.Show($"Профиль настроек сохранен как '{profile.Name}'", "Сохранение",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving settings profile", ex);
                MessageBox.Show("Ошибка при сохранении профиля настроек.", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveAsDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Сохранить текущие настройки как настройки по умолчанию?\n\n" +
                    "Эти настройки будут автоматически применяться при запуске приложения.",
                    "Сохранить как настройки по умолчанию",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var profile = CreateCurrentProfile();
                    profile.Name = "Default Settings";

                    var defaultProfilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "default.json");
                    var json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(defaultProfilePath, json);

                    SaveCurrentSettings();
                    await mainWindow.ViewModel.ReanalyzeProjectAsync();

                    Logger.LogInfo("Default settings profile saved");
                    MessageBox.Show("Настройки сохранены как настройки по умолчанию", "Сохранение",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving default settings", ex);
                MessageBox.Show("Ошибка при сохранении настроек по умолчанию.", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadProfileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Загрузить профиль настроек",
                    Filter = "Файлы профилей настроек (*.json)|*.json|Все файлы (*.*)|*.*",
                    InitialDirectory = GetProfilesDirectory()
                };

                if (dialog.ShowDialog() == true)
                {
                    var json = File.ReadAllText(dialog.FileName);
                    var profile = JsonSerializer.Deserialize<SettingsProfile>(json);

                    if (profile != null)
                    {
                        ApplyProfile(profile);
                        SaveCurrentSettings();
                        await mainWindow.ViewModel.ReanalyzeProjectAsync();
                        Logger.LogInfo($"Settings profile loaded from: {dialog.FileName}");
                        MessageBox.Show($"Профиль настроек '{profile.Name}' загружен", "Загрузка",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading settings profile", ex);
                MessageBox.Show("Ошибка при загрузке профиля настроек.", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private SettingsProfile CreateCurrentProfile()
        {
            return new SettingsProfile
            {
                UserInterface = new UserInterfaceSettings
                {
                    FontFamily = (FontComboBox.SelectedItem as FontFamily)?.Source ?? "Segoe UI",
                    FontSize = (int)FontSizeSlider.Value
                },
                FilterSettings = new FilterSettings
                {
                    FolderFilters = GetSelectedFolderFilters(),
                    FileFilters = GetSelectedFileFilters()
                },
                ApplicationSettings = new ApplicationSettings
                {
                    LastSelectedPath = mainWindow.ViewModel.SelectedPath ?? ""
                },
                CreatedDate = DateTime.Now
            };
        }

        private void ApplyProfile(SettingsProfile profile)
        {
            _isLoading = true;
            try
            {
                var fontFamily = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == profile.UserInterface.FontFamily) ??
                               Fonts.SystemFontFamilies.First(f => f.Source == "Segoe UI");
                FontComboBox.SelectedItem = fontFamily;
                FontSizeSlider.Value = Math.Max(10, Math.Min(24, profile.UserInterface.FontSize));

                LoadFolderFilters(profile.FilterSettings.FolderFilters.ToArray());
                LoadFileFilters(profile.FilterSettings.FileFilters.ToArray());

                UpdateCategoryCheckBoxes(null, null);
                PreviewSettingsChanged(null, null);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private string GetProfilesDirectory()
        {
            var exePath = AppDomain.CurrentDomain.BaseDirectory;
            var profilesDir = Path.Combine(exePath, "Settings");
            Directory.CreateDirectory(profilesDir);
            return profilesDir;
        }

        private void SaveCurrentSettings()
        {
            var fontFamily = (FontComboBox.SelectedItem as FontFamily)?.Source ?? "Segoe UI";
            var fontSize = FontSizeSlider.Value;

            var folderFilters = GetSelectedFolderFilters();
            var fileFilters = GetSelectedFileFilters();

            mainWindow.ViewModel.UpdateInterfaceSettings(fontFamily, fontSize);
            mainWindow.ViewModel.UpdateFilters(folderFilters, fileFilters);
            mainWindow.ViewModel.SaveCurrentSettings();

            mainWindow.ApplySettingsFromJson();
        }

        private List<string> GetSelectedFolderFilters()
        {
            var filters = new List<string>();
            if (FolderVsCheckBox.IsChecked == true) filters.Add(".vs");
            if (FolderGitCheckBox.IsChecked == true) filters.Add(".git");
            if (FolderVscodeCheckBox.IsChecked == true) filters.Add(".vscode");
            if (FolderIdeaCheckBox.IsChecked == true) filters.Add(".idea");
            if (FolderBinCheckBox.IsChecked == true) filters.Add("bin");
            if (FolderObjCheckBox.IsChecked == true) filters.Add("obj");
            if (FolderDebugCheckBox.IsChecked == true) filters.Add("Debug");
            if (FolderReleaseCheckBox.IsChecked == true) filters.Add("Release");
            if (FolderSrcCheckBox.IsChecked == true) filters.Add("src");
            if (FolderPropertiesCheckBox.IsChecked == true) filters.Add("Properties");
            if (FolderNodeModulesCheckBox.IsChecked == true) filters.Add("node_modules");
            if (FolderPackagesCheckBox.IsChecked == true) filters.Add("packages");
            if (FolderNugetCheckBox.IsChecked == true) filters.Add(".nuget");
            if (FolderDocsCheckBox.IsChecked == true) filters.Add("docs");
            if (FolderDocumentationCheckBox.IsChecked == true) filters.Add("Documentation");
            if (FolderTempCheckBox.IsChecked == true) filters.Add("temp");
            if (FolderTmpCheckBox.IsChecked == true) filters.Add("tmp");
            if (FolderCacheCheckBox.IsChecked == true) filters.Add("cache");
            return filters;
        }

        private List<string> GetSelectedFileFilters()
        {
            var filters = new List<string>();
            if (FileCsCheckBox.IsChecked == true) filters.Add(".cs");
            if (FileXamlCheckBox.IsChecked == true) filters.Add(".xaml");
            if (FileVbCheckBox.IsChecked == true) filters.Add(".vb");
            if (FileSlnCheckBox.IsChecked == true) filters.Add(".sln");
            if (FileCsprojCheckBox.IsChecked == true) filters.Add(".csproj");
            if (FileVbprojCheckBox.IsChecked == true) filters.Add(".vbproj");
            if (FileFsprojCheckBox.IsChecked == true) filters.Add(".fsproj");
            if (FileConfigCheckBox.IsChecked == true) filters.Add(".config");
            if (FileJsonCheckBox.IsChecked == true) filters.Add(".json");
            if (FileXmlCheckBox.IsChecked == true) filters.Add(".xml");
            if (FileYamlCheckBox.IsChecked == true) { filters.Add(".yaml"); filters.Add(".yml"); }
            if (FileDesignerCheckBox.IsChecked == true) filters.Add(".Designer.cs");
            if (FileGCsCheckBox.IsChecked == true) filters.Add(".g.cs");
            if (FileGICsCheckBox.IsChecked == true) filters.Add(".g.i.cs");
            if (FileAssemblyInfoCheckBox.IsChecked == true) filters.Add("AssemblyInfo.cs");
            if (FileGitignoreCheckBox.IsChecked == true) filters.Add(".gitignore");
            if (FileGitattributesCheckBox.IsChecked == true) filters.Add(".gitattributes");
            if (FileUserCheckBox.IsChecked == true) filters.Add(".user");
            if (FileSuoCheckBox.IsChecked == true) filters.Add(".suo");
            if (FileMdCheckBox.IsChecked == true) filters.Add(".md");
            if (FileTxtCheckBox.IsChecked == true) filters.Add(".txt");
            if (FileReadmeCheckBox.IsChecked == true) filters.Add("README.*");
            return filters;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Сбросить все настройки к значениям по умолчанию?",
                                       "Подтверждение",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ResetToDefaults();
            }
        }

        private void ResetToDefaults()
        {
            _isLoading = true;
            try
            {
                foreach (var categoryCheckBox in _categoryMappings.Keys)
                {
                    categoryCheckBox.IsChecked = false;
                }

                var allCheckBoxes = GetAllFilterCheckBoxes();
                foreach (var checkBox in allCheckBoxes)
                {
                    checkBox.IsChecked = false;
                }

                FolderVsCheckBox.IsChecked = true;
                FolderGitCheckBox.IsChecked = true;
                FolderBinCheckBox.IsChecked = true;
                FolderObjCheckBox.IsChecked = true;
                FileDesignerCheckBox.IsChecked = true;
                FileGCsCheckBox.IsChecked = true;
                FileGICsCheckBox.IsChecked = true;

                FontComboBox.SelectedItem = Fonts.SystemFontFamilies.First(f => f.Source == "Segoe UI");
                FontSizeSlider.Value = 13;

                UpdateCategoryCheckBoxes(null, null);
                PreviewSettingsChanged(null, null);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private List<CheckBox> GetAllFilterCheckBoxes()
        {
            var checkBoxes = new List<CheckBox>();
            foreach (var mapping in _categoryMappings)
            {
                checkBoxes.AddRange(mapping.Value);
            }
            checkBoxes.AddRange(new[] { FolderSrcCheckBox, FolderPropertiesCheckBox, FolderTempCheckBox, FolderTmpCheckBox, FolderCacheCheckBox, FileCsCheckBox, FileXamlCheckBox, FileVbCheckBox });
            return checkBoxes.Distinct().ToList();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PreviewSettingsChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FontComboBox.SelectedItem is FontFamily fontFamily)
                {
                    PreviewTextBlock.FontFamily = fontFamily;
                }
                PreviewTextBlock.FontSize = FontSizeSlider.Value;
            }
            catch
            {
            }
        }
    }
}