using System.Reactive.Linq;
using ReactiveUI;
using RxSpy.Models;

namespace RxSpy.ViewModels;

public sealed class RxSpyObservableGridItemViewModel : ReactiveObject
{
    public RxSpyObservableModel Model { get; private set; }

    readonly ObservableAsPropertyHelper<long> _id;
    public long Id => _id.Value;

    readonly ObservableAsPropertyHelper<string> _name;
    public string Name => _name.Value;

    readonly ObservableAsPropertyHelper<string> _tag;
    public string Tag => _tag.Value;

    readonly ObservableAsPropertyHelper<long> _valuesProduced;
    public long ValuesProduced => _valuesProduced.Value;

    readonly ObservableAsPropertyHelper<int> _parents;
    public int Parents => _parents.Value;

    readonly ObservableAsPropertyHelper<int> _children;
    public int Children => _children.Value;

    readonly ObservableAsPropertyHelper<int> _ancestors;
    public int Ancestors => _ancestors.Value;

    readonly ObservableAsPropertyHelper<int> _descendants;
    public int Descendants => _descendants.Value;

    readonly ObservableAsPropertyHelper<int> _totalSubscriptions;
    public int TotalSubscriptions => _totalSubscriptions.Value;

    readonly ObservableAsPropertyHelper<string> _callSite;
    public string CallSite => _callSite.Value;

    readonly ObservableAsPropertyHelper<string> _status;
    public string Status => _status.Value;

    public RxSpyObservableGridItemViewModel(RxSpyObservableModel model)
    {
        Model = model;

        this.WhenAnyValue(x => x.Model.Id)
            .ToProperty(this, x => x.Id, out _id);

        this.WhenAnyValue(x => x.Model.Name)
            .ToProperty(this, x => x.Name, out _name);

        this.WhenAnyValue(x => x.Model.Tag)
            .ToProperty(this, x => x.Tag, out _tag);

        this.WhenAnyValue(x => x.Model.ValuesProduced)
            .ToProperty(this, x => x.ValuesProduced, out _valuesProduced);

        this.WhenAnyValue(x => x.Model.Children.Count)
            .ToProperty(this, x => x.TotalSubscriptions, out _totalSubscriptions);

        this.WhenAnyValue(x => x.Model.Parents.Count)
            .ToProperty(this, x => x.Parents, out _parents);

        this.WhenAnyValue(x => x.Model.Children.Count)
            .ToProperty(this, x => x.Children, out _children);

        this.WhenAnyValue(x => x.Model.Ancestors)
            .ToProperty(this, x => x.Ancestors, out _ancestors);

        this.WhenAnyValue(x => x.Model.Descendants)
            .ToProperty(this, x => x.Descendants, out _descendants);

        this.WhenAnyValue(x => x.Model.CallSite)
            .Select(Convert.ToString)
            .ToProperty(this, x => x.CallSite, out _callSite);

        this.WhenAnyValue(x => x.Model.Status)
            .ToProperty(this, x => x.Status, out _status);
    }
}