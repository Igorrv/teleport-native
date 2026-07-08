#Requires -Version 5.1
<#
.SYNOPSIS
  Dispara build iOS no GitHub Actions (macOS na nuvem — sem Unity no seu PC).

.EXAMPLE
  .\scripts\trigger-github-ios.ps1 -Mode xcode
  .\scripts\trigger-github-ios.ps1 -Mode ipa
#>
param(
    [ValidateSet("xcode", "ipa")]
    [string]$Mode = "xcode",
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-Location $ProjectRoot

$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = "git"; $psi.Arguments = "credential fill"
$psi.RedirectStandardInput = $true; $psi.RedirectStandardOutput = $true; $psi.UseShellExecute = $false
$p = [System.Diagnostics.Process]::Start($psi)
$p.StandardInput.WriteLine("protocol=https"); $p.StandardInput.WriteLine("host=github.com"); $p.StandardInput.WriteLine("")
$p.StandardInput.Close(); $out = $p.StandardOutput.ReadToEnd(); $p.WaitForExit()
if ($out -match 'password=(\S+)') { $env:GH_TOKEN = $Matches[1] }

if (-not (gh auth status 2>$null)) {
    Write-Host "GitHub CLI nao autenticado. Usando credenciais git..."
}

$ulf = Join-Path $ProjectRoot "Build\Unity_lic.ulf"
if (-not (gh secret list --repo Igorrv/teleport-native 2>$null | Select-String "UNITY_LICENSE")) {
    if (Test-Path $ulf) {
        Write-Host "Gravando UNITY_LICENSE no GitHub..."
        gh secret set UNITY_LICENSE --repo Igorrv/teleport-native --body (Get-Content $ulf -Raw)
    } else {
        Write-Host @"
[trigger-github-ios] Falta secret UNITY_LICENSE no GitHub.
Rode primeiro: .\scripts\setup-unity-license.ps1
"@
        exit 1
    }
}

Write-Host "[trigger-github-ios] Disparando workflow mode=$Mode ..."
gh workflow run "Build iOS IPA" --repo Igorrv/teleport-native -f mode=$Mode
Start-Sleep 3
$run = gh run list --repo Igorrv/teleport-native --workflow "Build iOS IPA" --limit 1 --json databaseId,url,status --jq ".[0]"
Write-Host "Build iniciado: $run"
Write-Host @"
Acompanhe: https://github.com/Igorrv/teleport-native/actions
Quando terminar: Actions > ultimo run > Artifacts > ios-xcode ou ios-ipa

Modo xcode: mais rapido e confiavel. O .ipa final pode ser assinado no Sideloadly
            se voce tiver um Mac para archive, OU use modo ipa (pode falhar sem cert Apple).
"@
