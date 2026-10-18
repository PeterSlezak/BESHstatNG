# Shared by the Windows help-generation target and optional Python validation.
[CmdletBinding()]
param([string]$PythonExe = $env:PythonExe)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Print only the resolved executable on success so MSBuild can capture it.
# Require the version used by the existing generators and their PyYAML dependency.
$probe = 'import sys; assert sys.version_info >= (3, 10), ''Python 3.10+ is required''; import yaml; print(sys.executable)'
$candidates = if ($PythonExe) { @($PythonExe) } else { @('python', 'python3', 'py') }
$failures = @()
foreach ($candidate in $candidates) {
    $command = Get-Command -Name $candidate -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $command) {
        $failures += "${candidate}: executable not found"
        continue
    }
    # Windows Store aliases can open an interactive installer; never invoke them.
    if ($command.Source -match '[\\/]Microsoft[\\/]WindowsApps[\\/]') {
        $failures += "${candidate}: Windows Store alias is not a Python installation"
        continue
    }
    $probeArguments = @('-B', '-c', $probe)
    if ([IO.Path]::GetFileNameWithoutExtension($command.Source) -eq 'py') {
        $probeArguments = @('-3') + $probeArguments
    }
    # Python writes import/version diagnostics to stderr. Capture them without
    # PowerShell 5.1 treating native stderr as a terminating PowerShell error.
    $savedErrorPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $output = @(& $command.Source @probeArguments 2>&1)
        $probeExitCode = $LASTEXITCODE
    }
    finally { $ErrorActionPreference = $savedErrorPreference }
    if ($probeExitCode -eq 0 -and $output.Count -eq 1 -and (Test-Path -LiteralPath ([string]$output[0]) -PathType Leaf)) {
        Write-Output ([string]$output[0])
        return
    }
    $failures += "${candidate}: $($output -join ' ')"
}
throw ("Python discovery failed. Set PythonExe to a Python 3.10+ executable with PyYAML installed " +
    "(MSBuild /p:PythonExe=... or verify-beshstatng.ps1 -PythonExe ...). " +
    "No packages were installed. Details: " + ($failures -join '; '))
