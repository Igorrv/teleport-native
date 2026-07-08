#Requires -Version 5.1
param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = "Stop"
Set-Location $ProjectRoot

$graphify = "C:\Users\Admin\AppData\Roaming\Python\Python314\Scripts\graphify.exe"
if (-not (Test-Path $graphify)) {
    $cmd = Get-Command graphify -ErrorAction SilentlyContinue
    if ($cmd) { $graphify = $cmd.Source } else { throw "graphify nao encontrado" }
}

$graph = Join-Path $ProjectRoot "graphify-out\graph.json"
if (-not (Test-Path $graph)) {
    Write-Host "[graphify-auto] Gerando grafo AST..."
    & $graphify update $ProjectRoot
}

$outDir = Join-Path $ProjectRoot "graphify-out\auto-runs"
New-Item -ItemType Directory -Force -Path $outDir | Out-Null
$stamp = Get-Date -Format "yyyy-MM-dd_HHmmss"
$report = Join-Path $outDir "improve-$stamp.md"

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("# Graphify auto-improve - $stamp")
[void]$sb.AppendLine("")
$promptPath = Join-Path $ProjectRoot ".graphify\AUTO_PROMPT.md"
if (Test-Path $promptPath) {
    foreach ($line in Get-Content $promptPath) { [void]$sb.AppendLine($line) }
}
[void]$sb.AppendLine("")
[void]$sb.AppendLine("## Queries")
[void]$sb.AppendLine("")

$queries = @(
    "what screens lack error handling or user feedback?",
    "what are the core modules and entry points?",
    "how does capture flow connect to splat rendering?"
)

foreach ($q in $queries) {
    [void]$sb.AppendLine("### $q")
    [void]$sb.AppendLine("---")
    $out = & $graphify query $q --graph $graph --budget 1000 2>&1
    foreach ($line in $out) { [void]$sb.AppendLine($line) }
    [void]$sb.AppendLine("---")
    [void]$sb.AppendLine("")
}

$sb.ToString() | Out-File $report -Encoding utf8
Write-Host "[graphify-auto] Relatorio: $report"
