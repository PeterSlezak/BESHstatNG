$ErrorActionPreference = 'Stop'
# An audit-helper build only. No restore feeds, product builds, shared outputs or installs.
$root = (Get-Location).Path
$scratch = Join-Path $root '.04a29-audit.tmp'
if (Test-Path -LiteralPath $scratch) { throw 'Refuse existing helper output directory; use unchanged compiled helper for repeat runs.' }
New-Item -ItemType Directory -Path $scratch | Out-Null
& 'C:/Program Files/dotnet/dotnet.exe' build evidence/04A29/helpers/GraphicsAudit.csproj --configfile evidence/04A29/helpers/NuGet.Config "-p:BaseIntermediateOutputPath=$scratch/obj/" "-p:OutputPath=$scratch/bin/" --nologo --verbosity quiet
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
