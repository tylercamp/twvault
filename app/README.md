# Getting Started

Requirements:

- Visual Studio Community
- dotnet core 3.1 SDK
- A Postgres 10 DB

Open the `TW.Vault.sln` file to open the project in Visual Studio. All projects should be buildable as-is.

When debugging TW.Vault.App, Manage, or AppDataFetcher, change the debug target from "IIS Express" to the standalone server option.

When debugging TW.Vault.App, TW.ConfigurationFetcher, TW.Vault.Manage, or TW.Vault.MapDataFetcher, make sure to modify the environment variables / startup parameters for the application before attempting to run it. Specify the `ConnectionStrings__Vault` environment variable and assign it to a valid EntityFramework connection string, eg `Server=localhost; Port=5432; Database=vault; User Id=twu_vault; Password=password`.

The Postgres DB should be initialized using `TW.Vault.Migration` before attempting to run any of the other services locally.

The `launchSettings.json` files have been included, which have values used for debugging as a point of reference. If maintaining your own Vault, make sure not to commit local `launchSettings.json` changes to your repository, since these contain usernames and passwords.

Each project has a `publish-release.bat` file for generating standalone Linux executables in the folder `bin/Release/netcoreapp3.1/linux-x64/publish`, which can be copy/pasted to your server for deployment.

# Configuration

These projects use the standard `appsettings.json` file to manage configuration properties. These can be modified directly, or can be overridden via environment variables. See the released sample configuration files under `/etc/systemd` for example usage.

The following options are available but no longer necessary, and can be ignored:

- `Initialization.EnableRequiredFiles`
- `Initialization.RequiredFiles`
- `Security.PublicScripts`

The [TW.Vault.App README](TW.Vault.App) has more details on configuration. Database connection config options are reused by all Vault-related services (Manage, MapDataFetcher).

# Managing Security

Since all communications between user and server are managed by the Vault JS file, all attempts at protections can be inspected by any user and reverse-engineered. To mitigate this, the vault.js file is obfuscated automatically upon startup, and all data transmitted from the script to the server (including headers) is heavily obfuscated.

The data obfuscation method is simple. In order, the steps are:

1. LZ-string compress the payload
2. Base64 encode it
3. "Swizzle" various sets of bits from the data (take random-length chunks of payload bytes, reverse the order of each chunk, then place the reversed chunk into the output)

The "swizzling" operation is based on a seed, salt, and the UTC timestamp (snapped to 15-second intervals). By using timestamp as an obfuscation parameter, the data can have different obfuscated versions depending on when it was generated, preventing replay attacks. The seed and salt are configurable for the Vault server and are injected into the `vault.js` script on startup. When receiving data from the script, the Vault server uses these same parameters to recover the original data.

This "security" is adequate at best. It's only reliable when details of the process are kept hidden. Once the obfuscation method is known, you only need to find the seed and salt from the `vault.js` script in order to start crafting your own custom requests to the Vault to bypass authorization checks.

This approach was chosen after accepting that a "perfect" approach to security was impossible in a TW script environment, where I have no access to session validation and self-XSS is the norm. The goal was to deter anyone from putting in time to reverse-engineer it. Even if the seed and salt were found, it would take significant effort to figure out how to use them.

Now that the repo is fully open-source, this implementation is significantly less "secure". It can still be used, and the seed+salt can be changed, but it's strongly recommended to replace this with a different implementation. Optimally, a new obfuscation algorithm would be randomly generated on startup.

These comments only relate to securing communications with the Vault webapp. Measures should also be taken to secure the host machine itself. The public Vault used psad, [NGINX autoban](https://github.com/tylercamp/nginxautoban), non-standard port numbers, and NGINX config restrictions, to quickly block potential attackers or severely limit their potential effect on operation of the server. Nearly all ports were exposed and forwarded to the server to allow psad to detect and block port scanning attacks.

# Project Overview

## TW.Vault.Migration
Used to initialize a Postgres database with all necessary schemas, tables, and stored procedures. This was generated using the `dotnet ef migrate` tool, but some manual tweaks to the codebase were required to get it working, which have since been reverted since they were only necessary for generating migrations. I don't recall the exact details, but when updating the Migration tool, tinkering and troubleshooting will be necessary. Make sure not to change existing migration steps - new changes should go into a new migration step instead. (You can ignore this warning and modify the existing step anyway; the danger is that the new Migration tool would be incompatible with Vault DBs generated with a previous version, making it only useful for creating new DBs and useless for updating existing DBs.)

## TW.ConfigurationFetcher
A tool for automatically fetching config info for Tribal Wars game servers and storing in Vault DB. 

## TW.Vault.Manage
Public web interface for generating a new script from the vault, previously exposed at https://v.tylercamp.me/register.

## TW.Vault.App
The web server application for the vault. Provides .NET Web API controllers for all endpoints handled by the vault, as well as JavaScript files for the main `vault.js` file that is served.

## TW.Vault.Lib
Contains most of the TW-specific calculations and model types, as well as misc. utilities.

- `TW.Vault.Lib.Features`: Logic forming the base of many vault features, eg command planning, searching villages, and battle simulations.
- `TW.Vault.Lib.Model`: TW-specific model types and related calculations. (Does not contain DB model types.) Includes utilities for converting JSON <-> DB types, TW-specific entity data (eg troop attack/defense power, building construction times.)
- `TW.Vault.Lib.Scaffold`: Database model types and `DatabaseContext` which enable communication with a database.
- `TW.Vault.Lib.Security`: Utilities for security features in the vault.

## TW.Vault.MapDataFetcher
Utility for retrieving the latest village, player, tribe, and conquer data from all worlds in the provided database. Automatically refreshes data hourly and provides an endpoint for forcing an update.

## TW.Testing
A tool for miscellaneous testing of different features. Not intended to actually be used. Can be checked as a reference for how to use some different utilities in the project.

# Known Outstanding Issues

- Building/recruitment calculators use values hard-coded for w100
- Map overlay script always requests the full set of tags for the entire world, rather than just requesting for what's currently visible
- High scores service runs updates for all groups at the same time, causing regular spikes in CPU usage (these updates should be spaced evenly rather than running all at once)
- (... plus various others I've forgotten or am unaware of)