## Overview

The given `stackscript.sh` is available as a public StackScript on Linode for automatically deploying + configuring all needed Vault services, a database, and HTTPS, into a new VM. It can also be ran manually on Ubuntu 22.x. (It may work for older versions, but I haven't tested it.)

It will:

- Install PostgreSQL 15 database
- Install NGINX
- Fetch the latest code from this repo and build linux binaries
- Register the Vault apps as services which run on startup (main webapp, registration page, map data fetcher)
- Enable Encryption (with randomly-generated seed/salt) and Script Obfuscation by default
- Configure NGINX to provide access to Vault web services
- Use `certbot` to automatically fetch and configure certs for HTTPS with NGINX
- Add the tribalwars.net servers to the Vault
- Create some helper scripts for configuring the Vault
- Install the [`micro`](https://micro-editor.github.io/) text editor for convenience

It will NOT:

- **Install or configure any security-related services**
- **Configure firewalls or apply other best-practices for Linux server hardening (eg all services run as `root`)**
- Configure NGINX rate-limiting
- Include the Fake Scripts feature by default (just to be safe; you can [edit the service](#customizing-after-installation) to enable it if the server you're using allows the fake script)
- Include any translations for non-english servers
- Do any performance optimizations on the PostgreSQL database config

It takes about 5 minutes to deploy a fully functioning server. Progress can be viewed with `tail -f /root/stackscript.log`.

**You'll see a message at the end of the log once it's ready. It will include a link to the Vault registration page where you can make a script for yourself.**

### Running with Linode

The installer requires a domain name which points to your server, but you need to create the server before you can update your domain name.

Create a new VM using Ubuntu 22.x, but do NOT use the StackScript yet. Fetch the IP address of the server from Linode and update the DNS records for your domain name.

Once that's ready, use the VM's dropdown menu to Rebuild the VM and use the public StackScript named `twvault`. This will rebuild the server from scratch, using the StackScript, and it will retain the same IP that you copied earlier.

### Running Manually

Configure your domain name to point to your server's IP, then use the example below to run the script.

```
export DB_USER="user"
export DB_PASSWORD="password"
export SERVER_HOSTNAME="myhostname.com" # needed for HTTPS setup
export EMAIL="me@gmail.com" # needed for HTTPS setup; you'll be emailed if there's an issue auto-configuring HTTPS after the cert expires
sudo stackscript.sh
```

### Things to do after Installing

The Vault script registration page will be available to all, meaning others could get their own scripts and use your Vault for themselves if they happen to learn about it. Once you create a Vault script for yourself, use `micro /etc/nginx/sites-enabled/default` to edit the NGINX config and disable access to the registration page.

Look for the line `location /register/ {` and find the `#deny all` line below it. Remove the `#` from that line and use Ctrl+S to save, then Ctrl+Q to exit the editor. Finally, run `nginx -s reload` to apply the change. This will disable access to the registration page for _everyone_. If you need to make a new script for yourself, edit and add the `#` to that line again and run `nginx -s reload` to re-enable access.

### Customizing After Installation

**App Configuration**

The script registers the apps as services in `/etc/systemd/system`, the most important file is at `/etc/systemd/system/twvault-app.service`. You can edit this file to customize behavior of the Vault. The most notable line is `Environment=Behavior__DisableFakeScript=true`, you can set this to `false` instead to enable the fake scripts feature.

The Vault service providing the registration page is at `/etc/systemd/system/twvault-manage.service`. Captcha is disabled by default. You can edit this file to re-enable captcha and set the captcha secret-key and site-key.

After making changes to any files in the `/etc/systemd/system` folder, use `systemctl daemon-reload` followed by `systemctl restart <service-name>` (eg `systemctl restart twvault-app`) to apply the changes.

**Managing TW Servers**

The install script creates utility scripts in the `/vault` folder - `configure.sh`, `configure-help.sh`, `fetch-latest-servers.sh`.

`configure.sh` is used to add new worlds and remove old ones. Use `configure-help.sh` to see the available options. For example:

```bash
/vault/configure.sh -extraserver en100.tribalwars.net
/vault/configure.sh -clean
```

The script will let you review any pending changes before they are applied.

You can use the `fetch-latest-servers.sh` script to automatically fetch the latest servers and remove servers which have been closed. It will fetch all servers for the "TLDs" (eg tribalwars.net, tribalwars.co.uk) that have already been added to the Vault.

The Vault's map-update service will automatically fetch data for any worlds that were added. It may take ~10 minutes for the Vault to load data for new worlds.

### Known Issues

- Logging isn't configured properly for Vault services; they are set to log to `/vault/logs/` but no files are created. The current logging config is based on the Public/Cicada Vault dumps which use an older version of the Serilog logging library. This is likely caused by recent changes to the Vault which updated all dependencies to their latest versions
- The script isn't set up to handle non-english servers. You can still add them manually using the scripts in `/vault` created by the installer, but the only available language option for translations is "English". Fortunately the "language" option is only used for organization. You can create a new translation for your language, mark it as an "English" translation, and you can still use your translation on non-english servers. You'll need to manually edit the World Settings entries in the database (`vault.tw_provided.world.default_translation_id`) to set it as the default translation for the server(s) you're playing on.
- The script hasn't been tested extensively, there are probably issues not mentioned here
