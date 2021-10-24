using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TW.Vault.Lib.Security;

namespace TW.Vault.Lib.Features
{
    public class InvalidStringEncryptionException : Exception
    {
        String encryptedString;

        public InvalidStringEncryptionException(String encryptedString)
        {
            this.encryptedString = encryptedString;

            if (this.encryptedString.Length > 50)
                this.encryptedString = this.encryptedString.Substring(0, 47) + "...";
        }

        public override string Message => "The given text was not encrypted with any active seeds: " + encryptedString;
    }

    public static class Encryption
    {
        public static String Decrypt(String text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Note: All decrypted text should start with "vault:"; this is so we know if
            //       the text was properly decrypted. Otherwise we could get a decrypted
            //       string of junk text when using an old key, which is still technically "valid text"

            String result = null;

            if (Configuration.Security.Encryption.UseEncryption)
            {
                var seeds = EncryptionSeedProvider.AvailableSeeds;
                foreach (var seed in seeds)
                {
                    try
                    {
                        result = DecryptWithSeed(text, seed);
                    }
                    catch
                    {
                        result = null;
                    }

                    if (!string.IsNullOrWhiteSpace(result) && result.StartsWith("vault:"))
                        break;
                    else
                        result = null;
                }

                if (result == null)
                {
                    Serilog.Log.Debug("Couldn't decrypt with any of these seeds: " + string.Join(',', seeds) + " at timestamp: " + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds());
                    throw new InvalidStringEncryptionException(text);
                }

                result = result.Substring("vault:".Length);
            }
            else
            {
                result = text;
            }

            return result;
        }

        public static String DecryptWithSeed(String text, uint seed)
        {
            //  Assume texts starts with "ve_"
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
