"""One-time, pre-extraction source characterization; NOT native or scientific reference evidence."""
import hashlib
import json
from pathlib import Path
import re

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-06-20261102-run01'
if not (RUN/'start.json').is_file(): raise ValueError('Initialize preservation first')
path=ROOT/'BESHstatNG/src/UI/UibyID.vb'
source=path.read_text(encoding='utf-8-sig')
baseline=json.loads((RUN/'baseline.json').read_bytes())
if hashlib.sha256(path.read_bytes()).hexdigest()!=baseline['source_pins'][path.relative_to(ROOT).as_posix()]:
    raise ValueError('Capture must precede source editing')
methods={}
for name in ['RunNormality','RunOutliers','RunSymmetry','RunHomogeneityVar','RunDescStat','ComputeVarianceHomogeneity','ComputeDescriptiveStats']:
    match=re.search(r'    Private (Sub|Function) '+name+r'\(.*?    End \1',source,re.S)
    if not match: raise ValueError(name)
    methods[name]=match.group()
fixture=['Option Explicit On','Option Strict Off','Imports System','Imports System.Collections.Generic','Imports BESHStatNG',
    "' Frozen source projection of main 4a3d2e1, before extraction. No native/scientific reference claim.",
    'Friend NotInheritable Class LegacyAssumption06Characterization']
for name in ['RunNormality','RunOutliers','RunSymmetry']:
    body=methods[name].split("        'descriptive statistics")[0]
    if name=='RunNormality':
        body=body.split('        res.Add(rTable)')[0]+'        Return rTable\n'
        params=''
    elif name=='RunOutliers':
        body=body.split('        res.Add(outlierTable)')[0]+'        Return outlierTable\n'
        params=', grubbs As Boolean, rosner As Boolean, alpha As Decimal'
        body=body.replace('Me.optGrubbs.Checked','grubbs').replace('Me.optRosner.Checked','rosner').replace('Me.spinBtnAlphaOutliers.Value','alpha')
    else:
        body=body.split('        res.Add(asymTable)')[0]+'        Return asymTable\n'
        params=', mgg As Boolean'
        body=body.replace('Me.optMGG.Checked','mgg')
        body='\n'.join(l for l in body.splitlines() if 'plots' not in l and 'ckSymmetryPlot' not in l)+'\n'
    body='\n'.join(l for l in body.splitlines() if 'Dim box As' not in l)+'\n'
    body=re.sub(r'    Private Sub '+name+r'\(data As MultiGroupsUnpairedData\)',
        '    Public Shared Function '+name+'(data As MultiGroupsUnpairedData'+params+') As ResultTable',body)
    fixture += [body+'    End Function']
for name in ['ComputeVarianceHomogeneity','ComputeDescriptiveStats']:
    body=methods[name].replace('Private Function','Public Shared Function')
    if name=='ComputeVarianceHomogeneity':
        body=body.replace('data As MultiGroupsUnpairedData)', 'data As MultiGroupsUnpairedData, fligner As Boolean, levene As Boolean, squared As Boolean, bartlett As Boolean)')
        for control,value in [('ckFlignerKilleen','fligner'),('ckLevene','levene'),('ckSquaredRanks','squared'),('ckBartlett','bartlett')]:
            body=body.replace('Me.'+control+'.Checked',value)
    fixture.append(body)
fixture.append('End Class')
target=ROOT/'BESHStatNG.Test/src/LegacyAssumption06Characterization.vb'
with target.open('x',encoding='utf-8',newline='\n') as out: out.write('\n\n'.join(fixture)+'\n')
directory=ROOT/'docs/architecture/04A06'
directory.mkdir(exist_ok=True)
record=dict(schema='04A06-pre-extraction-source-characterization-v1',base=baseline['head'],source_sha256=hashlib.sha256(path.read_bytes()).hexdigest(),
    fixture_sha256=hashlib.sha256(target.read_bytes()).hexdigest(),provenance='Source projection captured before production edits; NOT native, NOT independent scientific evidence, NOT_GOLDEN',
    methods={name:dict(sha256=hashlib.sha256(text.encode()).hexdigest(),body=text) for name,text in methods.items()},
    ownership={name:('Common report assembly/Core calls; Windows view options, acquisition, plotting and writer remain' if name!='RunDescStat' else 'Windows checkbox snapshot; Common descriptive selection/projection') for name in methods},
    normality_rows=['Shapiro-Wilk test','Test statistics','Two-sided P-value',"D'Agostino-Pearson K2 test",'Test statistics','Two-sided P-value','Anderson Darling test','Test statistics','Two-sided P-value'],
    pvalue_body_rows=[3,6,9],shapiro_shipping_bounds='n > 3 AND n < 5000; inclusive Core documentation mismatch retained',
    mutations='Core sorts some supplied arrays; compatibility adapter must replay scratch ordering before native graphics; original provenance is never reassigned to sorted scratch',
    box='Windows graphics.Calculate/CalcForPlotting; Common maps six-column summary; no chart math migration',native_executed=False)
with (directory/'baseline-v1.json').open('x',encoding='utf-8',newline='\n') as out: json.dump(record,out,indent=2,ensure_ascii=False);out.write('\n')
print('Frozen characterization created:',record['source_sha256'])
