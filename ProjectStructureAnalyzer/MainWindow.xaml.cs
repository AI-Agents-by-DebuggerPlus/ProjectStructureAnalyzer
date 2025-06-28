using ProjectStructureAnalyzer.Views;
using System;
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
            catch (Exception ex)
            {
                // Логируем ошибку если есть система логирования
                if (viewModel != null)
                {
                    viewModel.StatusText = "Ошибка при выборе элемента";
                }

                // Можно добавить логирование:
                // Logger.LogError("Error handling TreeView selection", ex);
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
                        // Если указанный шрифт недоступен, используем шрифт по умолчанию
                        this.FontFamily = new FontFamily("Segoe UI");
                    }
                }

                // Применяем размер шрифта
                var fontSize = Properties.Settings.Default.ApplicationFontSize;
                if (fontSize > 0)
                {
                    this.FontSize = fontSize;
                }

                // Если нужно обновить анализ с новыми фильтрами
                if (viewModel?.ProjectItems?.Any() == true)
                {
                    // Можно добавить логику для повторного анализа с новыми настройками фильтров
                    // Например, если пользователь изменил фильтры и хочет сразу увидеть результат:
                    // viewModel.AnalyzeCommand?.Execute(null);

                    // Обновляем статус
                    viewModel.StatusText = "Настройки применены. Для применения новых фильтров выполните повторный анализ.";
                }
            }
            catch (Exception ex)
            {
                // Обрабатываем ошибки применения настроек
                MessageBox.Show($"Ошибка применения настроек: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);

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