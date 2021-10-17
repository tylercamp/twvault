# twvault

This is the original, full source code for my Tribal Wars Vault: https://forum.tribalwars.net/index.php?threads/vault.282252/

If you're seeing this README, the public Vault has been taken down and this code is no longer maintained by myself. I encourage others to fork this repository. I may accept pull requests, but it's preferred for others to make a copy and manage PRs independent of this one.

This README covers build instructions but not deployment.

# Repository Overview

- `/.vscode` - config files for VS code, not extensively used or tested
- `/app` - contains all code for the Vault
- `/docker` - an old set of files for generating various Docker images, not used or maintained; these were at least partially functional at some point
- `/k8s/twvault` - an old Helm 2 chart for deploying Vault on k8s; some work was done but this remains largely unfinished

Practically all folders except `/app` can be ignored.

# Getting Started

Requirements:

- Visual Studio Community
- dotnet core 3.1 SDK

Open the `/app/TW.Vault.sln` file to open the project in Visual Studio. All projects should be buildable as-is.

When debugging TW.Vault.App, change the debug target from "IIS Express" to the standalone server option.

When debugging TW.Vault.App, TW.ConfigurationFetcher, TW.Vault.Manage, or TW.Vault.MapDataFetcher, make sure to modify the environment variables for the application before attempting to run it. Specify the `ConnectionStrings__Vault` environment variable and assign it to a valid EntityFramework connection string, eg `Server=localhost; Port=5432; Database=vault; User Id=twu_vault; Password=password`.

# Configuration

These projects use the standard `appsettings.json` file to manage configuration properties. These can be modified directly, or can be overridden via environment variables. See the released sample configuration files under `/etc/systemd` for example usage.

The following options are available but no longer necessary, and can be ignored:

- `Initialization.EnableRequiredFiles`
- `Initialization.RequiredFiles`
- `Security.PublicScripts`

# Managing Security

Since all communications between user and server are managed by the Vault JS file, all attempts at protections can be inspected by any user and reverse-engineered. To mitigate this, the vault.js file is obfuscated automatically upon startup, and all data transmitted from the script to the server (including headers) is heavily obfuscated.

The data obfuscation method is simple. In order, the steps are: LZ-string compress the payload, base64 encode it, and "swizzle" various sets of bits from the data. The "swizzling" operation is based on a seed, salt, and the UTC timestamp (snapped to 15-second intervals). By using timestamp as an obfuscation parameter, the data can have different obfuscated versions depending on when it was generated. The seed and salt are configurable for the Vault server and are injected into the `vault.js` script on startup. When receiving data, the Vault server uses these same parameters to recover the original data.

This "security" is adequate at best. It's only reliable when details of the process are kept hidden. Once the obfuscation method is known, you only need to find the seed and salt from the `vault.js` script in order to start crafting your own custom requests to the Vault to bypass authorization checks.

This approach was chosen after accepting that a "perfect" approach to security was impossible in a TW script environment, where I have no access to session validation and self-XSS is the norm. The goal was to deter anyone from putting in time to reverse-engineer it. Even if the seed and salt were found, it would take significant effort to figure out how to use them.

Now that the repo is fully open-source, this implementation is significantly less "secure". It can still be used, and the seed+salt can be changed, but it's strongly recommended to replace this with a different implementation. Optimally, a new obfuscation algorithm would be randomly generated on startup.

These comments only relate to securing communications with the Vault webapp. Measures should also be taken to secure the host machine itself. The public Vault used psad, [NGINX autoban](https://github.com/tylercamp/nginxautoban), non-standard port numbers, and NGINX config restrictions, to quickly block potential attackers or severely limit their potential effect on operation of the server. Nearly all ports were exposed and forwarded to the server to allow psad to detect and block port scanning attacks.

# Project Overview

## TW.Vault.Migration
Used to initialize a Postgres database with all necessary schemas, tables, and stored procedures.

## TW.ConfigurationFetcher
A tool for automatically fetching config info for Tribal Wars game servers and storing in Vault DB. 

## TW.Vault.Manage
Public web interface for generating a new script from the vault, previously exposed at https://v.tylercamp.me/register.

## TW.Testing
A tool for miscellaneous testing of different features. Not intended to actually be used. Can be checked as a reference for how to use some different utilities in the project.

## TW.Vault.App
The web server application for the vault. Provides .NET Web API controllers for all endpoints handled by the vault, as well as JavaScript files for the main `vault.js` file that is served.

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

## TW.Vault.Lib
Contains most of the TW-specific calculations and model types, as well as misc. utilities.

### TW.Vault.Lib.Features
Logic forming the base of many vault features, eg command planning, searching villages, and battle simulations.

### TW.Vault.Lib.Model
TW-specific model types and related calculations. (Does not contain DB model types.) Includes utilities for converting JSON <-> DB types, TW-specific entity data (eg troop attack/defense power, building construction times.)

### TW.Vault.Lib.Scaffold
Database model types and `DatabaseContext` which enable communication with a database.

### TW.Vault.Lib.Security
Utilities for security features in the vault.

## TW.Vault.MapDataFetcher
Utility for retrieving the latest village, player, tribe, and conquer data from all worlds in the provided database. Automatically refreshes data hourly and provides an endpoint for forcing an update.
