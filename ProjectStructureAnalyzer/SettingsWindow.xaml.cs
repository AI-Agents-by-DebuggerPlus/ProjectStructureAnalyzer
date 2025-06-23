using System.Windows;
using System.Linq; // Уже подключено для LINQ

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
            // Загрузка сохраненных фильтров и установка чекбоксов
            string[] folderFilters = (Properties.Settings.Default.FolderFilters ?? "").Split(',').Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f)).ToArray();
            string[] fileFilters = (Properties.Settings.Default.FileFilters ?? "").Split(',').Select(f => f.Trim().ToLower()).Where(f => !string.IsNullOrEmpty(f)).ToArray();

            FolderSrcCheckBox.IsChecked = folderFilters.Contains("src");
            FolderBinCheckBox.IsChecked = folderFilters.Contains("bin");
            FolderObjCheckBox.IsChecked = folderFilters.Contains("obj");
            FolderPropertiesCheckBox.IsChecked = folderFilters.Contains("Properties");

            FileCsCheckBox.IsChecked = fileFilters.Contains(".cs");
            FileSlnCheckBox.IsChecked = fileFilters.Contains(".sln");
            FileCsprojCheckBox.IsChecked = fileFilters.Contains(".csproj");
            FileConfigCheckBox.IsChecked = fileFilters.Contains(".config");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Сохранение выбранных фильтров
            var folderFilters = new[]
            {
                FolderSrcCheckBox.IsChecked == true ? "src" : "",
                FolderBinCheckBox.IsChecked == true ? "bin" : "",
                FolderObjCheckBox.IsChecked == true ? "obj" : "",
                FolderPropertiesCheckBox.IsChecked == true ? "Properties" : ""
            }.Where(f => !string.IsNullOrEmpty(f)).ToArray();
            Properties.Settings.Default.FolderFilters = string.Join(",", folderFilters);

            var fileFilters = new[]
            {
                FileCsCheckBox.IsChecked == true ? ".cs" : "",
                FileSlnCheckBox.IsChecked == true ? ".sln" : "",
                FileCsprojCheckBox.IsChecked == true ? ".csproj" : "",
                FileConfigCheckBox.IsChecked == true ? ".config" : ""
            }.Where(f => !string.IsNullOrEmpty(f)).ToArray();
            Properties.Settings.Default.FileFilters = string.Join(",", fileFilters);

            Properties.Settings.Default.Save();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}