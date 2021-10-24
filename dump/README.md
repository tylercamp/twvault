# Overview

This folder contains various configuration and sample files extracted from servers I've configured to run twvault.

The following "snapshots" are provided:

- [`cicada-vault-snapshot`](cicada-vault-snapshot) - A handful of config files and a dump of command history from when it was first configured
- [`public-vault-snapshot`](public-vault-snapshot) - Full configuration files from the public Vault at v.tylercamp.me

_Each of these folders contains another `README.md` with more information._

Neither of these are meant to be plug-and-play for deploying the Vault. These are reference files which may be copied and modified for use in other deployments.

The Cicada information may be used for an overview of the process, and the Public information used as an "official" reference of what files/configs may be expected.

The Public copy has all original usernames/passwords in plaintext (as the server has already been shut down and wiped) while the Cicada copy has credentials redacted.

## Public Database Contents

World config and translation data has been exported from the public Vault to CSV form and are available in `db.zip`. Importing this to your own DB must be done [manually](https://dataschool.com/learn-sql/importing-data-from-csv-in-postgresql/).

## Building the Programs

Clone the original Vault repo from https://github.com/tylercamp/twvault and install the ASP.NET Core 3.1 SDK on your machine. If you're planning on modifying the Vault, use Microsoft Visual Studio Community for convenience.

Use the `build-release.bat` scripts to generate standalone linux binaries for each app.

## Minimum Deployment Requirements

A fully-functioning Vault deployment should have:

- Postgres 10 DB initialized with TW.Vault.Migration
- TW.Vault.App, TW.Vault.MapDataFetcher, TW.Vault.Manage
- Reverse-proxy configured in NGINX for each Vault web service
- HTTPS managed by NGINX

(NGINX is not required, but all sample configs use NGINX.)

## System Requirements

For small deployments, a 2-core server with 4GB of RAM should suffice. For larger deployments (ie public Vaults), a 4-core server with at least 8GB of RAM is recommended.

For servers with lots of RAM, use a [configuration generator](https://pgtune.leopard.in.ua/#/) to customize Postgres and make use of extra memory. Performance issues in new servers can typically be resolved by tweaking Postgres params.

## Maintenance Notes

When running on SSD, make sure to run `fstrim` regularly (at least weekly) to avoid disk perf. drops. A cron job is preferred.

The Vault is otherwise maintenance-free. For large deployments (public Vaults), the ConfigurationFetcher utility is used to download and configure new TW servers. If you have performance issues even after customizing Postgres config and running `fstrim`, the database might need to be optimized. Stop the webapps temporarily and run a `FULL VACUUM ANALYZE` in Postgres to restore performance. The server will be inoperable while this step is running. This can take anywhere from minutes to hours depending on the size of the DB and performance of the server. (The original Vault had an ~80GB DB and took 2-3 hours to complete.)

pgAdmin can be used to connect to your database and inspect statistics like active connections, deadlocks, and disk usage.

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

# NGINX Comments

Cicada Vault snapshot should be used for main settings, and the public snapshot can be considered for further tweaking.

All commented code can be ignored.

The `/dump/cicada-vault-snapshot/tpl/nginx` files have minimal configs:

- `default` has reverse-proxy configs for Vault webapp, Registration webapp, and a debug endpoint forcing refresh of map data for new worlds
- `default` has a stanza at the end of the file auto-managed by certbot which handles HTTPS config
- `tw_general.cfg` is imported for each reverse-proxy config, which forward headers for Vault + general HTTP
- `tw_general.cfg` also has minor optimization configs 
- `nginx.conf` has a minor change to proxied headers

The `/dump/public-vault-snapshot/etc/nginx` files have extra, old configs:

- `autobanips.conf` is auto-generated by [NAB](https://github.com/tylercamp/nginxautoban) (`blockips.conf` was an older list)
- `nginx-badbot-blocker` was a premade IP blacklist which I stopped using once I added NAB (it included some large IP bans which affected legitimate players)
- `sites-available/default` has many reverse-proxy configs which were used for QA and zero-downtime updates. "Stage 1" (local port 5000) was the primary service when the Vault was taken down. "Stage 2" (local port 5001) was the previous version which was deactivated after applying the update to Stage 1, setting NGINX to use Stage 1 as the primary, and reloading NGINX config