#Requires -Version 5.1
param(
    [string]$OutZip = "",
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)
$ErrorActionPreference = "Stop"
Set-Location $ProjectRoot
if (-not $OutZip) {
    New-Item -ItemType Directory -Force -Path "Build" | Out-Null
    $OutZip = Join-Path $ProjectRoot "Build\teleport-native-ios-src.zip"
}
$includes = @("Assets","Packages","ProjectSettings","codemagic.yaml",".github","scripts","BUILD.md","IPHONE.md","README.md","AGENTS.md",".gitignore") | Where-Object { Test-Path $_ }
if (Test-Path $OutZip) { Remove-Item $OutZip -Force }
Compress-Archive -Path $includes -DestinationPath $OutZip -CompressionLevel Optimal
$mb = [math]::Round((Get-Item $OutZip).Length / 1MB, 2)
Write-Host "[pack-for-mac] OK: $OutZip ($mb MB) — abra no Mac com Unity 6000.5.2f1 + iOS + Xcode"
