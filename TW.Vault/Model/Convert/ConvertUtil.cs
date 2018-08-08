using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Model.Convert
{
    public static class ConvertUtil
    {
        public static T? GetOrNull<T>(IDictionary<String, T> dictionary, String key) where T : struct
        {
            if (dictionary.ContainsKey(key))
                return new T?(dictionary[key]);
            else
                return null;
        }

        public static void AddIfNotNull<T>(IDictionary<String, T> dictionary, String key, T? value) where T : struct
        {
            if (value.HasValue)
                dictionary.Add(key, value.Value);
        }
    }
}
