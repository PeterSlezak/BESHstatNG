# Owner-run handoff. Allocation is explicit; the collector itself is read-only.
# No R/Excel process, permission change, elevation, impersonation or statistical execution.
param(
    [Parameter(Mandatory=$true)][string]$Root,
    [Parameter(Mandatory=$true)][string]$SourceDeclaration,
    [switch]$AllocateEmptyRoot
)
$ErrorActionPreference='Stop'
$identity=[Security.Principal.WindowsIdentity]::GetCurrent()
$expected='S-1-5-21-142429299-24905182-1790040237-1002'
if ($identity.User.Value -ne $expected) { throw "OWNER_CONTEXT_REQUIRED: actual $($identity.Name) / $($identity.User.Value); expected Peter / $expected. Do not elevate or impersonate." }
$principal=New-Object Security.Principal.WindowsPrincipal($identity)
if ($principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) { throw 'Use normal non-elevated owner Developer PowerShell.' }
$literal=[IO.Path]::GetFullPath($Root)
$local=[Environment]::GetFolderPath('LocalApplicationData')
if (-not $literal.StartsWith($local+'\',[StringComparison]::OrdinalIgnoreCase) -or
    [IO.Directory]::GetParent($literal).FullName -ne $local -or $literal.Contains('..')) { throw 'One direct child of the actual owner LocalAppData required.' }
if (-not $AllocateEmptyRoot) { throw 'Explicit empty-root allocation flag required; collection never changes permissions.' }
if (Test-Path -LiteralPath $literal) { throw 'Never reuse or overwrite a reference root; choose a new reviewed literal root.' }
$cursor=Get-Item -LiteralPath $local -Force
while ($cursor) {
    if ($cursor.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'Reparse allocation ancestor; stop without creating root.' }
    $cursor=$cursor.Parent
}
# The single new directory inherits the owner's existing permissions, unchanged.
New-Item -ItemType Directory -Path $literal -ErrorAction Stop | Out-Null
$raw=Join-Path $literal 'owner-acl-raw.json'
$collector=Join-Path $PSScriptRoot 'inspect_output_acl_v4.ps1'
$info=New-Object Diagnostics.ProcessStartInfo
$info.FileName='powershell.exe'
$info.Arguments='-NoProfile -ExecutionPolicy Bypass -File "'+$collector+'" -OutputRoot "'+$literal+'"'
$info.UseShellExecute=$false;$info.CreateNoWindow=$true;$info.RedirectStandardOutput=$true;$info.RedirectStandardError=$true
$process=New-Object Diagnostics.Process;$process.StartInfo=$info
if (-not $process.Start()) { throw 'Collector did not start; root retained.' }
$outTask=$process.StandardOutput.ReadToEndAsync();$errTask=$process.StandardError.ReadToEndAsync()
if (-not $process.WaitForExit(30000)) { $process.Kill();throw 'Read-only collector deadline; only this launched helper stopped; root retained.' }
$stdout=$outTask.Result;$stderr=$errTask.Result
function Write-New([string]$path,[string]$text) {
    $stream=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
    try { $bytes=(New-Object Text.UTF8Encoding($false)).GetBytes($text);$stream.Write($bytes,0,$bytes.Length);$stream.Flush($true) } finally {$stream.Dispose()}
}
Write-New $raw $stdout
Write-New (Join-Path $literal 'owner-acl-stderr.txt') $stderr
$receipt=[ordered]@{schema='04A02-C09-owner-collection-receipt-v1';execution_name=$identity.Name
    execution_sid=$identity.User.Value;source_declaration=$SourceDeclaration;root=$literal
    collector_sha256=(Get-FileHash -LiteralPath $collector -Algorithm SHA256).Hash.ToLowerInvariant()
    raw_sha256=(Get-FileHash -LiteralPath $raw -Algorithm SHA256).Hash.ToLowerInvariant()
    exit_code=$process.ExitCode;status='COLLECTED_PENDING_TECHNICAL_REVIEW';R_executed=$false;root_allocated_empty=$true
    reviewer=$identity.Name;declaration_scope='Operator collected these actual owner-context observations. No independent statistical review or ACL acceptance is inferred.'}
Write-New (Join-Path $literal 'owner-collection-receipt.json') ($receipt|ConvertTo-Json -Depth 6)
Write-Output "Owner identity: $($identity.Name) / $($identity.User.Value)"
Write-Output "Return these retained files: $literal\owner-acl-raw.json and owner-collection-receipt.json (plus stderr)."
Write-Output "Raw SHA256: $($receipt.raw_sha256). No R executed. Review effective rights and v4 threat model before technical GO."
if ($process.ExitCode -ne 0 -or $stderr) { throw 'Collector failure preserved; no retry or execution.' }
