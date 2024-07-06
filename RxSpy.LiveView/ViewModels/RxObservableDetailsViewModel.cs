using System.Reactive.Linq;
using System.Text;
using DynamicData.Binding;
using ReactiveUI;
using RxSpy.Extensions;
using RxSpy.Models;

namespace RxSpy.ViewModels;

public class RxSpyObservableDetailsViewModel : ReactiveObject
{
    private RxSpyObservableModel _model;

    readonly ObservableAsPropertyHelper<IObservableCollection<RxSpyObservedValueViewModel>> _observedValues;
    public IObservableCollection<RxSpyObservedValueViewModel> ObservedValues => _observedValues.Value;

    readonly ObservableAsPropertyHelper<bool> _valuesGridIsEnabled;
    public bool ValuesGridIsEnabled => _valuesGridIsEnabled.Value;

    readonly ObservableAsPropertyHelper<RxSpyObservablesGridViewModel> _parents;
    public RxSpyObservablesGridViewModel Parents => _parents.Value;

    readonly ObservableAsPropertyHelper<RxSpyObservablesGridViewModel> _children;
    public RxSpyObservablesGridViewModel Children => _children.Value;

    readonly ObservableAsPropertyHelper<bool> _showErrorTab;
    public bool ShowErrorTab => _showErrorTab.Value;

    readonly ObservableAsPropertyHelper<string> _errorText;
    public string ErrorText => _errorText.Value;

    public RxSpyObservableDetailsViewModel(RxSpyObservableModel model)
    {
        _model = model;

        this.WhenAnyValue(x => x._model.ObservedValues)
            .Select(x => x.CreateDerivedCollection(m => new RxSpyObservedValueViewModel(m)))
            .Scan((prev, cur) => { return cur; })
            .ToProperty(this, x => x.ObservedValues, out _observedValues);

        this.WhenAnyValue(x => x._model.Parents, x => new RxSpyObservablesGridViewModel(x))
            .ToProperty(this, x => x.Parents, out _parents);

        this.WhenAnyValue(x => x._model.Children, x => new RxSpyObservablesGridViewModel(x))
            .ToProperty(this, x => x.Children, out _children);

        this.WhenAnyValue(x => x._model.HasError)
            .ToProperty(this, x => x.ShowErrorTab, out _showErrorTab);

        this.WhenAnyValue(x => x._model.Error, FormatErrorText)
            .ToProperty(this, x => x.ErrorText, out _errorText);
    }

    string FormatErrorText(RxSpyErrorModel err)
    {
        if (err == null)
            return "Nope, you're good";

        var sb = new StringBuilder();

        sb.AppendLine("Received: " + err.Received);
        sb.AppendLine(err.ErrorType.Namespace + err.ErrorType.Name);
        sb.AppendLine();
        sb.AppendLine(err.Message);

        if (!string.IsNullOrEmpty(err.StackTrace))
        {
            sb.AppendLine();
            sb.AppendLine("Stacktrace: ");
            sb.AppendLine(err.StackTrace);
        }

        return sb.ToString();
    }
}