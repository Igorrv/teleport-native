#Requires -Version 5.1
<#
.SYNOPSIS
  Prepara e executa teleport-native localmente no Windows (Unity Editor ou build Standalone).
.EXAMPLE
  .\scripts\run-local.ps1
  .\scripts\run-local.ps1 -InstallHub
#>
param(
    [switch]$InstallHub,
    [switch]$BuildOnly,
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-Location $ProjectRoot

function Find-UnityExe {
    $hubRoot = "C:\Program Files\Unity\Hub\Editor"
    if (Test-Path $hubRoot) {
        $editors = Get-ChildItem $hubRoot -Directory | Sort-Object Name -Descending
        foreach ($ed in $editors) {
            $exe = Join-Path $ed.FullName "Editor\Unity.exe"
            if (Test-Path $exe) { return $exe }
        }
    }
    return $null
}

if ($InstallHub -and -not (Find-UnityExe)) {
    Write-Host "[run-local] Instalando Unity Hub via winget..."
    winget install Unity.UnityHub --accept-package-agreements --accept-source-agreements
    Write-Host "[run-local] Abra Unity Hub > Instalar Unity 6 LTS (6000.0.x) com modulo Windows Build Support."
    Write-Host "[run-local] Depois rode este script novamente."
    exit 0
}

$unity = Find-UnityExe
if (-not $unity) {
    Write-Host @"
[run-local] Unity Editor nao encontrado.

Passos:
  1. .\scripts\run-local.ps1 -InstallHub
  2. Unity Hub > Instalar Unity 6 LTS (6000.0.x)
     Modulos: Windows Build Support, Android (opcional)
  3. Rode este script novamente

Alternativa manual:
  Unity Hub > Add project > $ProjectRoot
  Menu Teleport > 4. Montar cena Main (auto)
  Menu Teleport > 7. Setup + Build Windows (teste local)
"@
    exit 1
}

Write-Host "[run-local] Unity: $unity"
Write-Host "[run-local] Gerando config + cena Main + build Windows..."

$log = Join-Path $ProjectRoot "Build\run-local.log"
New-Item -ItemType Directory -Force -Path (Join-Path $ProjectRoot "Build") | Out-Null

$args = @(
    "-batchmode", "-nographics", "-quit",
    "-projectPath", $ProjectRoot,
    "-executeMethod", "TeleportNative.Editor.LocalRunMenu.SetupAndBuildWindows",
    "-logFile", $log
)

$p = Start-Process -FilePath $unity -ArgumentList $args -Wait -PassThru -NoNewWindow
Write-Host "[run-local] Unity exit code: $($p.ExitCode)"
Write-Host "[run-local] Log: $log"

$exe = Join-Path $ProjectRoot "Build\Windows\teleport-native.exe"
if ($p.ExitCode -eq 0 -and (Test-Path $exe) -and -not $BuildOnly) {
    Write-Host "[run-local] Iniciando app..."
    Start-Process $exe -WorkingDirectory (Split-Path $exe)
} elseif (Test-Path $exe) {
    Write-Host "[run-local] Build OK: $exe"
} else {
    Write-Host "[run-local] Build falhou. Veja o log."
    Get-Content $log -Tail 40
    exit $p.ExitCode
}
