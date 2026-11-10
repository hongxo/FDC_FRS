param(
    [switch]$ConfirmDisposable,
    [string]$RunId = ([DateTime]::UtcNow.ToString('yyyyMMddTHHmmss') + '-' + [Guid]::NewGuid().ToString('N')),
    [string]$NativeLibrary,
    [string]$Simulator
)
$ErrorActionPreference = 'Stop'
if (!$ConfirmDisposable) { throw 'Explicit -ConfirmDisposable is required.' }
if ($RunId -notmatch '^[a-zA-Z0-9_-]{1,80}$') { throw 'Invalid run ID.' }
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$evidence = Join-Path $repoRoot "tmp/phase4-4ca10/$RunId"
if (Test-Path -LiteralPath $evidence) { throw 'A new evidence directory is required.' }
Get-Command sqlcmd -ErrorAction Stop | Out-Null
foreach ($name in @('FDC_IAM_LAB_CONNECTION','FDC_IAM_CONFIRMED_LAB','FDC_CENTER_LAB_CONNECTION',
    'FDC_CENTER_CONFIRMED_LAB','FDC_CENTER_RUNTIME_SECRET_FILE')) {
    if ([Environment]::GetEnvironmentVariable($name)) { throw "Remove shared or named Lab profile $name before this run." }
}
$master = 'Server=lpc:.;Database=master;Integrated Security=true;Encrypt=false;TrustServerCertificate=true'
$settings = @{
    FDC_RUN_A10_SQL='1'; FDC_A10_SQL_MASTER=$master; FDC_IAM_RUN_PROFILE='1'; FDC_IAM_EVIDENCE=$evidence
    FDC_RUN_A9_DISPOSABLE='1'; FDC_A9_SQL_MASTER=$master; FDC_CENTER_EVIDENCE=$evidence; FDC_RUN_AUTHORITY_PROFILE='1'
    FDC_RUN_A7='1'; FDC_A7_SQL_MASTER=$master; FDC_RUN_CENTER_BROWSER='1'; FDC_RUN_A9_FILE_LINK='1'
    FDC_UI_EVIDENCE=$evidence; FDC_PILOT_SIMULATED=$null; FDC_PILOT_EVIDENCE=$evidence
    FDC_PILOT_SIMULATOR=$null; FDC_PILOT_SIMULATOR_SHA256=$null
    FDC_PILOT_NATIVE_LIBRARY=$null; FDC_PILOT_NATIVE_SHA256=$null
}
if ($NativeLibrary -or $Simulator) {
    if (!$NativeLibrary -or !$Simulator) { throw 'Supply both pinned simulator and native library paths.' }
    $expectedLibrary = '6b7a29612dee4167ed448e05f36b91e7d0fa9dd4e165b20eb7f8d6f806eceaa4'
    $expectedSimulator = 'b2a51a1a77f7155676985982a1674d9bf59226598eccdbde46f75cfdd7a32420'
    if ((Get-FileHash -LiteralPath $NativeLibrary -Algorithm SHA256).Hash -ine $expectedLibrary -or
        (Get-FileHash -LiteralPath $Simulator -Algorithm SHA256).Hash -ine $expectedSimulator) { throw 'Simulator artifact hash mismatch.' }
    $settings.FDC_PILOT_SIMULATED='1'
    $settings.FDC_PILOT_NATIVE_LIBRARY=(Resolve-Path -LiteralPath $NativeLibrary).Path
    $settings.FDC_PILOT_NATIVE_SHA256=$expectedLibrary
    $settings.FDC_PILOT_SIMULATOR=(Resolve-Path -LiteralPath $Simulator).Path
    $settings.FDC_PILOT_SIMULATOR_SHA256=$expectedSimulator
}
$saved = @{}
New-Item -ItemType Directory -Path $evidence | Out-Null
Push-Location -LiteralPath $repoRoot
try {
    $sha = (git rev-parse HEAD).Trim()
    $settings.FDC_A5_COMMIT=$sha
    $settings.FDC_IAM_SOURCE_SHA=$sha
    foreach ($name in $settings.Keys) {
        $saved[$name] = [Environment]::GetEnvironmentVariable($name)
        [Environment]::SetEnvironmentVariable($name, $settings[$name])
    }
    dotnet build Fdc.Identity.slnx --no-restore *> (Join-Path $evidence 'build.log')
    if ($LASTEXITCODE) { throw 'Build failed. Inspect build.log.' }
    dotnet test Fdc.Identity.slnx --no-build --logger trx --results-directory (Join-Path $evidence 'trx') *> (Join-Path $evidence 'tests.log')
    $testExit = $LASTEXITCODE
    sqlcmd -S lpc:. -E -C -d master -b -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name LIKE N'FDC[_]A10[_]TEST[_]%' OR name LIKE N'FDC[_]A9[_]TEST[_]%' OR name LIKE N'FdcA7[_]%';" > (Join-Path $evidence 'cleanup.log')
    if ($LASTEXITCODE) { throw 'Cleanup verification query failed. Inspect cleanup.log.' }
    if ([int](Get-Content -LiteralPath (Join-Path $evidence 'cleanup.log') -Raw).Trim() -ne 0) {
        throw 'Owned fixture databases remain. Inspect cleanup.log and ownership markers before any manual cleanup.'
    }
    [pscustomobject]@{ Commit=$sha; Evidence=$evidence; TestExit=$testExit; Environment='Owned disposable SQL and loopback only' } |
        ConvertTo-Json | Set-Content -Encoding utf8 -LiteralPath (Join-Path $evidence 'run.json')
    if ($testExit) { throw 'Tests failed. Inspect tests.log and TRX.' }
    Write-Output $evidence
}
finally {
    foreach ($name in $saved.Keys) { [Environment]::SetEnvironmentVariable($name, $saved[$name]) }
    Pop-Location
}
