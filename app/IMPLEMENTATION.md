
This document contains notes regarding implementation of several features.

## Authentication and Permissions

Authentication is done via the `X-V-TOKEN` header, which contains a string formatted `PlayerId:TribeId:VaultKey:IsSitter` (`TW.Vault.Lib.Security.AuthenticationUtil`). This can easily be spoofed by manually constructing a request; obfuscation of this data can prevent this sort of attack.

Authentication info is extracted early in the request pipeline (`TW.Vault.Lib.Security.RequireAuthAttribute`) and stored for later use by ASP.NET Controllers (`TW.Vault.App.Controllers.BaseController`).

Each key is considered a new Vault "user". A Vault "user" is tied to a world ID and player ID.

Data permissions are determined by the user's permissions, and by their "access group". Users can only access data within their own access group, which are created when a key is generated using the "Register" page. New keys made by it are automatically placed in the same access group.

The `TW.Vault.App.Controllers.BaseController` class has a `CurrentSets` utility for reading data based on the Access Group and World for the current authenticated user.

## Map Overlay

Data for the overlay is fetched from `TW.Vault.App.Controllers.VillageController.GetMapTags`. A specific region can be queried, but the script currently fetches data for the whole map when ran.

When hovering over a village on the map, extra data is fetched from `TW.Vault.App.Controllers.VillageController.GetVillageArmy`.

Script logic for the map is at `TW.Vault.App/wwwroot/ui/map.js`. It handles the map overlay and the hover-details logic.

## "Encryption" / Obfuscation

Obfuscation is performed on the `vault.js` script and on data sent by the script. The `vault.js` script is (optionally) obfuscated on startup using `javascript-obfuscator` from npm.

Data obfuscation is handled in the script at `TW.Vault.App/wwwroot/lib/encryption.js`. It is handled in the server at `TW.Vault.Lib.Security.DecryptionMiddleware` and `EncryptionSeedProvider`.

## Fakes Script

Logic for generating the fakes script is at `TW.Vault.App.Controllers.ScriptController.GetFakeScript`. It modifies the `TW.Vault.App/wwwroot/fakes.js` file with the list of village coordinates to target. The result is cached for later requests, and is updated when the requested script is 5 minutes old.

The UI for generating a fake script is at `TW.Vault.App/wwwroot/ui/vault/tools.js`.

## Tracking Troop Counts, etc.

All records of current data estimates (troop counts, building counts) are handled via Postgres Triggers. The C# code simply uploads report data, and the database uses the trigger to update the appropriate records. The PSQL for this is at `TW.Vault.Migration/res/Report_Update_Troops.sql`.

Any code using the `CurrentX` DB entities is using the auto-updated data from that trigger. Any other transmitted data is typically calculated on the fly for each request.

## Tagging Incomings

Server code for generating tag data is at `TW.Vault.App.Controllers.CommandController.GetIncomingTags`. The JS script at `TW.Vault.App/wwwroot/ui/tag-incomings.js` mostly uses this data as-is without further calculations. Most of that JS logic is about managing settings, formatting text, and tracking previous labels in case we want to revert them.

## Backtime Planner

This feature can be ran on a specific village (for a specific command) or can be used to fetch all potential backtimes for all known commands.

Server logic for per-command planning is at `TW.Vault.App.Controllers.CommandController.MakeBacktimePlan`. JS logic is at `TW.Vault.App/wwwroot/ui/village-info.js`.

Server logic for all backtimes is at `TW.Vault.App.Controllers.PlanController.GetBacktimePlan`. JS logic is at `TW.Vault.App/wwwroot/ui/vault/tools.js`.

## Script "Compiler"

The server code for stitching multiple JS files into one is at `TW.Vault.Lib.Features.ScriptCompiler`. It includes a CompileTimeVars ("CVar") option for injecting text into the script. These include obfuscation config, host config (for making Vault scripts), and a toggle for hiding the Fakes script feature.

The syntax for importing a JS file is `//# REQUIRE {file-path}`. Scripts are only imported once; further imports for that file are ignored. Most imports are done in `TW.Vault.App/wwwroot/vault.js`.

The syntax for injecting a compile-time var is `?%V<VAR-NAME>`. New vars should be specified in the compiler before using them in a script. They can be added in `TW.Vault.Lib.Features.ScriptCompiler.InitCommonVars`.

## Translations

Translation data is stored as a `TW.Vault.Lib.Scaffold.TranslationEntry`, which are packaged together via their `TranslationRegistry`. The available text to translate are specified via `TranslationKey`, and any parameters for those keys are specified by `TranslationParameter`. Keys and parameters are automatically imported to the DB from files in `TW.Vault.Lib.Scaffold.Seed`. For new text/translations, changes can be made directly to the database during development. Changes to the DB should be propagated to the appropriate Seed files.

The translation for the current user is specified by the `X-V-TRANSLATION` header, which is a Translation Registry ID. Text generated by the server references this value. This is handled in `TW.Vault.Controllers.BaseController.Translation` and `CurrentTranslation`. Use of translations is done through the class `TW.Vault.Features.TranslationContext` in `TW.Vault.Lib`. It loads all data from the selected translation, fills missing data from the default translation for the language, and fills any other missing data from the default English translation.

Translation within the JS script is mostly handled via `lib.translate` at `TW.Vault.App/wwwroot/lib/lib.js`.

For translations which require parameters (eg datetime formats), the second parameter to `lib.translate` accepts an object whose fields should match the names of the available parameters. For example, the `TIME_TODAY_AT` key has parameter `time`, which should be specified as `lib.translate(lib.itlcodes.TIME_TODAY_AT, { time: myTimeText })`.

Since translations are provided by users and are unvetted, this is a potential vector for a code injection attack. For any text inserted to the page as HTML, include `_escaped: true` for the second parameter to prevent this security issue. For example, `lib.translate(lib.itlcodes.TIME_TODAY_AT, { time: myTimeText, _escaped: true })`.

The list of available translation keys are maintained at `TW.Vault.App/wwwroot/lib/translationCodes.js`. This is mostly used to provide autocomplete to the code editor, making it easier to code without constantly referencing the list of keys.

## Script UI

Logic for the tabbed UI layout is mostly handled in `TW.Vault.App/wwwroot/ui/ui-lib.js`. All files in `TW.Vault.App/wwwroot/ui/vault` make use of it. The `ui/vault.js` file initializes the UI.

The `ui-lib.js` file also has utilities for syncing UI changes to object values, which is used wherever there are text inputs ie Map overlay and Tagging.

## Script Utilities

The file `TW.Vault.App/wwwroot/lib/lib.js` has utilities for:

- Parsing datetime strings with a variety of formats (`parseTimeString`)
- Translating text (`translate` / `hasTranslation`)
- Making authenticated requests to the Vault server (`getApi`, `postApi`, `putApi`, `deleteApi`)
- Checking for captcha (`checkContainsCaptcha`)
- Formatting text (`formatDateTime`, `formatDuration`, `namedReplace`)
- Parsing text (`namedMatch`)
- Accessing local storage and cookies (`setLocalStorage`, `getLocalStorage`) with default values for missing entries
- JSON generation/parsing (`jsonParse`, `jsonStringify`) with extra handling for Date objects
- ... and many others