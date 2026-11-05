"""Embed the unchanged public multivariate documentation CSV in a new test fixture."""
import csv
import hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
CSV=ROOT/'BESHStatNG_Help_MkDocs/docs/assets/data/116multivariatecontrolcharts/116multivariatecontrolcharts.csv'
DEST=ROOT/'TestResults/04A-23-20261105-run01/drafts/Spc23Data.vb'
rows=list(csv.DictReader(CSV.read_text(encoding='utf-8-sig').splitlines()))
assert len(rows)==150
body='''Option Explicit On
Option Strict On
Imports BESHStatNG.StatisticalProcessControl
Public NotInheritable Class Spc23Data
    Public Const DatasetSha As String = "'''+hashlib.sha256(CSV.read_bytes()).hexdigest()+'''"
    Public Shared ReadOnly Charts As SpcChartType() = {SpcChartType.HotellingT2,SpcChartType.GeneralizedVariance,SpcChartType.PcaT2,SpcChartType.PcaQ,SpcChartType.Mewma,SpcChartType.Mcusum}
    Public Shared Function Measurements() As Double(,)
        Return New Double(,) {'''+',\n'.join('{'+','.join(r[k]+'R' for k in ['Temperature_C','Pressure_kPa','Flow_L_min'])+'}' for r in rows)+'''}
    End Function
End Class
'''
with DEST.open('x',encoding='utf-8',newline='\n') as f:f.write(body)
print('Embedded 150 x 3 public values;',hashlib.sha256(CSV.read_bytes()).hexdigest())
