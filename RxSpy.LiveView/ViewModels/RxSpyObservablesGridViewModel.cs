using System.Collections.ObjectModel;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using RxSpy.Models;
using RxSpy.Extensions;

namespace RxSpy.ViewModels;

public class RxSpyObservablesGridViewModel : ReactiveObject
{
    [Reactive]
    public IObservableCollection<RxSpyObservableGridItemViewModel> Observables { get; set; }

    [Reactive]
    public RxSpyObservableGridItemViewModel SelectedItem { get; set; }

    public RxSpyObservablesGridViewModel(ObservableCollection<RxSpyObservableModel> model)
    {
        Observables = model.CreateDerivedCollection(x => new RxSpyObservableGridItemViewModel(x));
    }
}