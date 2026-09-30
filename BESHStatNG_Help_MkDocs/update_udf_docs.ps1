Param(
  [Parameter(Mandatory=$false)][string]$AddinRoot = "..\BESHStatNG",
  [Parameter(Mandatory=$false)][string]$OutputDir = "docs\udf"
)

$udfs = Join-Path $AddinRoot "udfs"
if (-not (Test-Path $udfs)) {
  throw "UDF folder not found: $udfs"
}

python (Join-Path $PSScriptRoot "generate_udf_docs.py") --udfs-dir $udfs --output-dir $OutputDir
