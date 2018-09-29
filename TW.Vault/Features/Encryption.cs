using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TW.Vault.Security;

namespace TW.Vault.Features
{
    public static class Encryption
    {
        public static String Decrypt(String text)
        {
            String result = null;
            var seeds = EncryptionSeedProvider.AvailableSeeds;
            foreach (var seed in seeds)
            {
                result = DecryptWithSeed(text, seed);
                if (result != null)
                    break;
            }

            if (result == null)
                throw new InvalidOperationException("The given text was not encrypted with any active seeds");

            return result;
        }

        public static String DecryptWithSeed(String text, uint seed)
        {
            if (!text.StartsWith("ve_"))
                return text;

            text = text.Substring(3);

            var unswizzledBuilder = new StringBuilder();
            var swizzleSizes = SwizzleSizesFromSeed(seed);
            int i = 0;

            for (int si = 0; i < text.Length; si = (si + 1) % swizzleSizes.Count)
            {
                var swizzle = swizzleSizes[si];
                int partLength = Math.Min(swizzle, text.Length - i);
                var reversedPart = text.Substring(i, partLength);
                //  Text is sent reversed and grouped by swizzle size based on the seed,
                //  un-reverse in this swizzle group
                var part = String.Join("", reversedPart.Reverse());
                unswizzledBuilder.Append(part);
                i += swizzle;
            }

            var unswizzled = unswizzledBuilder.ToString();
            var uncompressed = LZStringCSharp.LZString.DecompressFromEncodedURIComponent(unswizzled);
            return uncompressed;
        }

        private static List<int> SwizzleSizesFromSeed(uint seed)
        {
            var result = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                uint nibble = (seed >> (i * 4)) & 0xF;
                result.Add((int)(nibble / 3 + 2));
            }
            return result;
        }
    }
}
