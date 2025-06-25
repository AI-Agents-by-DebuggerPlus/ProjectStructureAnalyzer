using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectStructureAnalyzer
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                // Загрузка шрифта
                var fontFamily = new FontFamily(Properties.Settings.Default.ApplicationFontFamily ?? "Segoe UI");
                FontComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
                FontComboBox.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == fontFamily.Source) ?? Fonts.SystemFontFamilies.First(f => f.Source == "Segoe UI");

                // Загрузка размера шрифта
                FontSizeSlider.Value = Properties.Settings.Default.ApplicationFontSize > 0 ? Properties.Settings.Default.ApplicationFontSize : 13;

                // Загрузка фильтров
                var folderFilters = Properties.Settings.Default.FolderFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                FolderSrcCheckBox.IsChecked = folderFilters.Contains("src", StringComparer.OrdinalIgnoreCase);
                FolderBinCheckBox.IsChecked = folderFilters.Contains("bin", StringComparer.OrdinalIgnoreCase);
                FolderObjCheckBox.IsChecked = folderFilters.Contains("obj", StringComparer.OrdinalIgnoreCase);
                FolderPropertiesCheckBox.IsChecked = folderFilters.Contains("Properties", StringComparer.OrdinalIgnoreCase);
                FolderVsCheckBox.IsChecked = folderFilters.Contains(".vs", StringComparer.OrdinalIgnoreCase);
                FolderGitCheckBox.IsChecked = folderFilters.Contains(".git", StringComparer.OrdinalIgnoreCase);

                var fileFilters = Properties.Settings.Default.FileFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                FileCsCheckBox.IsChecked = fileFilters.Contains(".cs", StringComparer.OrdinalIgnoreCase);
                FileSlnCheckBox.IsChecked = fileFilters.Contains(".sln", StringComparer.OrdinalIgnoreCase);
                FileCsprojCheckBox.IsChecked = fileFilters.Contains(".csproj", StringComparer.OrdinalIgnoreCase);
                FileConfigCheckBox.IsChecked = fileFilters.Contains(".config", StringComparer.OrdinalIgnoreCase);

                PreviewSettingsChanged(null, null); // Обновление предпросмотра
            }
            catch (Exception ex)
            {
                Logger.LogError("Error loading settings", ex);
                System.Windows.MessageBox.Show("Ошибка при загрузке настроек.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var folderFilters = new[] { FolderSrcCheckBox, FolderBinCheckBox, FolderObjCheckBox, FolderPropertiesCheckBox, FolderVsCheckBox, FolderGitCheckBox }
                    .Where(cb => cb.IsChecked == true)
                    .Select(cb => cb.Content.ToString())
                    .ToArray();
                Properties.Settings.Default.FolderFilters = string.Join(",", folderFilters);

                // Сохранение фильтров файлов
                var fileFilters = new[] { FileCsCheckBox, FileSlnCheckBox, FileCsprojCheckBox, FileConfigCheckBox }
                    .Where(cb => cb.IsChecked == true)
                    .Select(cb => cb.Content.ToString())
                    .ToArray();
                Properties.Settings.Default.FileFilters = string.Join(",", fileFilters);

                Properties.Settings.Default.Save();
                Logger.LogInfo("Settings saved successfully.");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving settings", ex);
                System.Windows.MessageBox.Show("Ошибка при сохранении настроек.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}