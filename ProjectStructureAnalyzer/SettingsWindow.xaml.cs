using System;
using System.IO; // Для StreamWriter и File
using System.Linq; // Для Select и Where
using System.Windows;
using System.Windows.Controls; // Для CheckBox

namespace ProjectStructureAnalyzer
{
    public partial class SettingsWindow : Window
    {
        private readonly string logFilePath = "settings_log.txt";

        public SettingsWindow()
        {
            InitializeComponent();
            Log("Запуск приложения");
            LoadSettings();
        }

        private void LoadSettings()
        {
            Log(""); // Пустая строка для разделения этапов
            Log("Загрузка настроек");

            var folderExclusions = (Properties.Settings.Default.FolderFilters ?? "").Split(',')
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToArray();
            var fileExclusions = (Properties.Settings.Default.FileFilters ?? "").Split(',')
                .Select(f => f.Trim().ToLower())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToArray();

            // BREAKPOINT: Проверка загружаемых значений исключений
            FolderSrcCheckBox.IsChecked = folderExclusions.Contains("src");
            FolderBinCheckBox.IsChecked = folderExclusions.Contains("bin");
            FolderObjCheckBox.IsChecked = folderExclusions.Contains("obj");
            FolderPropertiesCheckBox.IsChecked = folderExclusions.Contains("Properties");

            FileCsCheckBox.IsChecked = fileExclusions.Contains(".cs");
            FileSlnCheckBox.IsChecked = fileExclusions.Contains(".sln");
            FileCsprojCheckBox.IsChecked = fileExclusions.Contains(".csproj");
            FileConfigCheckBox.IsChecked = fileExclusions.Contains(".config");

            Log($"Загружены настройки: FolderExclusions={string.Join(",", folderExclusions)}, FileExclusions={string.Join(",", fileExclusions)}");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Log(""); // Пустая строка для разделения этапов
            Log("Сохранение и применение настроек");

            // Детальная проверка состояния каждого чекбокса
            bool binChecked = FolderBinCheckBox.IsChecked ?? false;
            bool objChecked = FolderObjCheckBox.IsChecked ?? false;
            Log($"Состояние чекбоксов: FolderBinCheckBox={binChecked}, FolderObjCheckBox={objChecked}");

            var folderExclusions = new[] { "src", "bin", "obj", "Properties" }
                .Where(f =>
                    (f == "bin" && FolderBinCheckBox.IsChecked == true) ||
                    (f == "obj" && FolderObjCheckBox.IsChecked == true) ||
                    (f == "src" && FolderSrcCheckBox.IsChecked == true) ||
                    (f == "Properties" && FolderPropertiesCheckBox.IsChecked == true))
                .ToArray();
            var fileExclusions = new[] { ".cs", ".sln", ".csproj", ".config" }
                .Where(f =>
                    (f == ".cs" && FileCsCheckBox.IsChecked == true) ||
                    (f == ".sln" && FileSlnCheckBox.IsChecked == true) ||
                    (f == ".csproj" && FileCsprojCheckBox.IsChecked == true) ||
                    (f == ".config" && FileConfigCheckBox.IsChecked == true))
                .ToArray();

            // BREAKPOINT: Отслеживание изменения состояния чекбоксов
            Log($"Состояние чекбоксов (после фильтрации): FolderExclusions={string.Join(",", folderExclusions)}, FileExclusions={string.Join(",", fileExclusions)}");

            // Разложение присвоений на промежуточные шаги
            string folderFilterString = string.Join(",", folderExclusions); // BREAKPOINT: Проверка объединения папок
            string fileFilterString = string.Join(",", fileExclusions);     // BREAKPOINT: Проверка объединения файлов
            Log($"Промежуточные строки: FolderFilterString={folderFilterString}, FileFilterString={fileFilterString}");

            Properties.Settings.Default.FolderFilters = folderFilterString; // BREAKPOINT: Присвоение FolderFilters
            Properties.Settings.Default.FileFilters = fileFilterString;    // BREAKPOINT: Присвоение FileFilters
            Log($"Присвоены значения: FolderFilters={Properties.Settings.Default.FolderFilters}, FileFilters={Properties.Settings.Default.FileFilters}");

            try
            {
                // BREAKPOINT: Перед вызовом Save для проверки значений
                Properties.Settings.Default.Save();
                Log($"Настройки сохранены: FolderFilters={Properties.Settings.Default.FolderFilters}, FileFilters={Properties.Settings.Default.FileFilters}");
            }
            catch (Exception ex)
            {
                // BREAKPOINT: Проверка ошибок при сохранении
                Log($"Ошибка сохранения настроек: {ex.Message}");
                MessageBox.Show($"Ошибка при сохранении настроек: {ex.Message}");
            }

            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
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
    }
}