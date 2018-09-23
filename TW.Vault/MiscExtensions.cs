using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault
{
    public static class MiscExtensions
    {
        public static Tuple<A, B> Tupled<A, B>(this KeyValuePair<A, B> kvp) => new Tuple<A, B>(kvp.Key, kvp.Value);
        public static IEnumerable<Tuple<A, B>> Tupled<A, B>(this Dictionary<A, B> dict) => dict.Select(Tupled);
    }
}
