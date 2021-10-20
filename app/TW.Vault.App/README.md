# Overview

This project contains logic for the Vault webapp. Note that a large chunk of the logic is in the `TW.Vault.Lib` project instead.

### Configuration

Main configuration files are `appsettings.json` and `hosting.json`. All Vault config is done in `appsettings.json`. When running locally via Visual Studio, `appsettings.Development.json` is used and overrides any configs from the standard `appsettings.json` file.

`ConnectionStrings.Vault` is the only required configuration property.

`Initialization.AutoObfuscatePrimaryScript` requires that [`javascript-obfuscator`](https://github.com/javascript-obfuscator/javascript-obfuscator) be locally installed via npm.

`Initialization.ServerHostname` and `ServerBasePath` affect URL generation by the Vault.

`Security.UseEncryption` determines whether the Vault script will obfuscate data before sending to the server.

`Security.ForcedKey`, `ForcedPlayerId`, and `ForcedTribeId` are used to force the Vault to use a specific user when receiving requests. This allows us to spoof our identity and access data from other players, without needing to sign into their account in TW. This was used when troubleshooting bug discovered by players.

Fields in `Behavior` are the requirements for players to use the Map and Tagging features in Vault. (This prevents players from using main features without uploading frequently.)

### Controllers
Each class under `TW.Vault.App.Controllers` serves a different base endpoint, eg `AdminController` serves content for `/api/admin`, etc. Endpoints generally take the form `/api/{worldName}/...`. All controllers that provide app features will inherit from `BaseController`, which has utilities for retrieving info on:
- The current authenticated user (from headers)
- The Tribal Wars world that was requested and its config (from URL parameters)
- Translations
- Database collection classes scoped to the current user and world (class `CurrentContextDbSets` and property `CurrentSets`)

Utility controllers such as `Script` and `Performance` don't provide TW-specific features and don't use this base class.

### Script files
#### Script Generation/Merging
A custom script "compiler" is used in `ScriptController` to merge files under `TW.Vault.App/wwwroot` into a single file. The root scripts are `wwwroot/main.js` and `wwwroot/vault.js`. `vault.js` is the primary entry-point. Scripts are merged by using the syntax in a script file:

```js
//# REQUIRE file.js
```

Contents of the required script are inserted using a basic text-replace, and each required file will only be included once in the final script.

#### Script Files Organization
```
- wwwroot/

- - lib/   - Misc. utility files

- - - lib.js   - Contains most utility functions, eg API querying, time parsing, translations

- - pages/ - Page-specific scripts for parsing data from different Tribal Wars pages

- - ui/    - Scripts for presenting UI. Each script in this folder provides UI for
             different pages, eg map and incomings. Any page that doesn't have its
             own specific UI will use the default Vault UI containing different
             tabs.
             
- - - vault/   - Scripts for UI in the main vault interface; each file (generally)
                 represents a different tab in the UI
```