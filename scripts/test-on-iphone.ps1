#Requires -Version 5.1
<#
.SYNOPSIS
  Guia rapido: testar teleport-native no iPhone (Windows + Codemagic + Sideloadly).

.EXAMPLE
  .\scripts\test-on-iphone.ps1
  .\scripts\test-on-iphone.ps1 -IpaPath C:\Downloads\app.ipa
#>
param(
    [string]$IpaPath = "",
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-Location $ProjectRoot

Write-Host @"
╔══════════════════════════════════════════════════════════╗
║  teleport-native — TESTE NO IPHONE (Windows)             ║
╚══════════════════════════════════════════════════════════╝

Fluxo: Codemagic gera .ipa (Mac na nuvem) -> Sideloadly instala no iPhone

"@ -ForegroundColor Cyan

$ulf = Join-Path $ProjectRoot "Build\Unity_lic.ulf"
if (-not (Test-Path $ulf)) {
    Write-Host "[1/4] Licenca Unity Personal (gratis, sem serial)..." -ForegroundColor Yellow
    Write-Host "      Rode: .\scripts\setup-unity-license.ps1`n"
    $run = Read-Host "Rodar setup agora? (s/n)"
    if ($run -eq "s") { & "$ProjectRoot\scripts\setup-unity-license.ps1" }
} else {
    Write-Host "[1/4] Unity_lic.ulf encontrado." -ForegroundColor Green
}

Write-Host @"
[2/4] Codemagic (no navegador):
      https://codemagic.io/apps
      - App: Igorrv/teleport-native
      - Environment > unity_credentials > UNITY_LICENSE (conteudo do .ulf)
      - Team settings > Apple Developer Portal > Apple ID
      - Start build: "iOS Test (Unity + IPA)"  (workflow ios-test)
      - Aguarde ~40-90 min (primeiro build demora)

[3/4] Baixe o .ipa em Artifacts quando terminar.

[4/4] Instale no iPhone (USB + Sideloadly):
"@ -ForegroundColor Cyan

if ($IpaPath -and (Test-Path $IpaPath)) {
    & "$ProjectRoot\scripts\install-iphone.ps1" -IpaPath $IpaPath
} else {
    & "$ProjectRoot\scripts\install-iphone.ps1"
}

Write-Host @"
No iPhone apos instalar:
  Ajustes > Privacidade > Camera > Teleport Native
  Ajustes > Geral > VPN e Gerenciamento > Confiar no desenvolvedor
"@ -ForegroundColor Green

Start-Process "https://codemagic.io/apps"
