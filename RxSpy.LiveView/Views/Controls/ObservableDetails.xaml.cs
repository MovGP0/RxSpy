using System.Windows;
using ReactiveUI;
using RxSpy.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace RxSpy.Views.Controls;

/// <summary>
/// Interaction logic for ObservableDetails.xaml
/// </summary>
public partial class ObservableDetails : UserControl, IViewFor<RxSpyObservableDetailsViewModel>
{
    public ObservableDetails()
    {
        InitializeComponent();

        this.OneWayBind(ViewModel, vm => vm.ObservedValues, v => v.observableValuesGrid.ItemsSource);
        this.OneWayBind(ViewModel, vm => vm.Parents, v => v.parentsView.ViewModel);
        this.OneWayBind(ViewModel, vm => vm.Children, v => v.childrenView.ViewModel);
        this.OneWayBind(ViewModel, vm => vm.ErrorText, v => v.errorText.Text);
    }

    public RxSpyObservableDetailsViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty) as RxSpyObservableDetailsViewModel;
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
        nameof(ViewModel),
        typeof(RxSpyObservableDetailsViewModel),
        typeof(ObservableDetails),
        new PropertyMetadata(null)
    );

    object IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = value as RxSpyObservableDetailsViewModel;
    }
}