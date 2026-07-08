#Requires -Version 5.1
<#
.SYNOPSIS
  Gera licenca Unity Personal (GRATIS) para Codemagic — SEM serial/key.
  Conta Unity gratuita + arquivo .ulf (5 minutos).

.EXAMPLE
  .\scripts\setup-unity-license.ps1
#>
param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-Location $ProjectRoot
New-Item -ItemType Directory -Force -Path "Build" | Out-Null

$unity = "C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe"
if (-not (Test-Path $unity)) {
    $unity = (Get-ChildItem "C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe" -ErrorAction SilentlyContinue | Select-Object -First 1).FullName
}
if (-not $unity) { throw "Instale Unity 6000.5.2f1 pelo Unity Hub." }

Write-Host "=== Unity Personal (gratis) — sem serial ===" -ForegroundColor Cyan
Write-Host "Gerando .alf ..."
& $unity -batchmode -nographics -quit -createManualActivationFile -logFile "Build\license.log"
Start-Sleep 2

$alf = Get-ChildItem -Path $ProjectRoot, $env:USERPROFILE -Filter "*.alf" -Recurse -ErrorAction SilentlyContinue |
    Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $alf) { throw "Arquivo .alf nao gerado. Faca login no Unity Hub e tente de novo." }

$dest = Join-Path $ProjectRoot "Build\UnityActivation.alf"
Copy-Item $alf.FullName $dest -Force

Write-Host @"

1. Abra: https://license.unity3d.com/manual
2. Login Unity (conta GRATIS — crie se nao tiver)
3. Envie: $dest
4. Baixe o .ulf e salve em: Build\Unity_lic.ulf

"@
Start-Process "https://license.unity3d.com/manual"
Start-Process explorer.exe -ArgumentList "/select,`"$dest`""

Read-Host "Pressione Enter apos salvar Build\Unity_lic.ulf"

$ulf = Join-Path $ProjectRoot "Build\Unity_lic.ulf"
if (-not (Test-Path $ulf)) {
    $p = Read-Host "Caminho do .ulf baixado"
    if ($p -and (Test-Path $p)) { Copy-Item $p $ulf -Force }
}
if (-not (Test-Path $ulf)) { throw "Unity_lic.ulf nao encontrado." }

Write-Host @"

=== PROXIMO: Codemagic ===

1. https://codemagic.io/apps
2. App teleport-native > Environment variables
3. Group: unity_credentials
4. Nova variavel:
   Nome: UNITY_LICENSE
   Valor: abra Build\Unity_lic.ulf no Bloco de Notas, COPIE TUDO, cole
   (marque Secure)

5. Team settings > Integrations > Apple Developer Portal > seu Apple ID

6. Start build > workflow "iOS Test (Unity + IPA)"

7. Baixe o .ipa e rode:
   .\scripts\install-iphone.ps1 -IpaPath caminho\app.ipa

"@ -ForegroundColor Green

$copy = Read-Host "Copiar conteudo do .ulf para a area de transferencia? (s/n)"
if ($copy -eq "s") {
    Get-Content $ulf -Raw | Set-Clipboard
    Write-Host "Conteudo copiado — cole no Codemagic como UNITY_LICENSE."
}
