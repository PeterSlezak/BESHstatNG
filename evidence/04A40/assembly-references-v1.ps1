param([string]$LibraryDirectory,[string]$Destination)
$ErrorActionPreference = 'Stop'
$rows = foreach ($assemblyName in @('BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll')) {
    $assemblyPath = Join-Path $LibraryDirectory $assemblyName
    $assemblyMetadata = [Reflection.Assembly]::ReflectionOnlyLoadFrom($assemblyPath)
    [pscustomobject]@{ assembly=$assemblyName; full_name=$assemblyMetadata.FullName; references=@($assemblyMetadata.GetReferencedAssemblies() | ForEach-Object { $_.FullName }) }
}
if (Test-Path -LiteralPath $Destination) { throw 'Refuse overwrite' }
[IO.File]::WriteAllText($Destination, ($rows | ConvertTo-Json -Depth 8), (New-Object Text.UTF8Encoding($false)))
