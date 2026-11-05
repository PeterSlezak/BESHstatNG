"""Freeze actual entry sources before 04A.22 changes; no historical writers."""
import hashlib
import importlib.util
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / 'TestResults/04A-22-20261105-run01'
OUT = ROOT / 'evidence/04A22'
BASE = '9330f8df18c0c73d2b233ec4b06dc7199cbb244a'
FORM = 'BESHstatNG/src/UI/Ui21ControlCharts.vb'
RENDERER = 'BESHstatNG/src/Graphics/SpcControlChartExcel.vb'

def sha(p):
    return hashlib.sha256(Path(p).read_bytes()).hexdigest()

def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory='+ROOT.as_posix(), *args], cwd=ROOT, text=True).strip()

def emit(name, value):
    (OUT/name).write_text(json.dumps(value, ensure_ascii=False, indent=2, allow_nan=False)+'\n', encoding='utf-8', newline='\n')

def members(t):
    return {m.group(2): m.group() for m in re.finditer(r'^        (?:Private|Public|Friend) (?:Shared )?(Sub|Function) (\w+)\b.*?^        End \1', t, re.M|re.S)}

def capture():
    assert git('rev-parse', 'HEAD') == git('rev-parse', 'main') == BASE
    assert git('branch', '--show-current') == 'codex/04A-22'
    assert not git('diff', '--name-only') and not git('diff', '--cached', '--name-only')
    assert (RUN/'start.json').is_file()
    paths = git('ls-files').splitlines()
    pins = {p:dict(sha256=sha(ROOT/p), bytes=(ROOT/p).stat().st_size) for p in paths
            if p.endswith(('.vb','.cs','.vbproj','.csproj','.ps1','.py','.cjs'))
            and not p.startswith(('TestResults/','evidence/'))}
    raw = (ROOT/RENDERER).read_text(encoding='utf-8-sig')
    methods = members(raw)
    for p in [FORM, RENDERER]:
        target = RUN/'preimages'/Path(p).name
        target.parent.mkdir(exist_ok=True)
        with target.open('xb') as f: f.write((ROOT/p).read_bytes())
    emit('prechange-method-inventory.json', dict(base=BASE,files=pins,renderer_methods={n:dict(sha256=hashlib.sha256(t.encode()).hexdigest(),first_line=raw[:raw.index(t)].count('\n')+1,body=t) for n,t in methods.items()}))
    frozen = [p for p in paths if p.startswith(('evidence/','docs/architecture/','planning/'))]
    # Include ignored authority documents identified by the accepted crosswalk.
    prior = json.loads((ROOT/'evidence/04A21/original-control-crosswalk.json').read_bytes())
    frozen = sorted(set(frozen)|set(prior['authorities']))
    emit('frozen-capture-hashes.json', dict(files={p:dict(sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in frozen},method='Exact live entry bytes; no ZIP line-ending normalization. All tracked baseline pins retained locally. Historical TestResults membership/metadata separately frozen.'))
    emit('entry-state.json',dict(base=BASE,main=git('rev-parse','main'),origin_main=git('rev-parse','origin/main'),parent=git('rev-parse','HEAD^'),tree=git('rev-parse','HEAD^{tree}'),branch=git('branch','--show-current'),index_empty=True,tracked_clean=True,worktrees=1,acceptance='Owner explicitly accepted 04A.21; actual linear main commit includes shipping policies, consumers and tests. No invented merge commit; no fetch.',shipping_policies={p:pins[p] for p in pins if '/Analysis/Spc' in p},gates=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET'),native_releases=0,deferred_discharges=0,ignored_inventory='LOCAL_ENTRY: initial git status --short --ignored; public paths omitted where historical filenames identify workstation/account.',historical_zip='Previous pre-merge ZIP is context only; entry measurements are current live bytes.'))
    names = ['BuildSharedAxisContext','BuildPanelChartData','AddPointLabels','AddSignalLabels','AddExclusionLabels','FormatRuleNumbers','ChartValue','MissingChartValue','IsFinite','HasValidZones','ComputeYAxisBounds','IncludeValuesInBounds','IncludeNullableInBounds','PanelSupportsSpecificationLimits','HasNaturalZero','IsNumericChartValue']
    bodies = '\n\n'.join(methods[n] for n in names)
    bodies = bodies.replace('Private Shared', 'Public Shared').replace('series As Series', 'series As List(Of Spc22LegacyLabel)')
    classes = raw[raw.index('        Private NotInheritable Class SharedAxisContext'):raw.rindex('    End Class')].replace('Private NotInheritable', 'Public NotInheritable')
    header = '''Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.StatisticalProcessControl

' Frozen source-derived host-free 04A.22 characterization. NOT_GOLDEN.
Public Class Spc22LegacyLabel
    Public ReadOnly Index As Integer
    Public ReadOnly Text As String
    Public Sub New(i As Integer, t As String)
        Index = i : Text = t
    End Sub
End Class
Public Class Spc22LegacyRenderer
    Public Class SpcControlChartAppearanceOptions
        Public Property UseSequenceValuesForHorizontalAxis As Boolean
        Public StatisticColor As Integer
        Public SignalColor As Integer
        Public ExclusionColor As Integer
        Public DataLabelFontSize As Double
        Public ShowSpecificationLimits As Boolean
        Public ShowTargetLine As Boolean
    End Class
    Public Enum XlDataLabelPosition
        xlLabelPositionAbove
        xlLabelPositionBelow
    End Enum
    Private Shared Sub SetPointLabel(series As List(Of Spc22LegacyLabel), i As Integer, text As String, color As Integer, size As Double, position As XlDataLabelPosition)
        If String.IsNullOrWhiteSpace(text) Then Return
        series.Add(New Spc22LegacyLabel(i, text))
    End Sub
'''
    derivative = ROOT/'BESHStat.Excel.Common.Tests/src/Spc22LegacyRenderer.vb'
    with derivative.open('x',encoding='utf-8',newline='\n') as f: f.write(header+bodies+'\n'+classes+'End Class\n')
    emit('legacy-renderer-provenance.json',dict(source=RENDERER,source_sha256=sha(ROOT/RENDERER),derivative=derivative.relative_to(ROOT).as_posix(),derivative_sha256=sha(derivative),methods={n:hashlib.sha256(methods[n].encode()).hexdigest() for n in names},adaptation='Visibility widened; Series replaced by a label collector, styling-only options/enum replaced by inert local types; SetPointLabel collects nonblank content and zero-based index. Shared axis/channel/label loop bodies unchanged. Stage predicate/fraction characterized separately against pinned original body; bounds/zones remain native.',status='NOT_GOLDEN / SAME_CORE_PRESENTATION_COMPATIBILITY_ONLY'))
    print('Frozen entry',len(pins),'sources;',len(frozen),'authorities; legacy derivative generated')

if __name__ == '__main__':
    capture()
