function makeEncryption() {

    //  Constants are base64-encoded since JS script obfuscator won't obfuscate numbers
    //  Constants are pulled from EncryptionSeedProvider.cs and should be kept in sync in any changes

    // 0x8D760E23 - From EncryptionSeedProvider.cs
    const SEED_SALT = parseInt(atob("MjM3MzMyNDMyMw=="));
    // 15 seconds (15000 ms) - From EncryptionSeedProvider.cs
    const SEED_SWAP_INTERVAL = parseInt(atob("NTAwMA=="));
    // Random prime number
    const SEED_RANDOM_PRIME = parseInt(atob("MjAzNTU2NzUxMQ=="));

    //  Encryption must be enabled or disabled on both the server and client, otherwise
    //  communication will fail
    const ENCRYPTION_ENABLED = false;



    //  Never need to change this (Base64-encoded to hide the use as a bit-mask, which are common
    //  in encryption algorithms)
    const BIT_MASK_32 = parseInt(atob("NDI5NDk2NzI5NQ=="));

    function getCurrentSeed(utcTime) {
        //  Push back UTC time a bit in case the client somehow is a bit further ahead than
        //  server time
        utcTime -= 1000;
        let currentInterval = Math.floor(utcTime / SEED_SWAP_INTERVAL) % BIT_MASK_32;

        //  Logic mirrors EncryptionSeedProvider.cs and should be kept in sync
        //  with any changes 
        let result = currentInterval;
        //  Keep masking into 32 bits (uint)
        result = ((result << 13) & BIT_MASK_32) >>> 0;
        result = ((result * SEED_RANDOM_PRIME) & BIT_MASK_32) >>> 0;
        result = (result % SEED_RANDOM_PRIME) >>> 0;
        result = ((result ^ currentInterval) & BIT_MASK_32) >>> 0;
        result = ((result ^ SEED_RANDOM_PRIME) & BIT_MASK_32) >>> 0;
        result = ((result ^ SEED_SALT) & BIT_MASK_32) >>> 0;
        return result >>> 0; // >>> 0 converts this into a 32-bit unsigned integer
    }

    function makeSwizzleSizesFromSeed(seed) {
        let result = [];
        for (var i = 0; i < 8; i++) {
            let nibble = (seed >>> (i * 4)) & 0xF;
            nibble >>>= 0;
            result.push(Math.floor(nibble / 3) + 2);
        }
        return result;
    }

    return {
        encryptString: function (data, currentTime) {
            if (!ENCRYPTION_ENABLED) {
                return data;
            }

            let seed = getCurrentSeed(currentTime);

            //  Prefix all encrypted data with "vault:" as a validation measure during
            //  decryption
            let dataString = lib.jsonStringify(`vault:${data}`);
            let lzString = lib.lzstr.compressToEncodedURIComponent(dataString);
            let swizzleSizes = makeSwizzleSizesFromSeed(seed);
            let swizzleParts = [];

            let i = 0;
            for (var si = 0; i < lzString.length; si = (si + 1) % swizzleSizes.length) {
                let swizzle = swizzleSizes[si];
                let part = lzString.substr(i, swizzle);
                let partChars = part.split('');
                partChars.reverse();
                swizzleParts.push(partChars.join(''));

                i += part.length;
            }
            return 've_' + swizzleParts.join('');
        }
    };
}