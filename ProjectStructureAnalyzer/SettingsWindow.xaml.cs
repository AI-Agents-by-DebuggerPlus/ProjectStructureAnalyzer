using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            // Загрузка фильтров
            var folderExclusions = (Properties.Settings.Default.FolderFilters ?? "").Split(',');
            var fileExclusions = (Properties.Settings.Default.FileFilters ?? "").Split(',');

            FolderSrcCheckBox.IsChecked = folderExclusions.Contains("src");
            FolderBinCheckBox.IsChecked = folderExclusions.Contains("bin");
            FolderObjCheckBox.IsChecked = folderExclusions.Contains("obj");
            FolderPropertiesCheckBox.IsChecked = folderExclusions.Contains("Properties");

            FileCsCheckBox.IsChecked = fileExclusions.Contains(".cs");
            FileSlnCheckBox.IsChecked = fileExclusions.Contains(".sln");
            FileCsprojCheckBox.IsChecked = fileExclusions.Contains(".csproj");
            FileConfigCheckBox.IsChecked = fileExclusions.Contains(".config");

            // Загрузка и настройка шрифта
            FontComboBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            string savedFont = Properties.Settings.Default.ApplicationFontFamily;
            if (!string.IsNullOrEmpty(savedFont))
            {
                FontComboBox.SelectedItem = new FontFamily(savedFont);
            }
            if (FontComboBox.SelectedItem == null)
            {
                FontComboBox.SelectedItem = new FontFamily("Segoe UI");
            }

            // Загрузка и настройка размера шрифта
            int savedSize = Properties.Settings.Default.ApplicationFontSize;
            if (savedSize > 0)
            {
                FontSizeSlider.Value = savedSize;
            }
            else
            {
                FontSizeSlider.Value = 13;
            }

            UpdatePreview();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохранение фильтров
            var folderExclusions = new List<string>();
            if (FolderSrcCheckBox.IsChecked == true) folderExclusions.Add("src");
            if (FolderBinCheckBox.IsChecked == true) folderExclusions.Add("bin");
            if (FolderObjCheckBox.IsChecked == true) folderExclusions.Add("obj");
            if (FolderPropertiesCheckBox.IsChecked == true) folderExclusions.Add("Properties");
            Properties.Settings.Default.FolderFilters = string.Join(",", folderExclusions);

            var fileExclusions = new List<string>();
            if (FileCsCheckBox.IsChecked == true) fileExclusions.Add(".cs");
            if (FileSlnCheckBox.IsChecked == true) fileExclusions.Add(".sln");
            if (FileCsprojCheckBox.IsChecked == true) fileExclusions.Add(".csproj");
            if (FileConfigCheckBox.IsChecked == true) fileExclusions.Add(".config");
            Properties.Settings.Default.FileFilters = string.Join(",", fileExclusions);

            // Сохранение семейства шрифта
            if (FontComboBox.SelectedItem is FontFamily selectedFont)
            {
                Properties.Settings.Default.ApplicationFontFamily = selectedFont.Source;
            }

            // Сохранение размера шрифта
            Properties.Settings.Default.ApplicationFontSize = (int)FontSizeSlider.Value;

            Properties.Settings.Default.Save();
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void PreviewSettingsChanged(object sender, RoutedEventArgs e)
        {
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (FontComboBox == null || FontSizeSlider == null || PreviewTextBlock == null)
            {
                return;
            }

            if (FontComboBox.SelectedItem is FontFamily selectedFont)
            {
                PreviewTextBlock.FontFamily = selectedFont;
                PreviewTextBlock.FontSize = FontSizeSlider.Value;
            }
        }

        #region Custom Title Bar Methods
        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}