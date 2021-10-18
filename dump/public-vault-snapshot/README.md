# Overview

This directory contains the files used in the Public TW Vault previously hosted at https://v.tylercamp.me.

It contains the following:

- systemd init files
- full nginx configuration files with ip blacklists
- general directory structure of relevant apps (see `/var/aspnetcore` and `/etc/nginx/autoban`)
- all files used to serve web content
- the original configuration files for nginx (.conf files) and all Vault-related services (appsettings.json and hosting.json)

It does NOT contain:

- postgres configuration (defaults are fine; my config was changed to make use of the full 32GB of RAM)
- pgbackrest configuration (Vahtos set this up; I'm not familiar with pgbackrest and would rather not try to give incomplete/misguided info on configuring that)
- psad configuration (configuration walkthroughs can be found online, mine isn't necessarily the most secure)
- nginx amplify configuration (this is optional for server monitoring)
- full binaries/logs of applications on the server (all vault-related apps were compiled with the "self-contained" option, which leads to many duplicate files. these binaries are unnecessary, as are the logs, and would unnecessarily bloat this snapshot package)
- letsencrypt configuration (relevant sections from nginx conf are kept, but there are no useful letsencrypt files to share)

Included folders are:

- `/www/html` - default page when accessing v.tylercamp.me
- `/var/aspnetcore` - contains runtime binaries, app configuration files
- `/etc/nginx` - NGINX config files
- `/etc/systemd` - service config files for vault apps, makes Vault run on boot

# Other Comments

The Vault used basic A/B staging to enable zero-downtime updates. I'd have Vault running from `/var/aspnetcore/twvault-s1` (port 5000), and when I want to update I'd swap out `/var/aspnetcore/twvault-s2` (port 5001) with the new version. Start the service, change NGINX config so traffic is routed to `s2` (port 5001) instead of `s1` (port 5000), then shut down the `s1` service. The NGINX config at `/etc/nginx/sites-available/default` contains a line with `proxy_pass http://localhost:5000`, which is where I made the change. (Changes should be saved and NGINX manually updated via `nginx -s reload`.)


