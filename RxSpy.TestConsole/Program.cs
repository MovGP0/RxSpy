using System.Reactive.Linq;

namespace RxSpy.TestConsole;

internal static class Program
{
    private static void Main()
    {
        RxSpySession.Launch();

        var dummy = new [] { "Foo", "Bar", "Baz" };

        while (true)
        {
            var obs1 = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1));
            var obs2 = obs1.Select(x => dummy[x % dummy.Length]);
            var obs3 = obs1.Select(x => "---");

            var obs4 = obs2.Where(x => x.StartsWith('B'));
            var obsErr = Observable.Throw<string>(new InvalidOperationException()).Catch(Observable.Return(string.Empty));

            var toJoin = new List<IObservable<string>> { obs3, obs4, obsErr };

            var obs5 = toJoin.CombineLatest();
            var obs6 = obs5.Select(x => string.Join(", ", x));

            using (obs6.Subscribe(Console.WriteLine))
            {
                Console.ReadLine();
                Console.WriteLine("Disposing of all observables");
            }

            Console.WriteLine("Press enter to begin again");
            Console.ReadLine();
        }
    }
}