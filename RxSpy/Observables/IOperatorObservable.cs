using RxSpy.Protobuf.Events;

namespace RxSpy.Observables;

public interface IOperatorObservable
{
    OperatorInfo OperatorInfo { get; }
}