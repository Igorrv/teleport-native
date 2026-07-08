#Requires -Version 5.1
<#
.SYNOPSIS
  Prepara o repositorio git e mostra os passos para gerar o .ipa no Codemagic (iPhone + camera AR).

.EXAMPLE
  .\scripts\prepare-iphone.ps1
  .\scripts\prepare-iphone.ps1 -RemoteUrl https://github.com/SEU_USUARIO/teleport-native.git
#>
param(
    [string]$RemoteUrl = "",
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-Location $ProjectRoot

Write-Host @"
=== teleport-native -> iPhone (camera AR) ===

Windows NAO compila iOS. Fluxo:
  1) Este script prepara o git
  2) Codemagic (macOS) gera o .ipa com ARKit
  3) scripts/install-iphone.ps1 instala no iPhone via USB
"@

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "[prepare-iphone] Git nao encontrado. Instale: winget install Git.Git"
    exit 1
}

if (-not (Test-Path ".git")) {
    git init
    git branch -M main
}

# Stage tudo relevante (ignora Library/ via .gitignore)
git add -A
$status = git status --porcelain
if ($status) {
    Write-Host "[prepare-iphone] Alteracoes prontas para commit (nao commitamos automaticamente)."
    git status -sb
} else {
    Write-Host "[prepare-iphone] Working tree limpo."
}

$remote = git remote get-url origin 2>$null
if (-not $remote -and $RemoteUrl) {
    git remote add origin $RemoteUrl
    $remote = $RemoteUrl
    Write-Host "[prepare-iphone] Remote origin: $remote"
} elseif ($remote) {
    Write-Host "[prepare-iphone] Remote origin: $remote"
} else {
    Write-Host @"

[prepare-iphone] Sem remote git. Crie um repo no GitHub e rode:
  git remote add origin https://github.com/SEU_USUARIO/teleport-native.git
  git commit -m "feat: app AR iPhone + UI captura"
  git push -u origin main
"@
}

Write-Host @"

--- Proximos passos ---

1. Commit + push (se ainda nao fez):
   git commit -m "feat: captura AR iPhone + layout UX"
   git push -u origin main

2. Codemagic (https://codemagic.io):
   - Add application > conecte o repo
   - Workflow: ios-sideload (codemagic.yaml)
   - Team settings > Apple Developer Portal (Apple ID)
   - Start build > baixe o .ipa

3. Instalar no iPhone (USB + Sideloadly):
   .\scripts\install-iphone.ps1 -IpaPath C:\Downloads\teleport-native.ipa

4. No iPhone:
   - Ajustes > Privacidade > Camera > permitir Teleport Native
   - Ajustes > Geral > VPN e Gerenciamento > confiar no perfil
   - Abra o app > onboarding > Captura AR (camera liga automaticamente)

Guia completo: IPHONE.md
"@
