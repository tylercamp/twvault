using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TW.Vault.Security
{
    /// <summary>
    /// Provides seed values for encryption, which change at a specific interval.
    /// </summary>
    public static class EncryptionSeedProvider
    {
        /* These values are reflected in encryption.js and should be kept in sync */
        //  How often the interval will be changed
        private static readonly TimeSpan SwapInterval = TimeSpan.FromSeconds(5);
        //  How long the previous interval should be valid for (in case there's lag between requests)
        private static readonly TimeSpan MaxIntervalLag = TimeSpan.FromSeconds(15);

        private static readonly uint SeedSalt = 0x8D760E23;
        
        public static uint CurrentSeed => MakeSeed(CurrentTime);

        public static List<uint> AvailableSeeds
        {
            get
            {
                var now = CurrentTime;

                var result = new List<uint>();
                var currentSeed = MakeSeed(now);
                result.Add(currentSeed);


                for (var previous = now - SwapInterval; previous >= now - MaxIntervalLag || previous == now - SwapInterval; previous -= SwapInterval)
                {
                    var previousSeed = MakeSeed(previous);
                    if (!result.Contains(previousSeed))
                        result.Add(previousSeed);
                }
                
                return result;
            }
        }

        private static DateTime CurrentTime => DateTime.UtcNow;

        private static uint MakeSeed(DateTime referenceTime)
        {
            ulong currentLongInterval = (ulong)(new DateTimeOffset(referenceTime).ToUnixTimeMilliseconds() / (long)SwapInterval.TotalMilliseconds);
            uint currentInterval = (uint)(currentLongInterval % 0xFFFFFFFF);
            return Randomizer.Randomize(currentInterval) ^ SeedSalt;
        }




        private static class Randomizer
        {
            private const uint RandomPrime = 2035567511;

            public static uint Randomize(uint x)
            {
                uint result = x;
                result <<= 13;
                result *= RandomPrime;
                result %= RandomPrime;
                result ^= x;
                result ^= RandomPrime;
                return result;
            }
        }
    }
}
