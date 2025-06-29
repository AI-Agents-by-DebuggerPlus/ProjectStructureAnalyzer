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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Создаем ViewModel и устанавливаем как DataContext
            viewModel = new MainViewModel();
            DataContext = viewModel;

            // Подписываемся на событие показа подсказки
            viewModel.ShowFolderSelectionHint += OnShowFolderSelectionHint;

            // Применяем сохраненные настройки при запуске
            ApplySettings();
        }

        /// <summary>
        /// Обработчик события показа подсказки о выборе папки
        /// </summary>
        private void OnShowFolderSelectionHint()
        {
            // Показываем подсказку пользователю
            ShowFolderSelectionHintMessage();

            // Опционально: анимируем кнопку выбора папки
            AnimateSelectFolderButton();
        }

        /// <summary>
        /// Показывает сообщение-подсказку о необходимости выбрать папку
        /// </summary>
        private void ShowFolderSelectionHintMessage()
        {
            MessageBox.Show(
                "Пожалуйста, сначала выберите папку для анализа с помощью кнопки 'Выбрать папку'.",
                "Папка не выбрана",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        /// <summary>
        /// Анимирует кнопку выбора папки для привлечения внимания
        /// </summary>
        private void AnimateSelectFolderButton()
        {
            // Найдем кнопку выбора папки (замените имя на актуальное из вашего XAML)
            if (FindName("SelectFolderButton") is Button selectButton)
            {
                // Создаем анимацию подсветки
                var colorAnimation = new ColorAnimation
                {
                    From = Colors.Transparent,
                    To = Colors.LightBlue,
                    Duration = TimeSpan.FromMilliseconds(500),
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(2)
                };

                // Применяем анимацию к фону кнопки
                var brush = new SolidColorBrush(Colors.Transparent);
                selectButton.Background = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }
        }

        #region Window Management Event Handlers

        /// <summary>
        /// Обработчик события MouseDown для заголовка окна - позволяет перетаскивать окно
        /// </summary>
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        /// <summary>
        /// Обработчик кнопки сворачивания окна
        /// </summary>
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Обработчик кнопки максимизации/восстановления окна
        /// </summary>
        private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        /// <summary>
        /// Обработчик кнопки закрытия окна
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region UI Event Handlers

        /// <summary>
        /// Обработчик кнопки "О программе"
        /// </summary>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this // Устанавливаем MainWindow как владельца для центрирования
            };
            aboutWindow.ShowDialog();
        }

        /// <summary>
        /// Обработчик выбора элемента в TreeView
        /// </summary>
        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранный элемент
                if (sender is TreeViewItem item && item.DataContext is ProjectItem projectItem)
                {
                    // Обновляем статус с информацией о выбранном элементе
                    if (viewModel != null)
                    {
                        var itemType = projectItem.IsDirectory ? "Папка" : "Файл";
                        viewModel.StatusText = $"Выбран: {itemType} - {projectItem.Name}";
                    }
                }
            }
            catch
            {
                // Логируем ошибку если есть система логирования
                if (viewModel != null)
                {
                    viewModel.StatusText = "Ошибка при выборе элемента";
                }

                // Можно добавить логирование:
                // Logger.LogError("Error handling TreeView selection");
            }
        }

        #endregion

        #region Settings Management

        /// <summary>
        /// Применяет сохраненные настройки к окну
        /// </summary>
        public void ApplySettings()
        {
            try
            {
                // Применяем настройки шрифта
                var fontFamily = Properties.Settings.Default.ApplicationFontFamily;
                if (!string.IsNullOrEmpty(fontFamily))
                {
                    try
                    {
                        this.FontFamily = new FontFamily(fontFamily);
                    }
                    catch
                    {
                        this.FontFamily = new FontFamily("Segoe UI");
                    }
                }

                // Применяем размер шрифта
                var fontSize = Properties.Settings.Default.ApplicationFontSize;
                if (fontSize > 0)
                {
                    this.FontSize = fontSize;
                }

                // Загружаем фильтры в ViewModel
                if (viewModel != null)
                {
                    viewModel.FolderFilters = Properties.Settings.Default.FolderFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim()).ToList() ?? new List<string>();
                    viewModel.FileFilters = Properties.Settings.Default.FileFilters?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(f => f.Trim()).ToList() ?? new List<string>();

                    if (viewModel.ProjectItems?.Any() == true)
                    {
                        // Повторный анализ с новыми фильтрами
                        viewModel.AnalyzeCommand?.Execute(null);
                        viewModel.StatusText = "Настройки применены. Выполнен повторный анализ с новыми фильтрами.";
                    }
                    else
                    {
                        viewModel.StatusText = "Настройки применены. Выберите папку для анализа.";
                    }
                }
            }
            catch (Exception ex)
            {
                // Обрабатываем ошибки применения настроек
                MessageBox.Show($"Ошибка применения настроек: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);

                // Восстанавливаем значения по умолчанию
                this.FontFamily = new FontFamily("Segoe UI");
                this.FontSize = 13;
                if (viewModel != null)
                {
                    viewModel.StatusText = "Настройки сброшены на значения по умолчанию из-за ошибки.";
                }

                // Можно добавить логирование:
                // Logger.LogError("Error applying settings", ex);
            }
        }

        #endregion

        /// <summary>
        /// Освобождение ресурсов при закрытии окна
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Отписываемся от событий для предотвращения утечек памяти
                if (viewModel != null)
                {
                    viewModel.ShowFolderSelectionHint -= OnShowFolderSelectionHint;
                }
            }
            catch
            {
                // Игнорируем ошибки при очистке ресурсов
            }

            base.OnClosed(e);
        }
    }
}