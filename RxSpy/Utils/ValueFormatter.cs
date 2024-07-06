using System.Collections;
using System.Collections.Concurrent;

namespace RxSpy.Utils;

public static class ValueFormatter
{
    private static readonly ConcurrentDictionary<Type, Lazy<Func<object, string>>> CachedFormatters = new();

    public static string ToString(object? value)
    {
        return value is null
            ? "<null>"
            : ToString(value, value.GetType());
    }

    public static string ToString(object? value, Type type)
    {
        if (value is null) return "<null>";
        var formatter = CachedFormatters.GetOrAdd(type, CreateFormatter);
        return formatter.Value(value);
    }

    private static Lazy<Func<object, string>> CreateFormatter(Type type)
        => new(() => BuildFormatterDelegate(type));

    private static Func<object?, string> BuildFormatterDelegate(Type type)
    {
        var typeName = TypeUtils.ToFriendlyName(type);
        
        return o =>
        {
            return o switch
            {
                null => typeName + "<null>",
                string str => str,
                Array { Length: < 15, Rank: 1 } array => FormatShortArray(array, typeName),
                Array { Rank: 1 } array => FormatLongArray(typeName, array),
                IList { Count: < 15 } list => FormatShortList(typeName, list),
                IList list => FormatLongList(typeName, list),
                _ => FormatObject(o, type)
            };
        };
    }

    private static string FormatObject(object obj, Type type)
    {
        if (DebuggerDisplayFormatter.TryGetDebuggerDisplayFormatter(type, out var debuggerDisplayFormatter))
        {
            return debuggerDisplayFormatter.Invoke(obj);
        }

        return Convert.ToString(obj) ?? "null";
    }
    
    private static string FormatLongList(string typeName, IList list)
    {
        return typeName + "[" + list.Count + "]";
    }

    private static string FormatShortList(string typeName, IList list)
    {
        return typeName + " {" + string.Join(", ", list.Cast<object>().Select(ToString)) + "}";
    }

    private static string FormatLongArray(string typeName, Array arr)
    {
        return typeName + "[" + arr.Length + "]";
    }

    private static string FormatShortArray(Array arr, string typeName)
    {
        var elements = new string[arr.Length];

        for (var i = 0; i < arr.Length; i++)
        {
            elements[i] = ToString(arr.GetValue(i));
        }

        return typeName + " {" + string.Join(", ", elements) + "}";
    }
}