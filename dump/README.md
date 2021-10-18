# Overview

This folder contains various configuration and sample files extracted from servers I've configured to run twvault.

The following "snapshots" are provided:

- [`cicada-vault-snapshot`](public-vault-snapshot) - A handful of config files and a dump of command history from when it was first configured
- `public-vault-snapshot` - Full configuration files from the public Vault at v.tylercamp.me

_Each of these folders contains another `README.md` with more information._

Neither of these are meant to be plug-and-play for deploying the Vault. These are reference files which may be copied and modified for use in other deployments.

The Cicada information may be used for an overview of the process, and the Public information used as an "official" reference of what files/configs may be expected.

The Public copy has all original usernames/passwords in plaintext (as the server has already been shut down and wiped) while the Cicada copy has credentials redacted.

## Building the Programs

Clone the original Vault repo from https://github.com/tylercamp/twvault and install the ASP.NET Core 3.1 SDK on your machine. If you're planning on modifying the Vault, use Microsoft Visual Studio Community for convenience.

!! TODO !!

Use the `build-release.bat` (windows) / `build-release.sh` (unix) scripts to generate standalone linux binaries.

## Rough Step-by-Step Setup

This list is based on the Cicada `command-history.txt` file and roughly from memory. Some steps may not be strictly required. 

1. Use Ubuntu 18.04 or later (can use Server or Desktop ISOs)
2. Install ASP.NET Core 3.1 SDK
3. Install Postgres 10 (Vault is only tested on pgsql 10) (save the username/password used during setup)
4. Install nodejs (any version)
5. Install [`javascript-obfuscator`](https://github.com/javascript-obfuscator/javascript-obfuscator) with `npm install -g javascript-obfuscator`
6. Copy the standalone linux binaries to the server, enable execution bit on application files (eg `chmod +x TW.Vault.App`)
7. Use the `TW.Vault.Migration` app to initialize your postgres database
8. Use the `TW.ConfigurationFetcher` app to fetch and store some config data for the server you play on
9. Create a `systemd` entry for the `TW.Vault.MapDataFetcher` application, configure as necessary, enable and start the service
10. Once the MapDataFetcher finishes pulling map data for the registered servers, create a `systemd` entry for `TW.Vault.App`, configure, enable, and start; monitor the application to ensure it starts successfully
11. Similarly configure and start the `TW.Vault.Manage` app. The Captcha options must be configured
12. Install NGINX, configure reverse-proxy from HTTP port to the configured port for TW.Vault.App; configure a separate dedicated path and reverse-proxy to TW.Vault.Manage
13. Navigate to the configured path for TW.Vault.Manage to see the registration page. Create a Vault key as usual
14. Reserve a domain name, point it at your Vault server, and configure SSL in NGINX (eg via certbot). SSL must be configured or your browser will block the script when you try to run it from TW
15. Put your script in your quickbar, cross your fingers, and run it

Notes:

- Vault isn't limited to Ubuntu, but my experience and these configs are all from Ubuntu
- Steps for `TW.Vault.Manage` can be skipped, but you'll need to manually create a new access group and user registration for yourself


## Expectations for New Deployments

The Vault accumulated various bugs over time due to TW updates, but there are other minor bugs that I hadn't pursued. Most notably are the occasional DB transaction errors due to FK constraint violations. This was seen many times in personal logs but I hadn't retrieved any associated bug reports, so I've generally ignored it.