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

{{- define "twvault.scripts.secretName" -}}
{{- printf "%s-%s" (include "twvault.fullname" .) "pubscripts" -}}
{{- end -}}