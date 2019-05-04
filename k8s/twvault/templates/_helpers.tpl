{{- define "twvault.name" -}}
{{- .Chart.Name -}}
{{- end -}}

{{- define "twvault.fullname" -}}
{{- $name := .Chart.Name -}}
{{- if contains $name .Release.Name -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}

{{- define "twvault.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "twvault.connectionString" -}}
{{- $dbHostname := (include "twvault.dbHostname" .) -}}
{{- $dbDatabase := .Values.env.app.ConnectionStrings__Vault__Database -}}
{{- $dbUser := .Values.env.app.ConnectionStrings__Vault__User -}}
{{- $dbPassword := .Values.env.app.ConnectionStrings__Vault__Password -}}
{{- printf "Server=%s; Database=%s; User Id=%s; Password=%s" $dbHostname $dbDatabase $dbUser $dbPassword -}}
{{- end -}}

{{- define "twvault.scripts.secretName" -}}
{{- printf "%s-%s" (include "twvault.fullname" .) "pubscripts" -}}
{{- end -}}

{{- define "twvault.dbHostname" -}}
{{- printf "%s-postgres.%s.svc.cluster.local" (include "twvault.fullname" .) .Release.Namespace -}}
{{- end -}}

{{- define "twvault.waitForDb.available" -}}
{{- $dbname := (include "twvault.dbHostname" .) -}}
- name: wait-for-db-available
  image: alpine
  command: ['sh', '-c']
  args:
  - for i in $(seq 1 180);
    do
        if (nc -z -w 2 {{ $dbname }} 5432);
        then
            echo 'Successfully connected to postgres';
            exit 0;
        else
            echo 'Retrying connection to {{ $dbname }}:5432...';
        fi;
    done;
    echo 'Postgres unavailable';
    exit 1
{{- end -}}

{{- define "twvault.waitForDb.initialized" -}}
{{- $dbname := (include "twvault.dbHostname" .) -}}
{{- $user := .Values.env.app.ConnectionStrings__Vault__User -}}
{{- $db := .Values.env.app.ConnectionStrings__Vault__Database -}}
- name: wait-for-db-initialized
  image: jbergknoff/postgresql-client
  command: ['sh', '-c']
  args:
  - for i in {1..180};
    do
        if (timeout -t 2 sh -c "psql -U {{ $user }} -h {{ $dbname }} {{ $db }} -c \"\\dn\" | grep -q tw_provided" echo $?);
        then
            echo 'Successfully verified schema initialization';
            echo 'Sleeping for 5 seconds...';
            sleep 5000;
            exit 0;
        else
            echo 'Retrying connection to {{ $dbname }}:5432...';
        fi;
    done;
    echo 'Postgres unavailable or not initialized';
    exit 1
{{- end -}}

{{- define "twvault.waitForApp.running" -}}
{{- $appname := printf "%s-%s" (include "twvault.fullname" .) "app" -}}
- name: wait-for-app-running
  image: alpine
  command: ['sh', '-c']
  args:
  - for i in $(seq 1 180);
    do
        if (nc -z -w 2 {{ $appname }} 5000);
        then
            echo 'Successfully connected to TW.Vault.App';
            exit 0;
        else
            echo 'Retrying connection to {{ $appname }}:5000...';
        fi;
    done;
    echo 'TW.Vault.App unavailable';
    exit 1
{{- end -}}