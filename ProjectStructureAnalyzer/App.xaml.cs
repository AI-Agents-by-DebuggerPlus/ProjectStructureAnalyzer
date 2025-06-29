using System.Windows;

namespace ProjectStructureAnalyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.ClearLogFile(); // Очистка лог-файла при запуске
            base.OnStartup(e);
        }
    }
}