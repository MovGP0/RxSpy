using RxSpy.Entities;

namespace RxSpy.Observables;

public interface IOperatorObservable
{
    OperatorInfo OperatorInfo { get; }
}