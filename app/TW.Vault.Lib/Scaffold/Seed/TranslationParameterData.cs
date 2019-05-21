
using System.Collections.Generic;

namespace TW.Vault.Scaffold.Seed
{
    public static class TranslationParameterData
    {
        /*
         * Given exported CSV data:
         *
         * let csv = `...`;
         * let parsedEntries = csv.split('\n').splice(1).map(l => l.split(',')).map(e => ({ id: e[0], name: e[1], keyId: e[2] }))
         * parsedEntries.map(e => `new TranslationParameter { Id = ${e.id}, Name = ${e.name}, KeyId = ${e.keyId} }`).join(',\n')
         */
        
        public static List<TranslationParameter> Contents { get; } = new List<TranslationParameter>
        {
            new TranslationParameter { Id = 1, Name = "numIncs", KeyId = 29 },
            new TranslationParameter { Id = 2, Name = "numDone", KeyId = 67 },
            new TranslationParameter { Id = 3, Name = "numTotal", KeyId = 67 },
            new TranslationParameter { Id = 4, Name = "numFailed", KeyId = 67 },
            new TranslationParameter { Id = 5, Name = "duration", KeyId = 125 },
            new TranslationParameter { Id = 6, Name = "nukesRequired", KeyId = 89 },
            new TranslationParameter { Id = 7, Name = "morale", KeyId = 89 },
            new TranslationParameter { Id = 8, Name = "lossPercent", KeyId = 89 },
            new TranslationParameter { Id = 9, Name = "numNukes", KeyId = 141 },
            new TranslationParameter { Id = 10, Name = "duration", KeyId = 147 },
            new TranslationParameter { Id = 11, Name = "tribeName", KeyId = 172 },
            new TranslationParameter { Id = 12, Name = "playerName", KeyId = 202 },
            new TranslationParameter { Id = 13, Name = "playerName", KeyId = 203 },
            new TranslationParameter { Id = 14, Name = "playerName", KeyId = 204 },
            new TranslationParameter { Id = 15, Name = "playerName", KeyId = 205 },
            new TranslationParameter { Id = 16, Name = "tribeName", KeyId = 231 },
            new TranslationParameter { Id = 20, Name = "numTimings", KeyId = 314 },
            new TranslationParameter { Id = 21, Name = "numNukes", KeyId = 314 },
            new TranslationParameter { Id = 22, Name = "numShown", KeyId = 314 },
            new TranslationParameter { Id = 23, Name = "numDone", KeyId = 360 },
            new TranslationParameter { Id = 24, Name = "numTotal", KeyId = 360 },
            new TranslationParameter { Id = 25, Name = "numFailed", KeyId = 360 },
            new TranslationParameter { Id = 26, Name = "numCommands", KeyId = 362 },
            new TranslationParameter { Id = 27, Name = "numDone", KeyId = 363 },
            new TranslationParameter { Id = 28, Name = "numTotal", KeyId = 363 },
            new TranslationParameter { Id = 29, Name = "numFailed", KeyId = 363 },
            new TranslationParameter { Id = 30, Name = "numDone", KeyId = 365 },
            new TranslationParameter { Id = 31, Name = "numTotal", KeyId = 365 },
            new TranslationParameter { Id = 32, Name = "numFailed", KeyId = 365 },
            new TranslationParameter { Id = 33, Name = "numIncomings", KeyId = 367 },
            new TranslationParameter { Id = 34, Name = "numDone", KeyId = 370 },
            new TranslationParameter { Id = 35, Name = "numTotal", KeyId = 370 },
            new TranslationParameter { Id = 36, Name = "numOld", KeyId = 376 },
            new TranslationParameter { Id = 37, Name = "numDone", KeyId = 378 },
            new TranslationParameter { Id = 38, Name = "numTotal", KeyId = 378 },
            new TranslationParameter { Id = 39, Name = "numFailed", KeyId = 378 },
            new TranslationParameter { Id = 40, Name = "numTotal", KeyId = 380 },
            new TranslationParameter { Id = 41, Name = "numDone", KeyId = 380 },
            new TranslationParameter { Id = 42, Name = "numFailed", KeyId = 380 },
            new TranslationParameter { Id = 43, Name = "numNobles", KeyId = 388 },
            new TranslationParameter { Id = 44, Name = "numVillages", KeyId = 389 },
            new TranslationParameter { Id = 45, Name = "numVillages", KeyId = 393 },
            new TranslationParameter { Id = 46, Name = "oldLoyalty", KeyId = 435 },
            new TranslationParameter { Id = 47, Name = "newLoyalty", KeyId = 435 },
            new TranslationParameter { Id = 50, Name = "dataType", KeyId = 441 },
            new TranslationParameter { Id = 51, Name = "time", KeyId = 449 },
            new TranslationParameter { Id = 52, Name = "time", KeyId = 450 },
            new TranslationParameter { Id = 53, Name = "time", KeyId = 451 },
            new TranslationParameter { Id = 54, Name = "date", KeyId = 451 },
            new TranslationParameter { Id = 55, Name = "date", KeyId = 452 },
            new TranslationParameter { Id = 56, Name = "time", KeyId = 452 },
            new TranslationParameter { Id = 57, Name = "hour", KeyId = 453 },
            new TranslationParameter { Id = 58, Name = "minute", KeyId = 453 },
            new TranslationParameter { Id = 59, Name = "second", KeyId = 453 },
            new TranslationParameter { Id = 60, Name = "day", KeyId = 453 },
            new TranslationParameter { Id = 61, Name = "month", KeyId = 453 },
            new TranslationParameter { Id = 62, Name = "year", KeyId = 453 },
            new TranslationParameter { Id = 63, Name = "numDone", KeyId = 462 },
            new TranslationParameter { Id = 64, Name = "numTotal", KeyId = 462 },
            new TranslationParameter { Id = 65, Name = "numFailed", KeyId = 462 },
            new TranslationParameter { Id = 66, Name = "playerName", KeyId = 463 },
            new TranslationParameter { Id = 67, Name = "playerName", KeyId = 464 },
            new TranslationParameter { Id = 68, Name = "playerName", KeyId = 465 },
            new TranslationParameter { Id = 69, Name = "playerName", KeyId = 466 },
            new TranslationParameter { Id = 70, Name = "playerName", KeyId = 467 },
            new TranslationParameter { Id = 71, Name = "oldPlayer", KeyId = 468 },
            new TranslationParameter { Id = 72, Name = "newPlayer", KeyId = 468 },
            new TranslationParameter { Id = 73, Name = "playerName", KeyId = 469 },
            new TranslationParameter { Id = 74, Name = "playerName", KeyId = 470 },
            new TranslationParameter { Id = 75, Name = "playerName", KeyId = 471 },
            new TranslationParameter { Id = 76, Name = "playerName", KeyId = 472 },
            new TranslationParameter { Id = 77, Name = "playerName", KeyId = 473 },
            new TranslationParameter { Id = 78, Name = "playerName", KeyId = 474 },
            new TranslationParameter { Id = 79, Name = "oldAdmin", KeyId = 474 },
            new TranslationParameter { Id = 80, Name = "newAdmin", KeyId = 474 },
            new TranslationParameter { Id = 81, Name = "playerName", KeyId = 475 },
            new TranslationParameter { Id = 82, Name = "playerName", KeyId = 476 },
            new TranslationParameter { Id = 83, Name = "name", KeyId = 521 },
            new TranslationParameter { Id = 84, Name = "author", KeyId = 521 },
            new TranslationParameter { Id = 85, Name = "keyName", KeyId = 525 },
            new TranslationParameter { Id = 86, Name = "parameters", KeyId = 525 },
            new TranslationParameter { Id = 87, Name = "name", KeyId = 529 },
            new TranslationParameter { Id = 88, Name = "name", KeyId = 534 },
            new TranslationParameter { Id = 89, Name = "day", KeyId = 538 },
            new TranslationParameter { Id = 90, Name = "month", KeyId = 538 },
            new TranslationParameter { Id = 91, Name = "year", KeyId = 538 },
            new TranslationParameter { Id = 92, Name = "oldLevel", KeyId = 540 },
            new TranslationParameter { Id = 93, Name = "newLevel", KeyId = 540 },
            new TranslationParameter { Id = 94, Name = "buildingName", KeyId = 541 },
            new TranslationParameter { Id = 95, Name = "oldLevel", KeyId = 541 },
            new TranslationParameter { Id = 96, Name = "newLevel", KeyId = 541 },
            new TranslationParameter { Id = 97, Name = "adminName", KeyId = 473 },
            new TranslationParameter { Id = 98, Name = "millisecond", KeyId = 453 }
        };
    }
}