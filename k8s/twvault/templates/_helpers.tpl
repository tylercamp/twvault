{{- define "twvault.name" -}}
{{- printf "%s-%s" $.Chart.Name . | trunc 63 | trimSuffix "-" -}}
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

{{- define "twvault.commonLabels" -}}
app.kubernetes.io/name: {{ include "twvault.name" $ }}
helm.sh/chart: {{ include "twvault.chart" $ }}
app.kubernetes.io/instance: {{ $.Release.Name }}
app.kubernetes.io/managed-by: {{ $.Release.Service }}
app.kubernetes.io/component: {{ . }}
{{- end -}}

{{- define "twvault.scripts.secretName" -}}
{{- printf "%s-%s" (include "twvault.fullname" .) "pubscripts" -}}
{{- end -}}