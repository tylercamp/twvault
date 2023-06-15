#!/bin/bash
set -e
exec >/root/stackscript.log 2>&1

source <ssinclude StackScriptID="1">

#######################################################
# (References)
###
#
# https://www.linode.com/community/questions/479/stackscript-guide#answer-70895
# https://www.linode.com/docs/products/compute/compute-instances/guides/set-up-and-secure/
# https://cloud.linode.com/stackscripts/1 (StackScript Bash Library)

# https://stackoverflow.com/questions/369758/how-to-trim-whitespace-from-a-bash-variable
trim() {
    local var="$*"
    # remove leading whitespace characters
    var="${var#"${var%%[![:space:]]*}"}"
    # remove trailing whitespace characters
    var="${var%"${var##*[![:space:]]}"}"
    printf '%s' "$var"
}

# <UDF name="SSP_SERVER_HOSTNAME" label="Domain Name" example="myvault.com">
# <UDF name="SSP_DB_USER" label="Database User" example="twu_vault" default="twu_vault">
# <UDF name="SSP_DB_PASSWORD" label="Database Password" example="password123" default="password123">
# <UDF name="SSP_VAULT_REPO" label="Vault Git repository" default="https://github.com/tylercamp/twvault">
# <UDF name="SSP_EMAIL" label="Email for HTTPS notifications">

# trim surrounding whitespace from all params to minimize user error

SERVER_HOSTNAME=$(trim $SSP_SERVER_HOSTNAME)
DB_USER=$(trim $SSP_DB_USER)
DB_PASSWORD=$(trim $SSP_DB_PASSWORD)
VAULT_REPO=$(trim $SSP_VAULT_REPO)
EMAIL=$(trim $SSP_EMAIL)

apt update

# ensure NTP is configured to prevent time drift; vault encryption relies on accurate time tracking
system_configure_ntp

# (install `micro` editor in case user wants to edit files, less annoying than nano or vi)
curl https://getmic.ro | bash
mv micro /usr/bin

#######################################################
# Install PostgreSQL
###

sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'
wget -qO- https://www.postgresql.org/media/keys/ACCC4CF8.asc | sudo tee /etc/apt/trusted.gpg.d/pgdg.asc &>/dev/null
apt update
apt install postgresql-15 -y

sudo -u postgres psql -c "CREATE USER $DB_USER SUPERUSER PASSWORD '$DB_PASSWORD'"

DB_CONNECTION_STRING="Server=localhost; Database=vault; User Id=$DB_USER; Password=$DB_PASSWORD; Port=5432"

#######################################################
# Install obfuscation package
###

echo "!!! Installing Obfuscator"
apt install npm -y
npm install -g javascript-obfuscator

#######################################################
# Install + (mostly) configure NGINX
###

apt install nginx -y

# modify global HTTP config to remove some proxy warnings
# https://www.jamiebalfour.scot/devkit/sed/
sed -i 's/http {/http {\n\tproxy_headers_hash_max_size 1024;\n\tproxy_headers_hash_bucket_size 256;/g' /etc/nginx/nginx.conf

# template file handling reverse-proxy config in location blocks
cat <<EOT >> /etc/nginx/tw-proxy.cfg
client_body_buffer_size 25m;
proxy_buffers 128 15m;

proxy_http_version      1.1;
proxy_set_header        Upgrade \$http_keepalive;
proxy_set_header        Connection keep-alive;
proxy_set_header        Host \$host;
proxy_set_header        X-Forwarded-Host \$host;
proxy_cache_bypass      \$http_upgrade;
proxy_set_header        X-Forwarded-For \$remote_addr;
proxy_set_header        X-Forwarded-Proto \$scheme;
proxy_set_header        X-Real-IP \$remote_addr;
proxy_set_header        X-Forwarded-Host \$remote_addr;
EOT

# Actual server block providing reverse-proxy for Vault services, will be auto-updated by certbot for HTTPS

cat <<EOT >> /etc/nginx/sites-available/default

server {
	server_name $SERVER_HOSTNAME;
	proxy_pass_request_headers on;
	underscores_in_headers on;

	location ~ ^/api/forcerefresh {
		proxy_pass http://localhost:5020;
		proxy_set_header X-Original-Url \$request_uri;
		include tw-proxy.cfg;
	}

	location ~ ^/(api|script)/ {
		proxy_pass http://localhost:5000;
		proxy_set_header X-Original-Url \$request_uri;
		include tw-proxy.cfg;
	}

	location /register/ {
		rewrite ^/register/(.*)\$ /\$1 break;
		proxy_pass http://localhost:5030;
		proxy_set_header X-Original-Url \$request_uri;
		include tw-proxy.cfg;

		#deny all;
	}
}
EOT

#######################################################
# Install .net SDK
###

# fix for .NET 7 being dumb and not supporting libssl3 shipped with Ubuntu 22.04 LTS (even though it's supposed to)
echo "deb http://security.ubuntu.com/ubuntu focal-security main" | tee /etc/apt/sources.list.d/focal-security.list
apt update

apt install libssl1.1 -y
apt install dotnet-sdk-7.0 -y

#######################################################
# Fetch Vault code and build executables
###

mkdir /vault && cd /vault
git clone https://github.com/tylercamp/twvault src

mkdir /vault/bin
mkdir /vault/bin/init
mkdir /vault/bin/webapp
mkdir /vault/bin/manage
mkdir /vault/bin/update
mkdir /vault/bin/tools

cd /vault/src/app/TW.Vault.App
dotnet publish -c Release -r linux-x64 --no-self-contained -o /vault/bin/webapp

cd /vault/src/app/TW.ConfigurationFetcher
dotnet publish -c Release -r linux-x64 --no-self-contained -o /vault/bin/tools

cd /vault/src/app/TW.Vault.Manage
dotnet publish -c Release -r linux-x64 --no-self-contained -o /vault/bin/manage

cd /vault/src/app/TW.Vault.MapDataFetcher
dotnet publish -c Release -r linux-x64 --no-self-contained -o /vault/bin/update

cd /vault/src/app/TW.Vault.Migration
dotnet publish -c Release -r linux-x64 --no-self-contained -o /vault/bin/init

#######################################################
# Initialize DB
###

echo "Using connection string: '$DB_CONNECTION_STRING'"
cd /vault/bin/init
dotnet TW.Vault.Migration.dll "$DB_CONNECTION_STRING"

#######################################################
# Create a utility script for running the configuration tool
###

cat <<EOT >> /vault/configure.sh
dotnet /vault/bin/tools/TW.ConfigurationFetcher.dll "$DB_CONNECTION_STRING" \$@
EOT

cat <<EOT >> /vault/configure-help.sh
echo "Note: 'connection-string' is provided automatically by configure.sh"
echo "All displayed flags can be given directly to configure.sh"
dotnet /vault/bin/tools/TW.ConfigurationFetcher.dll
EOT

cat <<EOT >> /vault/fetch-latest-servers.sh
/vault/configure.sh -clean -fetch-all -reset-on-diff
EOT

chmod +x /vault/configure.sh /vault/configure-help.sh /vault/fetch-latest-servers.sh

#######################################################
# Register the .net TLD server
###

/vault/configure.sh -extraTLD tribalwars.net -fetch-all -accept

#######################################################
# Configure Vault programs as services which run on start
###

mkdir /vault/logs
mkdir /vault/logs/webapp
mkdir /vault/logs/manage
mkdir /vault/logs/update

mkdir /vault/map-cache

cat <<EOT >> /vault/bin/webapp/start.sh
#!/bin/bash
dotnet TW.Vault.App.dll
EOT

cat <<EOT >> /vault/bin/manage/start.sh
#!/bin/bash
dotnet TW.Vault.Manage.dll
EOT

cat <<EOT >> /vault/bin/update/start.sh
#!/bin/bash
dotnet TW.Vault.MapDataFetcher.dll
EOT

chmod +x /vault/bin/webapp/start.sh /vault/bin/manage/start.sh /vault/bin/update/start.sh

ENC_SEED_SALT=$[ $RANDOM * $RANDOM - $RANDOM ]
ENC_SEED_PRIME=$[ $RANDOM * $RANDOM - $RANDOM ]

cat <<EOT >> /etc/systemd/system/twvault-app.service
[Unit]
Description=Vault Main Webapp
[Service]
WorkingDirectory=/vault/bin/webapp/
ExecStart=/vault/bin/webapp/start.sh
Restart=always
RestartSec=10
SyslogIdentifier=aspnet-twvault-webapp
User=root

Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment="urls=http://*:5000"
Environment=ConnectionStrings__Vault="$DB_CONNECTION_STRING"

### Logging
Environment=Serilog__WriteTo__MainLogger__Args__configureLogger__WriteTo__AsyncSink__Args__configure__0__Args__path="/vault/logs/webapp/log-.log"
Environment=Serilog__WriteTo__IpLogger__Args__configureLogger__WriteTo__AsyncSink__Args__configure__0__Args__path="/vault/logs/webapp/log-ip-.log"

### Initialization
Environment=Initialization__ServerHostname="$SERVER_HOSTNAME"
Environment=Initialization__ServerBasePath="/"

### Other settings
Environment=Rankings__EnableRinkingsService=true
Environment=Rankings__RefreshCheckIntervalSeconds=300
Environment=Security__RestrictSitterAccess=true
Environment=Security__UseEncryption=true

Environment=Security__Encryption__UseEncryption=true
Environment=Security__Encryption__SeedSalt=$ENC_SEED_SALT
Environment=Security__Encryption__SeedPrime=$ENC_SEED_PRIME

# Disabled by default to be safe, may enable depending on server rules
Environment=Behavior__DisableFakeScript=true

# Vault will deny access to some features if the user hasn't uploaded in a while, limits are defined here
Environment=Behavior__Map__MaxDaysSinceReportUpload=1
Environment=Behavior__Map__MaxDaysSinceTroopUpload=3
Environment=Behavior__Map__MaxDaysSinceCommandUpload=3
Environment=Behavior__Map__MaxDaysSinceIncomingsUpload=3
Environment=Behavior__Tagging__MaxDaysSinceReportUpload=1
Environment=Behavior__Tagging__MaxDaysSinceTroopUpload=1
Environment=Behavior__Tagging__MaxDaysSinceCommandUpload=1
Environment=Behavior__Tagging__MaxDaysSinceIncomingsUpload=1

[Install]
WantedBy=multi-user.target
EOT

cat <<EOT >> /etc/systemd/system/twvault-manage.service
[Unit]
Description=Vault User Registration App
[Service]
WorkingDirectory=/vault/bin/manage/
ExecStart=/vault/bin/manage/start.sh
Restart=always
RestartSec=10
SyslogIdentifier=aspnet-twvault-manage
User=root

Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment="urls=http://*:5030"
Environment=ConnectionStrings__Vault="$DB_CONNECTION_STRING"

Environment=UseCaptcha=false
Environment=CaptchaSecretKey="your captcha secret key if captcha is enabled"
Environment=CaptchaSiteKey="your captcha site key if captcha is enabled"

Environment=Serilog__WriteTo__MainLogger__Args__configureLogger__WriteTo__AsyncSink__Args__configure__0__Args__path=/vault/logs/manage/log-.log

[Install]
WantedBy=multi-user.target
EOT

cat <<EOT >> /etc/systemd/system/twvault-map-fetcher.service
[Unit]
Description=Vault Map Data Updater
[Service]
WorkingDirectory=/vault/bin/update/
ExecStart=/vault/bin/update/start.sh
Restart=always
RestartSec=10
SyslogIdentifier=aspnet-twvault-map-fetch
User=root

Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment="urls=http://*:5020"
Environment=ConnectionStrings__Vault="$DB_CONNECTION_STRING"

Environment=Serilog__MinimumLevel__Override__Microsoft=Warning
Environment=Serilog__WriteTo__MainLogger__Args__configureLogger__WriteTo__AsyncSink__Args__configure__0__Args__path=/vault/logs/update/log-.log

Environment=CachingFilePath=/vault/map-cache
Environment=DataBatchSize=500
Environment=CheckDelaySeconds=300

[Install]
WantedBy=multi-user.target
EOT

#######################################################
# Register and start Vault services
###
systemctl daemon-reload
systemctl enable twvault-app.service
systemctl enable twvault-manage.service
systemctl enable twvault-map-fetcher.service

systemctl start twvault-map-fetcher
systemctl start twvault-manage
systemctl start twvault-app

#######################################################
# Configure HTTPS
###

# Wait for DNS name to match this VM
EXPECTED_IP=$(system_primary_ip)
EXPECTED_IP=$(trim $EXPECTED_IP)
CURRENT_IP=""

# Install and run certbot

apt install snapd -y
snap install core
snap refresh core
snap install --classic certbot

echo "Attempting to fetch HTTPS certificate"

# https://eff-certbot.readthedocs.io/en/stable/using.html#certbot-command-line-options
until certbot --nginx -n -d "$SERVER_HOSTNAME" --agree-tos -m "$EMAIL" --no-eff-email
do
  sleep 900 # rate limit 4x per hour
done

#######################################################
# Basic security config
###

ufw_install
configure_basic_firewall
add_ports 80 443
save_firewall
enable_fail2ban

cat << EOF

===


# Done!

Get a Vault script at:
https://$SERVER_HOSTNAME/register/

Then use the commands below to update NGINX to disable the Register page.
(Otherwise someone else can use your server for themselves.)

Run this to edit the file:
   micro /etc/nginx/sites-enabled/default

Look for the "#deny all;" text around line 116 and remove the "#" so the rule gets
applied and all requests to the Register page are blocked.
(Ctrl+S to save, Ctrl+Q to close.)

Then run this to save your changes:
   nginx -s reload

EOF
