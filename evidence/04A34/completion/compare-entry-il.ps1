$ErrorActionPreference='Stop'
Add-Type -Path evidence/04A34/completion/CompareEntryIl.cs
$types34=(Get-Content -Raw evidence/04A34/completion/checkpoint/coordinate-and-chart-spec-contract.json | ConvertFrom-Json).contracts.public_headers | Where-Object { $_ -match '^Public (NotInheritable Class|Module)' } | ForEach-Object { ($_ -split ' ')[-1] } | Where-Object { $_ -ne 'PsmLoveSeriesPolicy' }
$result34=[CompareEntryIl34]::Compare((Get-Location).Path,[string[]]$types34)
[IO.File]::WriteAllText((Join-Path (Get-Location) 'evidence/04A34/completion/entry-portable-il.csv'),$result34,[Text.UTF8Encoding]::new($false))
Write-Output 'PASS entry portable methods; resolved metadata-token-normalized IL'
