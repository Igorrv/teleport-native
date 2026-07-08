#Requires -Version 5.1
<#
.SYNOPSIS
  Gera base64 para secrets Apple do GitHub Actions.

.EXAMPLE
  .\scripts\setup-apple-secrets.ps1 -CertPath "C:\certs\dev.p12" -ProfilePath "C:\certs\Teleport.mobileprovision"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$CertPath,
    [Parameter(Mandatory = $true)]
    [string]$ProfilePath,
    [string]$TeamId = "",
    [string]$SigningIdentity = "Apple Development"
)

if (-not (Test-Path $CertPath)) { throw "Certificado nao encontrado: $CertPath" }
if (-not (Test-Path $ProfilePath)) { throw "Profile nao encontrado: $ProfilePath" }

$certB64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($CertPath))
$profileB64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($ProfilePath))

Write-Host @"

=== GitHub Secrets (Settings > Secrets and variables > Actions) ===

APPLE_CERTIFICATE_BASE64
  (copiado para clipboard)

APPLE_PROVISIONING_PROFILE_BASE64
  (copiado para clipboard - use depois do cert)

APPLE_CERTIFICATE_PASSWORD
  = senha do arquivo .p12 (voce define ao exportar)

APPLE_TEAM_ID
  = Team ID em https://developer.apple.com/account (Membership)

APPLE_SIGNING_IDENTITY
  = ex: "Apple Development: Seu Nome (TEAMID)"
  Keychain Access > certificados > copie o nome exato

KEYCHAIN_PASSWORD
  = qualquer string aleatoria (ex: ci-keychain-$(Get-Random))

"@ -ForegroundColor Cyan

$certB64 | Set-Clipboard
Write-Host "APPLE_CERTIFICATE_BASE64 copiado. Cole no GitHub, depois rode:" -ForegroundColor Green
Write-Host "  Get-Content ... | Set-Clipboard  # para profile" -ForegroundColor Yellow

if ($TeamId) { Write-Host "APPLE_TEAM_ID sugerido: $TeamId" }
Write-Host "APPLE_SIGNING_IDENTITY sugerido: $SigningIdentity"

# Salva arquivos locais para referencia (nao commitar)
$out = Join-Path (Split-Path -Parent $PSScriptRoot) "Build"
New-Item -ItemType Directory -Force -Path $out | Out-Null
Set-Content -Path (Join-Path $out "apple_cert.b64.txt") -Value $certB64 -NoNewline
Set-Content -Path (Join-Path $out "apple_profile.b64.txt") -Value $profileB64 -NoNewline
Write-Host "Base64 salvos em Build/apple_cert.b64.txt e Build/apple_profile.b64.txt (gitignore)"
