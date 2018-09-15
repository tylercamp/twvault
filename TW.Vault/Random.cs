using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault
{
    public static class Random
    {
        public static System.Random Instance { get; } = new System.Random();

        public static long NextLong
        {
            get
            {
                long result = 0;
                result |= ((long)Instance.Next()) << 0;
                result |= ((long)Instance.Next()) << 32;

                return result;
            }
        }
    }
}
