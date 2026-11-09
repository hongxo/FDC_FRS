[CmdletBinding()]
param([Parameter(Mandatory = $true)][string]$EvidenceDirectory)

# Development/Lab test orchestration only. Never migrates, seeds, changes SQL permissions
# or retries mutations. Test-local concurrency scenarios remain concurrent; assemblies
# sharing the singleton empty-lab fixtures run sequentially. Credentials stay in environment.
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
if ($env:FDC_CENTER_CONFIRMED_LAB -ne 'FDC_Center_Lab' -or
    $env:FDC_IAM_CONFIRMED_LAB -ne 'FdcIam_DevelopmentLab') {
    throw 'Both disposable Lab targets must be explicitly confirmed.'
}
foreach ($entry in @(
    @{ Variable = 'FDC_CENTER_LAB_CONNECTION'; Database = 'FDC_Center_Lab' },
    @{ Variable = 'FDC_IAM_LAB_CONNECTION'; Database = 'FdcIam_DevelopmentLab' }
)) {
    $value = [Environment]::GetEnvironmentVariable($entry.Variable)
    if ([string]::IsNullOrWhiteSpace($value)) { throw 'Supply Lab connections through the secret environment.' }
    $settings = [System.Data.Common.DbConnectionStringBuilder]::new()
    # Explicit accessors avoid PowerShell's IDictionary adapter treating ConnectionString as a key.
    $settings.set_ConnectionString($value)
    if ($settings.get_Item('Server') -ne 'lpc:.' -or $settings.get_Item('Database') -ne $entry.Database) {
        throw 'Only the explicitly named local Lab connections are supported.'
    }
    $settings.Clear(); $value = $null
}
$output = [IO.Path]::GetFullPath($EvidenceDirectory)
New-Item -ItemType Directory -Force -Path $output | Out-Null
$mutex = [Threading.Mutex]::new($false, 'Local\FdcD12LabVerification')
$ownsMutex = $false
try {
    $ownsMutex = $mutex.WaitOne(0)
    if (!$ownsMutex) { throw 'Another Lab verification harness is running.' }
    Push-Location $repo
    try {
        & dotnet build Fdc.Identity.slnx --no-restore -v minimal *> (Join-Path $output 'build.log')
        if ($LASTEXITCODE -ne 0) { throw 'Build failed; raw evidence retained.' }
        foreach ($name in @('Fdc.Identity.Tests', 'Fdc.Center.Tests')) {
            $result = Join-Path $output $name
            New-Item -ItemType Directory -Force -Path $result | Out-Null
            & dotnet test "tests/$name/$name.csproj" --no-build --no-restore --logger trx --results-directory $result *> (Join-Path $result 'test.log')
            if ($LASTEXITCODE -ne 0) { throw "Test assembly failed: $name; raw evidence retained. No automatic retry." }
        }
    }
    finally { Pop-Location }
}
finally {
    if ($ownsMutex) { $mutex.ReleaseMutex() }
    $mutex.Dispose()
}
