using ProjectStructureAnalyzer.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProjectStructureAnalyzer
{
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            DataContext = viewModel;
            viewModel.ShowFolderSelectionHint += OnShowFolderSelectionHint;
            ApplySettingsFromJson(); // Применяем настройки из default.json
        }

        public MainViewModel ViewModel => viewModel;

        private void OnShowFolderSelectionHint()
        {
            ShowFolderSelectionHintMessage();
            AnimateSelectFolderButton();
        }

        private void ShowFolderSelectionHintMessage()
        {
            MessageBox.Show(
                "Пожалуйста, сначала выберите папку для анализа с помощью кнопки 'Выбрать папку'.",
                "Папка не выбрана",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void AnimateSelectFolderButton()
        {
            if (FindButtonByContent("Выбрать папку") is Button selectButton)
            {
                var colorAnimation = new ColorAnimation
                {
                    From = Colors.Transparent,
                    To = Colors.LightBlue,
                    Duration = TimeSpan.FromMilliseconds(500),
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(2)
                };

                var brush = new SolidColorBrush(Colors.Transparent);
                selectButton.Background = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
        }

        private Button FindButtonByContent(string content)
        {
            return FindVisualChildren<Button>(this)
                .FirstOrDefault(b => b.Content?.ToString() == content);
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        #region Window Management Event Handlers

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

        #endregion

        #region UI Event Handlers

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow { Owner = this };
            aboutWindow.ShowDialog();
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is TreeViewItem item && item.DataContext is ProjectItem projectItem)
                {
                    if (viewModel != null)
                    {
                        var itemType = projectItem.IsDirectory ? "Папка" : "Файл";
                        viewModel.StatusText = $"Выбран: {itemType} - {projectItem.Name}";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error handling TreeView selection", ex);
                if (viewModel != null)
                {
                    viewModel.StatusText = "Ошибка при выборе элемента";
                }
            }
        }

        #endregion

        #region Settings Management

        public void ApplySettingsFromJson()
        {
            try
            {
                // Убедимся, что настройки загружены в viewModel
                if (viewModel.AppSettings == null || viewModel.AppSettings.UserInterface == null)
                {
                    viewModel.ReloadSettings(); // Перезагрузка настроек, если они не инициализированы
                }

                var uiSettings = viewModel.AppSettings.UserInterface;
                if (uiSettings == null)
                {
                    ApplyDefaultSettings();
                    return;
                }

                ApplyFontSettings(uiSettings.FontFamily, uiSettings.FontSize);
                ApplyWindowSettings(uiSettings);
                UpdateStatusAfterSettingsApplied();
                Logger.LogInfo("JSON settings applied successfully to MainWindow from default.json");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error applying JSON settings to MainWindow", ex);
                MessageBox.Show($"Ошибка применения настроек из JSON: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                ApplyDefaultSettings();
            }
        }

        private void UpdateStatusAfterSettingsApplied()
        {
            if (viewModel != null)
            {
                if (!string.IsNullOrEmpty(viewModel.SelectedPath))
                {
                    viewModel.StatusText = "Настройки из default.json применены. Последняя папка загружена из настроек.";
                }
                else
                {
                    viewModel.StatusText = "Настройки из default.json загружены успешно. Выберите папку для анализа.";
                }
            }
        }

        private void ApplyFontSettings(string fontFamily, double fontSize)
        {
            try
            {
                if (!string.IsNullOrEmpty(fontFamily))
                {
                    this.FontFamily = new FontFamily(fontFamily);
                }
                if (fontSize > 0)
                {
                    this.FontSize = fontSize;
                }
                Logger.LogInfo($"Font settings applied: {fontFamily}, size: {fontSize} (from default.json)");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error applying font settings: {fontFamily}, {fontSize}", ex);
                // Удаляем резервные значения, чтобы использовать дефолт из default.json
                this.FontFamily = !string.IsNullOrEmpty(fontFamily) ? new FontFamily(fontFamily) : new FontFamily("Segoe UI");
                this.FontSize = fontSize > 0 ? fontSize : 10; // Используем 10 как резервное значение из default.json
            }
        }

        private void ApplyWindowSettings(UserInterfaceSettings uiSettings)
        {
            try
            {
                if (uiSettings.WindowWidth > 0 && uiSettings.WindowHeight > 0)
                {
                    this.Width = uiSettings.WindowWidth;
                    this.Height = uiSettings.WindowHeight;
                }
                if (Enum.TryParse<WindowState>(uiSettings.WindowState, out var windowState))
                {
                    this.WindowState = windowState;
                }
                Logger.LogInfo($"Window settings applied: {uiSettings.WindowWidth}x{uiSettings.WindowHeight}, state: {uiSettings.WindowState}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error applying window settings", ex);
                this.Width = 1000;
                this.Height = 700;
                this.WindowState = WindowState.Normal;
            }
        }

        private void ApplyDefaultSettings()
        {
            try
            {
                this.FontFamily = new FontFamily("Segoe UI");
                this.FontSize = 10; // Используем значение из default.json
                this.Width = 1000;
                this.Height = 700;
                this.WindowState = WindowState.Normal;
                if (viewModel != null)
                {
                    viewModel.StatusText = "Применены настройки по умолчанию из-за ошибки загрузки default.json.";
                }
                Logger.LogInfo("Default settings applied");
            }
            catch (Exception ex)
            {
                Logger.LogError("Error applying default settings", ex);
            }
        }

        #endregion

        #region Window State Management

        private void SaveWindowState()
        {
            try
            {
                if (viewModel?.AppSettings?.UserInterface != null)
                {
                    var uiSettings = viewModel.AppSettings.UserInterface;
                    if (this.WindowState != WindowState.Minimized)
                    {
                        uiSettings.WindowWidth = this.Width;
                        uiSettings.WindowHeight = this.Height;
                        uiSettings.WindowState = this.WindowState.ToString();
                    }
                    viewModel.SaveCurrentSettings();
                    Logger.LogInfo("Window state saved successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error saving window state", ex);
            }
        }

        #endregion

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                SaveWindowState();
                if (viewModel != null)
                {
                    viewModel.ShowFolderSelectionHint -= OnShowFolderSelectionHint;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error during window cleanup", ex);
            }
            base.OnClosed(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            try
            {
                if (viewModel?.AppSettings?.UserInterface != null)
                {
                    viewModel.AppSettings.UserInterface.WindowState = this.WindowState.ToString();
                }
                base.OnStateChanged(e);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error handling window state change", ex);
                base.OnStateChanged(e);
            }
        }
    }
}