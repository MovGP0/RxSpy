using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RxSpy.Models;

namespace RxSpy.ViewModels;

public class RxSpyObservablesGridViewModel : ReactiveObject
{
    [Reactive]
    public IReactiveDerivedList<RxSpyObservableGridItemViewModel> Observables { get; set; }

    [Reactive]
    public RxSpyObservableGridItemViewModel SelectedItem { get; set; }

    public RxSpyObservablesGridViewModel(IReadOnlyReactiveList<RxSpyObservableModel> model)
    {
        Observables = model.CreateDerivedCollection(x => new RxSpyObservableGridItemViewModel(x));
    }
}