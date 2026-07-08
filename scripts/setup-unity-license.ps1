#Requires -Version 5.1
<#
.SYNOPSIS
  Licenca Unity GRATIS (Personal) — SEM serial/key.
  Gera arquivo .alf no PC, voce ativa no site com login Unity, cola o .ulf no CI.

.EXAMPLE
  .\scripts\setup-unity-license.ps1
  .\scripts\setup-unity-license.ps1 -UploadToGitHub   # grava secret UNITY_LICENSE no GitHub
#>
param(
    [switch]$UploadToGitHub,
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-Location $ProjectRoot
New-Item -ItemType Directory -Force -Path "Build" | Out-Null

$unity = "C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe"
if (-not (Test-Path $unity)) {
    $found = Get-ChildItem "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe" -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($found) { $unity = $found.FullName } else { throw "Unity nao encontrado. Instale Unity 6000.5.2f1 pelo Hub." }
}

Write-Host @"
=== Unity Personal (GRATIS) — sem serial ===

Voce NAO precisa de "license key" / UNITY_SERIAL.
Conta Unity gratuita + arquivo .ulf e suficiente para o CI buildar iOS.

Passo 1/3: gerando arquivo de ativacao (.alf)...
"@

$log = Join-Path $ProjectRoot "Build\license-alf.log"
& $unity -batchmode -nographics -quit -createManualActivationFile -logFile $log
Start-Sleep 3

$alf = Get-ChildItem -Path $ProjectRoot, $env:USERPROFILE -Filter "*.alf" -Recurse -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $alf) {
    Write-Host "Log: $log"
    Get-Content $log -Tail 20 -ErrorAction SilentlyContinue
    throw "Arquivo .alf nao encontrado. Abra o Unity Hub, faca login com sua conta Unity (gratis) e tente de novo."
}

$alfDest = Join-Path $ProjectRoot "Build\UnityActivation.alf"
Copy-Item $alf.FullName $alfDest -Force
Write-Host "  .alf: $alfDest"

Write-Host @"

Passo 2/3: ativar no site (2 minutos)
  1. Abra: https://license.unity3d.com/manual
  2. Login com sua conta Unity (crie gratis se nao tiver — NAO e serial)
  3. Envie o arquivo: $alfDest
  4. Baixe o arquivo .ulf (Unity_v6000.x.ulf)

Salve o .ulf em: Build\Unity_lic.ulf
"@

Start-Process "https://license.unity3d.com/manual"
Start-Process "explorer.exe" -ArgumentList "/select,`"$alfDest`""

$ulfPath = Join-Path $ProjectRoot "Build\Unity_lic.ulf"
Read-Host "Pressione Enter depois de baixar o .ulf e salvar em Build\Unity_lic.ulf"
if (-not (Test-Path $ulfPath)) {
    $custom = Read-Host "Caminho do .ulf (ou Enter para cancelar)"
    if ($custom -and (Test-Path $custom)) { Copy-Item $custom $ulfPath -Force }
}
if (-not (Test-Path $ulfPath)) {
    Write-Host @"
[setup-unity-license] .ulf ainda nao encontrado.
Quando tiver o arquivo, rode:
  Copy-Item C:\Downloads\Unity_*.ulf Build\Unity_lic.ulf
  .\scripts\trigger-github-ios.ps1 -Mode xcode
"@
    exit 0
}

Write-Host "[setup-unity-license] .ulf OK: $ulfPath"

if ($UploadToGitHub) {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "git"; $psi.Arguments = "credential fill"
    $psi.RedirectStandardInput = $true; $psi.RedirectStandardOutput = $true; $psi.UseShellExecute = $false
    $p = [System.Diagnostics.Process]::Start($psi)
    $p.StandardInput.WriteLine("protocol=https"); $p.StandardInput.WriteLine("host=github.com"); $p.StandardInput.WriteLine("")
    $p.StandardInput.Close(); $out = $p.StandardOutput.ReadToEnd(); $p.WaitForExit()
    if ($out -match 'password=(\S+)') { $env:GH_TOKEN = $Matches[1] }
    gh secret set UNITY_LICENSE --repo Igorrv/teleport-native --body (Get-Content $ulfPath -Raw)
    Write-Host "[setup-unity-license] Secret UNITY_LICENSE gravado no GitHub."
}

Write-Host @"

Passo 3/3: disparar build iOS (GitHub Actions — mais rapido que Codemagic)

  .\scripts\trigger-github-ios.ps1 -Mode xcode    # ~25 min, baixa projeto Xcode
  .\scripts\trigger-github-ios.ps1 -Mode ipa      # ~40 min, tenta .ipa direto

Codemagic (alternativa): Environment > unity_credentials > UNITY_LICENSE = conteudo do .ulf
  (nao use UNITY_SERIAL — nao existe na conta Personal)

Depois do .ipa: .\scripts\install-iphone.ps1 -IpaPath ...
"@
