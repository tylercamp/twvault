### Controllers
Each class under `TW.Vault.App.Controllers` serves a different base endpoint, eg `AdminController` serves content for `/api/admin`, etc. Endpoints generally take the form `/api/{worldName}/...`. All controllers that provide app features will inherit from `BaseController`, which has utilities for retrieving info on:
- The current authenticated user (from headers)
- The Tribal Wars world that was requested and its config (from URL parameters)
- Translation utilities
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