using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectStructureAnalyzer
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            Logger.ClearLogFile();

            viewModel = new MainViewModel();
            DataContext = viewModel;
            ApplySettings();
        }

        public void ApplySettings()
        {
            Logger.LogInfo("--- Applying settings on MainWindow ---");
            try
            {
                string fontFamilyName = Properties.Settings.Default.ApplicationFontFamily;
                int fontSize = Properties.Settings.Default.ApplicationFontSize;

                Logger.LogInfo($"Read settings: FontFamily='{fontFamilyName}', FontSize='{fontSize}'");

                if (!string.IsNullOrEmpty(fontFamilyName))
                {
                    this.FontFamily = new FontFamily(fontFamilyName);
                    Logger.LogInfo($"Applied FontFamily: {fontFamilyName}");
                }
                if (fontSize > 0)
                {
                    this.FontSize = fontSize;
                    Logger.LogInfo($"Applied FontSize: {fontSize}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error applying settings", ex);
            }
            Logger.LogInfo("--- Finished applying settings ---");
        }

        private void ExpandTreeViewItems(ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {
                if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem treeViewItem)
                {
                    treeViewItem.IsExpanded = true;
                    ExpandTreeViewItems(treeViewItem);
                }
            }
        }

        private void ExpandTreeViewItems(TreeViewItem item)
        {
            item.IsExpanded = true;
            foreach (var subItem in item.Items)
            {
                if (item.ItemContainerGenerator.ContainerFromItem(subItem) is TreeViewItem subTreeViewItem)
                {
                    ExpandTreeViewItems(subTreeViewItem);
                }
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem treeViewItem && treeViewItem.DataContext is ProjectItem selectedItem)
            {
                viewModel.StatusText = $"Выбран: {selectedItem.Name}";
            }
        }

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

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new Views.AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }
    }
}