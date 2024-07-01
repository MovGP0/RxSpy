using System.Reactive.Linq;
using System.Windows;
using ReactiveUI;
using RxSpy.Communication;
using RxSpy.Models;
using RxSpy.ViewModels;
using RxSpy.Views;
using RxSpy.Views.Controls;
using Splat;
using MessageBox = System.Windows.MessageBox;

namespace RxSpy.AppStartup;

public static class StartupSequence
{
    public static void Start()
    {
        var args = Environment.GetCommandLineArgs();

        var address = args.Length > 1
            ? new Uri(args[1])
            : new Uri("http://localhost:65073/rxspy/");

        var client = new RxSpyHttpClient();

        var session = new RxSpySessionModel();

        client.Connect(address, TimeSpan.FromSeconds(5))
            .Where(x => x != null)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(session.OnEvent, ex => { MessageBox.Show(App.Current.MainWindow, "Lost connection to host", "Host disconnected", MessageBoxButton.OK, MessageBoxImage.Error); });

        var mainViewModel = new MainViewModel(session);

        Locator.CurrentMutable.RegisterConstant(mainViewModel, typeof(MainViewModel));
        Locator.CurrentMutable.Register(() => new MainView(), typeof(IViewFor<MainViewModel>));
        Locator.CurrentMutable.Register(() => new TrackedObservablesGrid(), typeof(IViewFor<RxSpyObservablesGridViewModel>));
        Locator.CurrentMutable.Register(() => new ObservableDetails(), typeof(IViewFor<RxSpyObservableDetailsViewModel>));
    }
}