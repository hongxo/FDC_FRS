[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[a-zA-Z0-9][a-zA-Z0-9_-]{0,63}$')][string]$RunId,
    [string]$Workbook,
    [switch]$LocalIntegration,
    [switch]$Browser
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$run = Join-Path $repo "tmp/phase4-4b/$RunId"
if (Test-Path -LiteralPath $run) { throw 'Use a new RunId. Existing evidence is retained.' }
if ($LocalIntegration) {
    $connection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $connection.set_ConnectionString($env:FDC_CENTER_LAB_CONNECTION)
    if ($env:FDC_CENTER_CONFIRMED_LAB -ne 'FDC_Center_Lab' -or $env:FDC_PILOT_SIMULATED -ne '1' -or
        $connection.get_Item('Server') -ne 'lpc:.' -or $connection.get_Item('Database') -ne 'FDC_Center_Lab') {
        throw 'Use only the explicitly confirmed local shared-memory SQL Lab and simulator profile.'
    }
    $allowedKeys = 'server','database','user id','password','integrated security','encrypt','trustservercertificate'
    if ($connection.get_Keys() | Where-Object { $_ -notin $allowedKeys }) {
        throw 'Unexpected connection option in the local Lab profile.'
    }
    foreach ($name in 'FDC_PILOT_SIMULATOR','FDC_PILOT_NATIVE_LIBRARY') {
        $path = [Environment]::GetEnvironmentVariable($name)
        $pinName = if ($name -eq 'FDC_PILOT_NATIVE_LIBRARY') { 'FDC_PILOT_NATIVE_SHA256' } else { 'FDC_PILOT_SIMULATOR_SHA256' }
        $pin = [Environment]::GetEnvironmentVariable($pinName)
        if (!$path -or $pin -notmatch '^[a-fA-F0-9]{64}$' -or (Get-FileHash -LiteralPath $path).Hash -ne $pin) {
            throw "Missing input or fixture hash mismatch for $name."
        }
    }
    if (!$env:FDC_CENTER_RUNTIME_SECRET_FILE -or !(Test-Path -LiteralPath $env:FDC_CENTER_RUNTIME_SECRET_FILE -PathType Leaf)) {
        throw 'The local Center runtime secret file is required.'
    }
}
else {
    foreach ($name in 'FDC_CENTER_CONFIRMED_LAB','FDC_CENTER_LAB_CONNECTION','FDC_PILOT_SIMULATED') {
        if ([Environment]::GetEnvironmentVariable($name)) { throw 'Clear integration variables or select -LocalIntegration.' }
    }
}

$null = New-Item -ItemType Directory -Path $run
function Invoke-Checked {
    param([string]$Executable, [string[]]$Arguments, [string]$Log)
    & $Executable @Arguments *> (Join-Path $run $Log)
    if ($LASTEXITCODE -ne 0) { throw "$Executable failed. See private evidence $Log." }
}

$previousEvidence = $env:FDC_PILOT_EVIDENCE, $env:FDC_CENTER_EVIDENCE, $env:FDC_PILOT_DASHBOARD
Push-Location $repo
try {
    $env:FDC_PILOT_EVIDENCE = $run
    $env:FDC_CENTER_EVIDENCE = $run
    Invoke-Checked dotnet @('build','Fdc.Identity.slnx','--no-restore','-v','q') 'build.log'
    Invoke-Checked dotnet @('test','Fdc.Identity.slnx','--no-build','--no-restore','--logger','trx','--results-directory',(Join-Path $run 'trx')) 'tests.log'
    $tool = @('run','--project','tools/Fdc.LabPilot','--no-build','--no-restore','--')
    Invoke-Checked dotnet ($tool + @('offline-demo',(Join-Path $run 'offline'))) 'offline.log'
    $inventoryLink = ''
    if ($Workbook) {
        Invoke-Checked dotnet ($tool + @('inventory-preview',$Workbook,'IP','2','LAB',(Join-Path $run 'inventory.html'))) 'inventory.log'
        Invoke-Checked dotnet ($tool + @('acceptance-map',$Workbook,'IP','2','LAB',(Join-Path $run 'mapping.private.json'))) 'mapping.log'
        $inventoryLink = '<li><a href="inventory.html">Private inventory and device capabilities</a></li>'
    }
    if ($Browser) {
        $env:FDC_PILOT_DASHBOARD = Join-Path $run 'offline/dashboard'
        Invoke-Checked node @('tests/Fdc.Collector.Pilot.Tests/browser/offline-dashboard.cjs') 'browser.log'
    }

    $profile = if ($LocalIntegration) { 'Offline and isolated local simulation' } else { 'Offline only' }
    $docs = '../../../docs/fdc-frs-lab-pilot/phase-4.4B'
    @"
<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width">
<meta http-equiv="Content-Security-Policy" content="default-src 'none'">
<title>FDC/FRS Lab acceptance</title><h1>Lab acceptance evidence</h1>
<p>Profile: $profile. Independent review is pending.</p>
<p>Real IED, SCADA coexistence, pre-site acceptance and integrated OT security: BLOCKED.</p>
<ul>$inventoryLink
<li><a href="offline/dashboard/index.html">Collector queue, file discovery, records and waveforms (offline fixture)</a></li>
<li><a href="offline/run.json">Offline run data</a></li>
<li><a href="$docs/LAB_FDC_Test_Matrix.md">Test execution matrix</a></li>
<li><a href="$docs/SCADA_Coexistence_Baseline.md">SCADA coexistence</a></li>
<li><a href="$docs/Defect_Register.md">Defect register</a></li>
<li><a href="$docs/Lab_Acceptance_Report.md">Acceptance summary</a></li>
</ul></html>
"@ | Set-Content -LiteralPath (Join-Path $run 'index.html') -Encoding utf8
    Write-Output "Private evidence: $run"
}
finally {
    $env:FDC_PILOT_EVIDENCE, $env:FDC_CENTER_EVIDENCE, $env:FDC_PILOT_DASHBOARD = $previousEvidence
    Pop-Location
}
