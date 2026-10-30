[CmdletBinding()]
param([ValidateSet('Capture','Check')][string]$Mode = 'Check')
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Set-Location $root
$destination = Join-Path $root 'TestResults/04A-01/protected'
$manifest = Join-Path $destination 'before.json'
if ($Mode -eq 'Capture') {
    if (Test-Path $manifest) { throw 'Snapshot exists; refusing to overwrite original evidence.' }
    New-Item -ItemType Directory -Force $destination | Out-Null
    $tracked = @(& git -c safe.directory=C:/Users/peter/source/repos/BESHstatNG ls-files)
    if ($LASTEXITCODE -ne 0) { throw 'Git census failed.' }
    # All tracked baseline bytes, not only a selected subset of protected files.
    $records = @($tracked | ForEach-Object {
        $path = $_
        $backup = Join-Path $destination ('bytes/' + $path)
        New-Item -ItemType Directory -Force (Split-Path $backup) | Out-Null
        Copy-Item -LiteralPath $path -Destination $backup
        [ordered]@{ path=$path; sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() }
    })
    ConvertTo-Json -Depth 5 -InputObject $records | Set-Content -Encoding UTF8 $manifest
    Write-Output ('Captured ' + $records.Count + ' tracked files; original bytes retained under ignored TestResults/04A-01/protected/bytes.')
} else {
    $records = Get-Content -Raw $manifest | ConvertFrom-Json
    $changed = @($records | Where-Object { -not (Test-Path -LiteralPath $_.path) -or (Get-FileHash -LiteralPath $_.path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $_.sha256 })
    $after = @($records | ForEach-Object {
        [ordered]@{path=$_.path; before_sha256=$_.sha256; after_sha256=if(Test-Path -LiteralPath $_.path){(Get-FileHash -LiteralPath $_.path -Algorithm SHA256).Hash.ToLowerInvariant()}else{'MISSING'} }
    })
    ConvertTo-Json -Depth 5 -InputObject $after | Set-Content -Encoding UTF8 (Join-Path $destination 'comparison.json')
    if ($changed.Count) { $changed.path | Write-Output; throw 'Baseline bytes changed. Inspect and restore only from the captured original copies if verification caused the changes.' }
    Write-Output ('PASS: all ' + $records.Count + ' baseline tracked files retain exact SHA-256.')
}
