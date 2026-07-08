#Requires -Version 5.1
<#
.SYNOPSIS
  Instala teleport-native no iPhone via Sideloadly (Windows + cabo USB).
  Requer um arquivo .ipa (Codemagic/GitHub Actions/macOS).

.EXAMPLE
  .\scripts\install-iphone.ps1 -IpaPath .\Build\ios\teleport-native.ipa
  .\scripts\install-iphone.ps1   # abre Sideloadly se IPA existir em Build\ios\
#>
param(
    [string]$IpaPath = "",
    [string]$AppleId = "",
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-Location $ProjectRoot

function Find-Ipa {
    param([string]$Hint)
    if ($Hint -and (Test-Path $Hint)) { return (Resolve-Path $Hint).Path }
    $candidates = @(
        "$ProjectRoot\Build\ios\ipa\*.ipa",
        "$ProjectRoot\Build\ios\*.ipa",
        "$ProjectRoot\Build\*.ipa"
    )
    foreach ($pat in $candidates) {
        $f = Get-ChildItem $pat -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($f) { return $f.FullName }
    }
    return $null
}

function Find-Sideloadly {
    $paths = @(
        "$env:LOCALAPPDATA\Sideloadly\Sideloadly.exe",
        "$env:ProgramFiles\Sideloadly\Sideloadly.exe",
        "${env:ProgramFiles(x86)}\Sideloadly\Sideloadly.exe",
        "$env:USERPROFILE\Downloads\Sideloadly\Sideloadly.exe"
    )
    foreach ($p in $paths) { if (Test-Path $p) { return $p } }
    return $null
}

Write-Host @"
=== teleport-native -> iPhone (Windows) ===
iOS NAO compila no Windows. Fluxo:
  1) Codemagic ou GitHub Actions (macOS) gera o .ipa
  2) Baixe o .ipa para este PC
  3) Este script instala no iPhone via Sideloadly + cabo USB
"@

$ipa = Find-Ipa $IpaPath
if (-not $ipa) {
    Write-Host @"

[install-iphone] Nenhum .ipa encontrado.

Opcao A — Codemagic (recomendado):
  1. Crie conta em https://codemagic.io
  2. Add app > conecte este projeto (git)
  3. Configure Apple ID em Team settings
  4. Rode workflow 'ios-sideload' (codemagic.yaml)
  5. Baixe o artefato .ipa
  6. Rode: .\scripts\install-iphone.ps1 -IpaPath caminho\teleport-native.ipa

Opcao B — Mac emprestado/nuvem:
  Unity > Teleport > 8. Preparar export iOS
  Xcode > Run no iPhone conectado

Depois de ter o IPA, rode este script novamente.
"@
    exit 1
}

Write-Host "[install-iphone] IPA: $ipa"

$sly = Find-Sideloadly
if (-not $sly) {
    Write-Host "[install-iphone] Sideloadly nao encontrado. Baixando..."
    $zip = "$env:TEMP\SideloadlySetup.zip"
    $url = "https://sideloadly.io/SideloadlySetup.zip"
    try {
        Invoke-WebRequest -Uri $url -OutFile $zip -UseBasicParsing
        Expand-Archive -Path $zip -DestinationPath "$env:LOCALAPPDATA\Sideloadly" -Force
        $sly = Find-Sideloadly
    } catch {
        Write-Host "[install-iphone] Download automatico falhou. Instale manualmente: https://sideloadly.io"
        Start-Process "https://sideloadly.io"
        exit 1
    }
}

Write-Host "[install-iphone] Sideloadly: $sly"
Write-Host @"
Conecte o iPhone via USB e confie no computador.
No iPhone: Ajustes > Geral > VPN e Gerenciamento de Dispositivo (apos instalar).

Abrindo Sideloadly com o IPA...
"@

Start-Process $sly -ArgumentList @("`"$ipa`"")

if (-not $AppleId) {
    Write-Host "Informe seu Apple ID no Sideloadly (conta gratuita OK; app expira em 7 dias)."
} else {
    Write-Host "Apple ID fornecido - complete a assinatura no Sideloadly se solicitado."
}

Write-Host "[install-iphone] Pronto. Se o device nao aparecer, desbloqueie o iPhone e aceite Confiar neste computador."
