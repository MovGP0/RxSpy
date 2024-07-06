using RxSpy.AppStartup;
using Application = System.Windows.Application;

namespace RxSpy
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            StartupSequence.Start();
        }
    }
}
