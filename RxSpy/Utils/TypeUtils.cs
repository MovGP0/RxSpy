using System.Collections.Concurrent;

namespace RxSpy.Utils
{
    public static class TypeUtils
    {
        private static readonly Dictionary<Type, string> CsFriendlyTypeNames;

        private static readonly ConcurrentDictionary<Type, Lazy<string>> TypeNameCache = new();

        static TypeUtils()
        {
            CsFriendlyTypeNames = new Dictionary<Type, string>
            {
                { typeof(sbyte), "sbyte" },
                { typeof(byte), "byte" },
                { typeof(short), "short" },
                { typeof(ushort), "ushort" },
                { typeof(int), "int" },
                { typeof(uint), "uint" },
                { typeof(long), "long" },
                { typeof(ulong), "ulong" },
                { typeof(float), "float" },
                { typeof(double), "double" },
                { typeof(bool), "bool" },
                { typeof(char), "char" },
                { typeof(string), "string" },
                { typeof(object), "object" },
                { typeof(decimal), "decimal" }
            };
        }

        public static string ToFriendlyName(Type type)
        {
            var lazy = TypeNameCache.GetOrAdd(type, _ => new Lazy<string>(
                    () => ToFriendlyNameImpl(type),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            return lazy.Value;
        }

        private static string ToFriendlyNameImpl(Type type)
        {
            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return ToFriendlyName(type.GetGenericArguments()[0]) + "?";
                }

                var definition = type.GetGenericTypeDefinition();
                return GetNameWithoutGenerics(definition) + "<" + string.Join(", ", type.GetGenericArguments().Select(ToFriendlyName)) + ">";
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType() ?? typeof(object);
                return ToFriendlyName(elementType) + Repeat("[]", type.GetArrayRank());
            }

            return CsFriendlyTypeNames.TryGetValue(type, out var name)
                ? name
                : type.Name;
        }

        private static string GetNameWithoutGenerics(Type definition)
        {
            var n = definition.Name;
            var p = n.IndexOf('`');

            return p == -1
                ? n
                : n[..p];
        }

        private static string Repeat(string str, int count)
        {
            if (count == 1)
            {
                return str;
            }

            var arr = new string[count];

            for (var i = 0; i < count; i++)
            {
                arr[i] = str;
            }

            return string.Concat(arr);
        }
    }
}
