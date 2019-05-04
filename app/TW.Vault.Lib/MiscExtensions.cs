using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace TW.Vault
{
    public static class MiscExtensions
    {
        public static Tuple<A, B> Tupled<A, B>(this KeyValuePair<A, B> kvp) => new Tuple<A, B>(kvp.Key, kvp.Value);
        public static IEnumerable<Tuple<A, B>> Tupled<A, B>(this Dictionary<A, B> dict) => dict.Select(Tupled);
        public static String Capitalized(this String str)
        {
            if (String.IsNullOrWhiteSpace(str))
                return str;
            else if (str.Length == 1)
                return str.ToUpper();
            else
                return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static IEnumerable<IEnumerable<T>> Grouped<T>(this IEnumerable<T> enumerable, int groupSize)
        {
            var enumerator = enumerable.GetEnumerator();
            List<T> workingGroup = null;
            while (enumerator.MoveNext())
            {
                if (workingGroup == null)
                {
                    workingGroup = new List<T> { enumerator.Current };
                    continue;
                }

                if (workingGroup.Count >= groupSize)
                {
                    yield return workingGroup;
                    workingGroup = new List<T> { enumerator.Current };
                }
                else
                {
                    workingGroup.Add(enumerator.Current);
                }
            }

            if (workingGroup != null)
                yield return workingGroup;
        }

        public static IEnumerable<IEnumerable<T>> GroupWhile<T>(this IEnumerable<T> enumerable, Func<T, T, bool> predicate)
        {
            var enumerator = enumerable.GetEnumerator();
            List<T> workingGroup = null;
            while (enumerator.MoveNext())
            {
                if (workingGroup == null)
                {
                    workingGroup = new List<T> { enumerator.Current };
                    continue;
                }

                var previous = workingGroup.Last();
                var current = enumerator.Current;
                if (predicate(previous, current))
                    workingGroup.Add(current);
                else
                    yield return workingGroup;
            }

            if (workingGroup != null)
                yield return workingGroup;
        }

        public static String UrlEncode(this String str)
        {
            if (str == null)
                return null;
            else
                return WebUtility.UrlEncode(str)
                    .Replace("*", "%2A")
                    .Replace("(", "%28")
                    .Replace(")", "%29")
                    .Replace("!", "%21");
        }

        public static String UrlDecode(this String str)
        {
            if (str == null)
                return null;
            else
                return WebUtility.UrlDecode(str
                    .Replace("%2A", "*")
                    .Replace("%28", "(")
                    .Replace("%29", ")")
                    .Replace("%21", "!")
                );
        }
    }
}
