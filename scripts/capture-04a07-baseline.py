"""Pre-edit source characterization for 04A.07; NOT native or independent numerical evidence."""
import hashlib
import json
from pathlib import Path
import re

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-07-20261102-run01'
baseline=json.loads((RUN/'baseline.json').read_bytes())
paths={'grouped':'BESHstatNG/src/UI/UibyID.vb','repeated':'BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb','nested':'BESHstatNG/src/UI/Ui9ANOVA2nested.vb'}
sources={k:(ROOT/p).read_text(encoding='utf-8-sig') for k,p in paths.items()}
for p in paths.values():
    assert hashlib.sha256((ROOT/p).read_bytes()).hexdigest()==baseline['source_pins'][p]
def member(text,name):
    return re.search(r'^    Private (Sub|Function) '+name+r'\b.*?^    End \1',text,re.M|re.S)[0]
members={n:member(sources[k],n) for k,n in [('grouped','RunOneWayANOVAThroughSharedLayer'),('grouped','RunKruskallWalis'),('repeated','Run1WayRmANOVA'),('repeated','ComputeDescriptiveStatistics'),('nested','Run2WayNested')]}
fixture=['Option Explicit On','Option Strict Off','Imports System','Imports System.Collections.Generic','Imports BESHStatNG',
    "' Frozen main b984e20 source projection; no native or independent scientific claim.", 'Friend NotInheritable Class LegacyAnovaWorkflow07']
body=members['Run1WayRmANOVA'].split("        'box plot if requested")[0]
body=body.replace('Private Sub Run1WayRmANOVA(data As MultiGroupsPairedData)',
    'Public Shared Function Repeated(data As MultiGroupsPairedData, alpha As Double, gg As Boolean, hf As Boolean, tukey As Boolean, mauchly As Boolean, descriptive As Boolean) As List(Of ResultTable)')
body='\n'.join(l for l in body.splitlines() if 'Dim alpha As Double' not in l and 'Dim box As' not in l)
for control,value in [('ckGreenhouse','gg'),('ckHuyhn','hf'),('ckTukey','tukey'),('ckMauchly','mauchly'),('ckDescriptiveStatistics','descriptive')]:body=body.replace('Me.'+control+'.Checked',value)
fixture.append(body+'\n        Return res\n    End Function')
fixture.append(members['ComputeDescriptiveStatistics'].replace('Private Function','Public Shared Function'))
body=members['Run2WayNested'].split('        Dim rr =')[0]
body=body.replace('Private Sub Run2WayNested(d As MultiGroupsPairedDataObj)',
    'Public Shared Function Nested(d As MultiGroupsPairedDataObj) As Tuple(Of List(Of ResultTable), Boolean)')
body='\n'.join(l for l in body.splitlines() if 'Dim WriteRes' not in l)
fixture.append(body+'\n        Return Tuple.Create(res, nest.balancedDesign)\n    End Function')
fixture.append('End Class')
target=ROOT/'BESHStatNG.Test/src/LegacyAnovaWorkflow07.vb'
with target.open('x',encoding='utf-8',newline='\n') as f:f.write('\n\n'.join(fixture)+'\n')
out=ROOT/'docs/architecture/04A07';out.mkdir(exist_ok=True)
record=dict(schema='04A07-source-baseline-v1',base=baseline['head'],source_pins={p:baseline['source_pins'][p] for p in paths.values()},
    methods={n:dict(sha256=hashlib.sha256(b.encode()).hexdigest(),body=b) for n,b in members.items()},fixture_sha256=hashlib.sha256(target.read_bytes()).hexdigest(),
    provenance='Source-derived characterization before production edits; NOT native; NOT independent scientific evidence; NOT_GOLDEN',
    ownership={'RunOneWayANOVAThroughSharedLayer':'Existing SharedAnalysisApplicationService retained; Common plan owns homogeneity/descriptive/box order and plot ordering; native output closure unchanged',
        'RunKruskallWalis':'Existing shared KW/Dunn retained; Common plan preserves descriptive BEFORE homogeneity, then box; legacy Dunn always true',
        'Run1WayRmANOVA':'Common calls compute/GG/HF/TukeyKramerRM2/Tukey/wrap/Mauchly in original order; descriptive via accepted C06; native writer/chart remain',
        'Run2WayNested':'Common owns model and balanced/banner/separator metadata; existing native writer placement stays Windows',
        'WindowsPairedInputAdapter':'Add weak provenance sidecars to existing accepted parser results; no acquisition/missing/filtering/return-shape change'},
    defaults=dict(alpha='AppGlobals.DefaultAlpha (Core AnalysisDefaultsOptions setter normalizes invalid global values)',Dunn=True,blank_separator_rows=1),
    known_ambiguities=['Nested Core computes subgroup means by global subgroup label; same label under different parents is not automatically equivalent to a unique subgroup ID. Existing parser supports hierarchical pairs; do not silently fix Core.',
        'Both existing repeated Tukey routines run when the one Tukey checkbox is selected; keep both tables and their distinct sphericity assumptions.'],
    native_executed=False)
with (out/'baseline-v1.json').open('x',encoding='utf-8',newline='\n') as f:json.dump(record,f,indent=2,ensure_ascii=False);f.write('\n')
print('Frozen 04A.07 source/ownership and test fixture captured')
