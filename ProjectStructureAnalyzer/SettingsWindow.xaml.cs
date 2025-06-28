using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectStructureAnalyzer
{
    public partial class SettingsWindow : Window
    {
        private readonly MainWindow mainWindow;
        private bool _isLoading = false;

        // Словари для связи категорий с чекбоксами
        private Dictionary<CheckBox, List<CheckBox>> _categoryMappings;

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
                [ExcludeSystemFilesCheckBox] = new List<CheckBox>
                {
                    FolderVsCheckBox, FolderGitCheckBox, FolderVscodeCheckBox, FolderIdeaCheckBox,
                    FileGitignoreCheckBox, FileGitattributesCheckBox, FileUserCheckBox, FileSuoCheckBox
                },
                [ExcludeBuildFilesCheckBox] = new List<CheckBox>
                {
                    FolderBinCheckBox, FolderObjCheckBox, FolderDebugCheckBox, FolderReleaseCheckBox,
                    FileSlnCheckBox, FileCsprojCheckBox, FileVbprojCheckBox, FileFsprojCheckBox
                },
                [ExcludeGeneratedFilesCheckBox] = new List<CheckBox>
                {
                    FileDesignerCheckBox, FileGCsCheckBox, FileGICsCheckBox, FileAssemblyInfoCheckBox
                },
                [ExcludeConfigFilesCheckBox] = new List<CheckBox>
                {
                    FileConfigCheckBox, FileJsonCheckBox, FileXmlCheckBox, FileYamlCheckBox
                },
                [ExcludeResourceFilesCheckBox] = new List<CheckBox>
                {
                    FolderNodeModulesCheckBox, FolderPackagesCheckBox, FolderNugetCheckBox
                },
                [ExcludeDocumentationCheckBox] = new List<CheckBox>
                {
                    FolderDocsCheckBox, FolderDocumentationCheckBox,
                    FileMdCheckBox, FileTxtCheckBox, FileReadmeCheckBox
                }
            };
        }

        private void LoadSettings()
        {
            _isLoading = true;
            try
            {
                // Загрузка шрифта
                var fontFamily = new FontFamily(Properties.Settings.Default.ApplicationFontFamily ?? "Segoe UI");
                FontComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
                FontComboBox.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == fontFamily.Source) ??
                                          Fonts.SystemFontFamilies.First(f => f.Source == "Segoe UI");

                // Загрузка размера шрифта
                FontSizeSlider.Value = Properties.Settings.Default.ApplicationFontSize > 0 ?
                                      Properties.Settings.Default.ApplicationFontSize : 13;

                // Загрузка фильтров папок
                var folderFilters = Properties.Settings.Default.FolderFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                LoadFolderFilters(folderFilters);

                // Загрузка фильтров файлов
                var fileFilters = Properties.Settings.Default.FileFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                LoadFileFilters(fileFilters);

                // Обновление состояния категорий
                UpdateCategoryCheckBoxes();

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
            // Системные папки
            FolderVsCheckBox.IsChecked = folderFilters.Contains(".vs", StringComparer.OrdinalIgnoreCase);
            FolderGitCheckBox.IsChecked = folderFilters.Contains(".git", StringComparer.OrdinalIgnoreCase);
            FolderVscodeCheckBox.IsChecked = folderFilters.Contains(".vscode", StringComparer.OrdinalIgnoreCase);
            FolderIdeaCheckBox.IsChecked = folderFilters.Contains(".idea", StringComparer.OrdinalIgnoreCase);

            // Папки сборки
            FolderBinCheckBox.IsChecked = folderFilters.Contains("bin", StringComparer.OrdinalIgnoreCase);
            FolderObjCheckBox.IsChecked = folderFilters.Contains("obj", StringComparer.OrdinalIgnoreCase);
            FolderDebugCheckBox.IsChecked = folderFilters.Contains("Debug", StringComparer.OrdinalIgnoreCase);
            FolderReleaseCheckBox.IsChecked = folderFilters.Contains("Release", StringComparer.OrdinalIgnoreCase);

            // Структурные папки
            FolderSrcCheckBox.IsChecked = folderFilters.Contains("src", StringComparer.OrdinalIgnoreCase);
            FolderPropertiesCheckBox.IsChecked = folderFilters.Contains("Properties", StringComparer.OrdinalIgnoreCase);

            // Папки зависимостей
            FolderNodeModulesCheckBox.IsChecked = folderFilters.Contains("node_modules", StringComparer.OrdinalIgnoreCase);
            FolderPackagesCheckBox.IsChecked = folderFilters.Contains("packages", StringComparer.OrdinalIgnoreCase);
            FolderNugetCheckBox.IsChecked = folderFilters.Contains(".nuget", StringComparer.OrdinalIgnoreCase);

            // Папки документации
            FolderDocsCheckBox.IsChecked = folderFilters.Contains("docs", StringComparer.OrdinalIgnoreCase);
            FolderDocumentationCheckBox.IsChecked = folderFilters.Contains("Documentation", StringComparer.OrdinalIgnoreCase);

            // Временные папки
            FolderTempCheckBox.IsChecked = folderFilters.Contains("temp", StringComparer.OrdinalIgnoreCase);
            FolderTmpCheckBox.IsChecked = folderFilters.Contains("tmp", StringComparer.OrdinalIgnoreCase);
            FolderCacheCheckBox.IsChecked = folderFilters.Contains("cache", StringComparer.OrdinalIgnoreCase);
        }

        private void LoadFileFilters(string[] fileFilters)
        {
            // Исходный код
            FileCsCheckBox.IsChecked = fileFilters.Contains(".cs", StringComparer.OrdinalIgnoreCase);
            FileXamlCheckBox.IsChecked = fileFilters.Contains(".xaml", StringComparer.OrdinalIgnoreCase);
            FileVbCheckBox.IsChecked = fileFilters.Contains(".vb", StringComparer.OrdinalIgnoreCase);

            // Проектные файлы
            FileSlnCheckBox.IsChecked = fileFilters.Contains(".sln", StringComparer.OrdinalIgnoreCase);
            FileCsprojCheckBox.IsChecked = fileFilters.Contains(".csproj", StringComparer.OrdinalIgnoreCase);
            FileVbprojCheckBox.IsChecked = fileFilters.Contains(".vbproj", StringComparer.OrdinalIgnoreCase);
            FileFsprojCheckBox.IsChecked = fileFilters.Contains(".fsproj", StringComparer.OrdinalIgnoreCase);

            // Конфигурационные
            FileConfigCheckBox.IsChecked = fileFilters.Contains(".config", StringComparer.OrdinalIgnoreCase);
            FileJsonCheckBox.IsChecked = fileFilters.Contains(".json", StringComparer.OrdinalIgnoreCase);
            FileXmlCheckBox.IsChecked = fileFilters.Contains(".xml", StringComparer.OrdinalIgnoreCase);
            FileYamlCheckBox.IsChecked = fileFilters.Contains(".yaml", StringComparer.OrdinalIgnoreCase) ||
                                        fileFilters.Contains(".yml", StringComparer.OrdinalIgnoreCase);

            // Автогенерируемые
            FileDesignerCheckBox.IsChecked = fileFilters.Contains(".Designer.cs", StringComparer.OrdinalIgnoreCase);
            FileGCsCheckBox.IsChecked = fileFilters.Contains(".g.cs", StringComparer.OrdinalIgnoreCase);
            FileGICsCheckBox.IsChecked = fileFilters.Contains(".g.i.cs", StringComparer.OrdinalIgnoreCase);
            FileAssemblyInfoCheckBox.IsChecked = fileFilters.Contains("AssemblyInfo.cs", StringComparer.OrdinalIgnoreCase);

            // Системные
            FileGitignoreCheckBox.IsChecked = fileFilters.Contains(".gitignore", StringComparer.OrdinalIgnoreCase);
            FileGitattributesCheckBox.IsChecked = fileFilters.Contains(".gitattributes", StringComparer.OrdinalIgnoreCase);
            FileUserCheckBox.IsChecked = fileFilters.Contains(".user", StringComparer.OrdinalIgnoreCase);
            FileSuoCheckBox.IsChecked = fileFilters.Contains(".suo", StringComparer.OrdinalIgnoreCase);

            // Документация
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

        private void UpdateCategoryCheckBoxes()
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
                    categoryCheckBox.IsChecked = null; // Частичное выделение
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сохранение шрифта
                if (FontComboBox.SelectedItem is FontFamily fontFamily)
                {
                    Properties.Settings.Default.ApplicationFontFamily = fontFamily.Source;
                }

                // Сохранение размера шрифта
                Properties.Settings.Default.ApplicationFontSize = (int)FontSizeSlider.Value;

                // Сохранение фильтров папок
                var folderFilters = GetSelectedFolderFilters();
                Properties.Settings.Default.FolderFilters = string.Join(",", folderFilters);

                // Сохранение фильтров файлов
                var fileFilters = GetSelectedFileFilters();
                Properties.Settings.Default.FileFilters = string.Join(",", fileFilters);

                Properties.Settings.Default.Save();
                Logger.LogInfo("Settings saved successfully.");

                // Применение настроек в MainWindow
                mainWindow.ApplySettings();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving settings", ex);
                MessageBox.Show("Ошибка при сохранении настроек.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<string> GetSelectedFolderFilters()
        {
            var filters = new List<string>();

            // Системные папки
            if (FolderVsCheckBox.IsChecked == true) filters.Add(".vs");
            if (FolderGitCheckBox.IsChecked == true) filters.Add(".git");
            if (FolderVscodeCheckBox.IsChecked == true) filters.Add(".vscode");
            if (FolderIdeaCheckBox.IsChecked == true) filters.Add(".idea");

            // Папки сборки
            if (FolderBinCheckBox.IsChecked == true) filters.Add("bin");
            if (FolderObjCheckBox.IsChecked == true) filters.Add("obj");
            if (FolderDebugCheckBox.IsChecked == true) filters.Add("Debug");
            if (FolderReleaseCheckBox.IsChecked == true) filters.Add("Release");

            // Структурные папки
            if (FolderSrcCheckBox.IsChecked == true) filters.Add("src");
            if (FolderPropertiesCheckBox.IsChecked == true) filters.Add("Properties");

            // Папки зависимостей
            if (FolderNodeModulesCheckBox.IsChecked == true) filters.Add("node_modules");
            if (FolderPackagesCheckBox.IsChecked == true) filters.Add("packages");
            if (FolderNugetCheckBox.IsChecked == true) filters.Add(".nuget");

            // Папки документации
            if (FolderDocsCheckBox.IsChecked == true) filters.Add("docs");
            if (FolderDocumentationCheckBox.IsChecked == true) filters.Add("Documentation");

            // Временные папки
            if (FolderTempCheckBox.IsChecked == true) filters.Add("temp");
            if (FolderTmpCheckBox.IsChecked == true) filters.Add("tmp");
            if (FolderCacheCheckBox.IsChecked == true) filters.Add("cache");

            return filters;
        }

        private List<string> GetSelectedFileFilters()
        {
            var filters = new List<string>();

            // Исходный код
            if (FileCsCheckBox.IsChecked == true) filters.Add(".cs");
            if (FileXamlCheckBox.IsChecked == true) filters.Add(".xaml");
            if (FileVbCheckBox.IsChecked == true) filters.Add(".vb");

            // Проектные файлы
            if (FileSlnCheckBox.IsChecked == true) filters.Add(".sln");
            if (FileCsprojCheckBox.IsChecked == true) filters.Add(".csproj");
            if (FileVbprojCheckBox.IsChecked == true) filters.Add(".vbproj");
            if (FileFsprojCheckBox.IsChecked == true) filters.Add(".fsproj");

            // Конфигурационные
            if (FileConfigCheckBox.IsChecked == true) filters.Add(".config");
            if (FileJsonCheckBox.IsChecked == true) filters.Add(".json");
            if (FileXmlCheckBox.IsChecked == true) filters.Add(".xml");
            if (FileYamlCheckBox.IsChecked == true)
            {
                filters.Add(".yaml");
                filters.Add(".yml");
            }

            // Автогенерируемые
            if (FileDesignerCheckBox.IsChecked == true) filters.Add(".Designer.cs");
            if (FileGCsCheckBox.IsChecked == true) filters.Add(".g.cs");
            if (FileGICsCheckBox.IsChecked == true) filters.Add(".g.i.cs");
            if (FileAssemblyInfoCheckBox.IsChecked == true) filters.Add("AssemblyInfo.cs");

            // Системные
            if (FileGitignoreCheckBox.IsChecked == true) filters.Add(".gitignore");
            if (FileGitattributesCheckBox.IsChecked == true) filters.Add(".gitattributes");
            if (FileUserCheckBox.IsChecked == true) filters.Add(".user");
            if (FileSuoCheckBox.IsChecked == true) filters.Add(".suo");

            // Документация
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
                // Сброс всех чекбоксов категорий
                foreach (var categoryCheckBox in _categoryMappings.Keys)
                {
                    categoryCheckBox.IsChecked = false;
                }

                // Сброс всех чекбоксов папок и файлов
                var allCheckBoxes = GetAllFilterCheckBoxes();
                foreach (var checkBox in allCheckBoxes)
                {
                    checkBox.IsChecked = false;
                }

                // Установка стандартных исключений (рекомендуемые)
                FolderVsCheckBox.IsChecked = true;
                FolderGitCheckBox.IsChecked = true;
                FolderBinCheckBox.IsChecked = true;
                FolderObjCheckBox.IsChecked = true;
                FileDesignerCheckBox.IsChecked = true;
                FileGCsCheckBox.IsChecked = true;
                FileGICsCheckBox.IsChecked = true;

                // Сброс шрифта
                FontComboBox.SelectedItem = Fonts.SystemFontFamilies.First(f => f.Source == "Segoe UI");
                FontSizeSlider.Value = 13;

                UpdateCategoryCheckBoxes();
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

            // Добавление всех чекбоксов из категорий
            foreach (var mapping in _categoryMappings)
            {
                checkBoxes.AddRange(mapping.Value);
            }

            // Добавление остальных чекбоксов
            checkBoxes.AddRange(new[]
            {
                FolderSrcCheckBox, FolderPropertiesCheckBox, FolderTempCheckBox, FolderTmpCheckBox, FolderCacheCheckBox,
                FileCsCheckBox, FileXamlCheckBox, FileVbCheckBox
            });

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
                // Игнорируем ошибки предпросмотра
            }
        }

        public bool SaveSettings(string fontFamily, double fontSize, string folderFilters, string fileFilters)
        {
            try
            {
                Properties.Settings.Default.ApplicationFontFamily = fontFamily;
                if (fontSize > 0)
                {
                    Properties.Settings.Default.ApplicationFontSize = (int)fontSize;
                }
                else
                {
                    return false;
                }
                Properties.Settings.Default.FolderFilters = folderFilters;
                Properties.Settings.Default.FileFilters = fileFilters;
                Properties.Settings.Default.Save();
                mainWindow.ApplySettings();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}