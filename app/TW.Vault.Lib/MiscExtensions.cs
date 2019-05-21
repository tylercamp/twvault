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

        private static Dictionary<String, char> UrlDecodeMap = new Dictionary<string, char>
        {
            { "%7E", '~' },
            { "%80", '€' },
            { "%82", '‚' },
            { "%83", 'ƒ' },
            { "%84", '„' },
            { "%85", '…' },
            { "%86", '†' },
            { "%87", '‡' },
            { "%88", 'ˆ' },
            { "%89", '‰' },
            { "%8A", 'Š' },
            { "%8B", '‹' },
            { "%8C", 'Œ' },
            { "%8E", 'Ž' },
            { "%91", '‘' },
            { "%92", '’' },
            { "%93", '“' },
            { "%94", '”' },
            { "%95", '•' },
            { "%96", '–' },
            { "%97", '—' },
            { "%98", '˜' },
            { "%99", '™' },
            { "%9A", 'š' },
            { "%9B", '›' },
            { "%9C", 'œ' },
            { "%9E", 'ž' },
            { "%9F", 'Ÿ' },
            { "%A1", '¡' },
            { "%A2", '¢' },
            { "%A3", '£' },
            { "%A5", '¥' },
            { "%A6", '|' },
            { "%A7", '§' },
            { "%A8", '¨' },
            { "%A9", '©' },
            { "%AA", 'ª' },
            { "%AB", '«' },
            { "%AC", '¬' },
            { "%AD", '¯' },
            { "%AE", '®' },
            { "%AF", '¯' },
            { "%B0", 'º' },
            { "%B1", '±' },
            { "%B2", 'ª' },
            { "%B3", '³' },
            { "%B4", ',' },
            { "%B5", 'µ' },
            { "%B6", '¶' },
            { "%B7", '·' },
            { "%B8", '¸' },
            { "%B9", '¹' },
            { "%BA", 'º' },
            { "%BB", '»' },
            { "%BC", '¼' },
            { "%BD", '½' },
            { "%BE", '¾' },
            { "%BF", '¿' },
            { "%C0", 'À' },
            { "%C1", 'Á' },
            { "%C2", 'Â' },
            { "%C3", 'Ã' },
            { "%C4", 'Ä' },
            { "%C5", 'Å' },
            { "%C6", 'Æ' },
            { "%C7", 'Ç' },
            { "%C8", 'È' },
            { "%C9", 'É' },
            { "%CA", 'Ê' },
            { "%CB", 'Ë' },
            { "%CC", 'Ì' },
            { "%CD", 'Í' },
            { "%CE", 'Î' },
            { "%CF", 'Ï' },
            { "%D0", 'Ð' },
            { "%D1", 'Ñ' },
            { "%D2", 'Ò' },
            { "%D3", 'Ó' },
            { "%D4", 'Ô' },
            { "%D5", 'Õ' },
            { "%D6", 'Ö' },
            { "%D8", 'Ø' },
            { "%D9", 'Ù' },
            { "%DA", 'Ú' },
            { "%DB", 'Û' },
            { "%DC", 'Ü' },
            { "%DD", 'Ý' },
            { "%DE", 'Þ' },
            { "%DF", 'ß' },
            { "%E0", 'à' },
            { "%E1", 'á' },
            { "%E2", 'â' },
            { "%E3", 'ã' },
            { "%E4", 'ä' },
            { "%E5", 'å' },
            { "%E6", 'æ' },
            { "%E7", 'ç' },
            { "%E8", 'è' },
            { "%E9", 'é' },
            { "%EA", 'ê' },
            { "%EB", 'ë' },
            { "%EC", 'ì' },
            { "%ED", 'í' },
            { "%EE", 'î' },
            { "%EF", 'ï' },
            { "%F0", 'ð' },
            { "%F1", 'ñ' },
            { "%F2", 'ò' },
            { "%F3", 'ó' },
            { "%F4", 'ô' },
            { "%F5", 'õ' },
            { "%F6", 'ö' },
            { "%F7", '÷' },
            { "%F8", 'ø' },
            { "%F9", 'ù' },
            { "%FA", 'ú' },
            { "%FB", 'û' },
            { "%FC", 'ü' },
            { "%FD", 'ý' },
            { "%FE", 'þ' },
            { "%FF", 'ÿ' }
        };
        private static Dictionary<char, String> UrlEncodeMap =
            UrlDecodeMap
                .GroupBy(kvp => kvp.Value)
                .Select(g => new { g.Key, Value = g.First().Key })
                .ToDictionary(p => p.Key, p => p.Value);

        public static String UrlEncode(this String str)
        {
            if (str == null)
                return null;

            var result = WebUtility.UrlEncode(str);
            foreach (var (ch, code) in UrlEncodeMap.Tupled())
                result = result.Replace(ch.ToString(), code);
            return result;
        }

        public static String UrlDecode(this String str)
        {
            if (str == null)
                return null;

            var result = str;
            foreach (var (code, ch) in UrlDecodeMap.Tupled())
                result = result.Replace(code, ch.ToString());

            return WebUtility.UrlDecode(result);
        }
    }
}
