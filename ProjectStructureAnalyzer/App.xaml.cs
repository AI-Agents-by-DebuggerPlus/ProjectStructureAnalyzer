using System.Windows;

namespace ProjectStructureAnalyzer
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Настройка глобальных параметров приложения
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }
    }
}