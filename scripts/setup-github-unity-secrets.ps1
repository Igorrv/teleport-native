#Requires -Version 5.1
<#
.SYNOPSIS
  Configura secrets Unity no GitHub e dispara workflow iOS Build.

.EXAMPLE
  $pwd = Read-Host "Senha Unity" -AsSecureString
  .\scripts\setup-github-unity-secrets.ps1 -Email "seu@email.com" -Password $pwd -RunWorkflow

  # So a senha (nao o texto do prompt):
  #   Read-Host "Senha Unity" -AsSecureString   <- correto
  #   Read-Host "minhasenha" -AsSecureString    <- ERRADO (isso e so o rotulo)
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Email,
    [Parameter(Mandatory = $true)]
    [SecureString]$Password,
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [switch]$RunWorkflow
)

$ErrorActionPreference = "Stop"
$repo = "Igorrv/teleport-native"

# gh auth via git credential (mesmo token do push)
if (-not $env:GH_TOKEN) {
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = "git"
    $psi.Arguments = "credential fill"
    $psi.RedirectStandardInput = $true
    $psi.RedirectStandardOutput = $true
    $psi.UseShellExecute = $false
    $proc = [System.Diagnostics.Process]::Start($psi)
    $proc.StandardInput.WriteLine("protocol=https")
    $proc.StandardInput.WriteLine("host=github.com")
    $proc.StandardInput.WriteLine("")
    $proc.StandardInput.Close()
    $credOut = $proc.StandardOutput.ReadToEnd()
    $proc.WaitForExit()
    if ($credOut -match 'password=(\S+)') { $env:GH_TOKEN = $Matches[1] }
}
if (-not (gh auth status 2>$null)) { throw "gh nao autenticado. Rode: gh auth login" }

$ulfSrc = Join-Path $env:ProgramData "Unity\Unity_lic.ulf"
$ulfLocal = Join-Path $ProjectRoot "Build\Unity_lic.ulf"
if (-not (Test-Path $ulfSrc)) { throw "Licenca nao encontrada: $ulfSrc" }
Copy-Item $ulfSrc $ulfLocal -Force

$plain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password))

Write-Host "Configurando secrets em $repo ..." -ForegroundColor Cyan
Get-Content $ulfLocal -Raw | gh secret set UNITY_LICENSE --repo $repo
if ($LASTEXITCODE -ne 0) { throw "Falha ao salvar UNITY_LICENSE" }
gh secret set UNITY_EMAIL --repo $repo --body $Email
if ($LASTEXITCODE -ne 0) { throw "Falha ao salvar UNITY_EMAIL" }
gh secret set UNITY_PASSWORD --repo $repo --body $plain
if ($LASTEXITCODE -ne 0) { throw "Falha ao salvar UNITY_PASSWORD" }

Write-Host "Secrets OK: UNITY_LICENSE, UNITY_EMAIL, UNITY_PASSWORD" -ForegroundColor Green
gh secret list --repo $repo

if ($RunWorkflow) {
    gh workflow run "iOS Build" --repo $repo -f signed=false
    Start-Sleep 2
    gh run list --repo $repo --workflow "ios-build.yml" --limit 1
    Write-Host "Acompanhe: https://github.com/$repo/actions" -ForegroundColor Yellow
}
