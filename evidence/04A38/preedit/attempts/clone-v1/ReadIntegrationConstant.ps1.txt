param([Parameter(Mandatory=$true)][string]$IntegrationDll)
$ErrorActionPreference = 'Stop'
$integrationAssembly = [Reflection.Assembly]::LoadFrom($IntegrationDll)
$integrationAssembly.GetType('ExcelDna.Integration.XlCall').GetField('xlCoerce').GetRawConstantValue()
