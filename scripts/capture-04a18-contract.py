"""04A.18 prechange/layout oracle and source/evidence preservation. No native execution."""
import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-18-20261104-run01'
OUT=ROOT/'evidence/04A18'
BASE='f539d4e31779a2083d4c135e2784314c0083cff4'
FORMS=['BESHstatNG/src/UI/Ui11PCA.vb','BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb']
ADAPTER='BESHstatNG/src/DM/WindowsMultivariateWorkflowAdapter.vb'
PAGES=['BESHStatNG_Help_MkDocs/docs/methods/'+s+'.md' for s in ['principal-component-analysis','correspondence-analysis','multiple-correspondence-analysis']]
CHANGED=set(FORMS+[ADAPTER,'BESHStatNG.Test/BESHStatNG.Tests.vbproj']+PAGES)
s=importlib.util.spec_from_file_location('contract17',ROOT/'scripts/capture-04a17-contract.py')
prior=importlib.util.module_from_spec(s);s.loader.exec_module(prior)


def sha(path):return hashlib.sha256(Path(path).read_bytes()).hexdigest()
def git(*args):return prior.git(*args)
def original(path):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+path],cwd=ROOT).decode('utf-8-sig').replace('\r\n','\n')
def members(path,text):return prior.members(path,text)
def write(name,value):(OUT/name).write_text(json.dumps(prior.prior.sanitize(value),indent=2,ensure_ascii=False,allow_nan=False)+'\n',encoding='utf-8',newline='\n')


def audit():
    assert git('branch','--show-current')=='codex/04A-18'
    assert git('rev-parse','HEAD')==git('rev-parse','main')==BASE
    assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
    paths=FORMS+[ADAPTER,'BESHStat.Excel.Common/src/Analysis/MultivariateWorkflowPolicy.vb','BESHStat.Excel.Common/src/Results/MultivariateReportPolicy.vb',
        'BESHStat.Core/src/RegModels/Multivariate/PCA.vb','BESHStat.Core/src/RegModels/Multivariate/CA.vb',
        'BESHstatNG/src/Graphics/PcaPlotExcel.vb','BESHstatNG/src/Graphics/CorrespondenceAnalysisPlotExcel.vb',
        'BESHStat.Core/src/DM/ResultWriter/ResultTableOutput.vb','BESHstatNG/src/DM/ResultWriter/ExcelDnaResultWriter.vb',
        'BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMultivariate.vb',
        'BESHstatNG/src/UI/Ui11PCA.Designer.vb','BESHstatNG/src/UI/Ui0OneRefeditMulticol.Designer.vb',
        'BESHStatNG_Help_MkDocs/docs/_meta/methods.yml']+PAGES
    inventory={}
    for p in paths:
        b=(ROOT/p).read_bytes();t=b.decode('utf-8-sig').replace('\r\n','\n')
        methods=members(p,t) if p.endswith('.vb') and not p.endswith('.Designer.vb') else {}
        inventory[p]=dict(sha256=sha(ROOT/p),bytes=len(b),encoding='UTF-8 BOM' if b.startswith(b'\xef\xbb\xbf') else 'UTF-8',crlf=b.count(b'\r\n'),bare_lf=b.count(b'\n')-b.count(b'\r\n'),final_newline=b.endswith(b'\n'),methods={n:dict(sha256=hashlib.sha256(body.encode()).hexdigest(),first_line=t[:t.index(body)].count('\n')+1) for n,body in methods.items()})
    write('prechange-method-inventory.json',dict(base=BASE,files=inventory))
    ui=members(FORMS[0],original(FORMS[0]))
    pca=ui['RunPCA'];pca=pca[pca.index('        Dim metadataColumnCount'):pca.index("        'PCA numerical results")]
    mca=ui['RunMCA'];mca=mca[mca.index('        WriteRes.write({"Row ID"})'):mca.index("        'MCA numerical results")]
    oracle='''Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG
Imports BESHStatNG.Multivariate

' Frozen prechange Data-sheet write statements; only method/parameter boundaries added.
Public NotInheritable Class LegacyMultivariate18Layout
    Public Shared Sub PcaData(WriteRes As ResultTableWriterBase, MyData As DataObj, objPCA As PCA, rowLabels() As String, groupLabels() As String)
'''+pca+'''    End Sub
    Public Shared Sub McaData(WriteRes As ResultTableWriterBase, MyData As DataObj, mca As CA)
'''+mca+'''    End Sub
    Public Shared Function LabelledCounts(data As DataObj) As Integer(,)
        Dim TableData(data.nRows - 1, data.nCols - 2) As Integer
        For i = 0 To UBound(data.FinalData, 1)
            For j = 1 To UBound(data.FinalData, 2)
                TableData(i, j - 1) = CInt(data.FinalData(i, j))
            Next
        Next
        Return TableData
    End Function
    Public Shared Function LegacyUnlabelledCounts(data As DataObj) As Integer(,)
        Dim TableData(data.nRows - 1, data.nCols - 1) As Integer
        For i = 0 To UBound(data.FinalData, 1)
            For j = 0 To UBound(data.FinalData, 2)
                TableData(i, j - 1) = CInt(data.FinalData(i, j))
            Next
        Next
        Return TableData
    End Function
End Class
'''
    path='BESHStat.Excel.Common.Tests/src/LegacyMultivariate18Layout.vb'
    assert not (ROOT/path).exists();(ROOT/path).write_text(oracle,encoding='utf-8',newline='\n')
    write('legacy-layout-provenance.json',dict(base=BASE,source_methods={FORMS[0]:{n:inventory[FORMS[0]]['methods'][n]['sha256'] for n in ['RunPCA','RunMCA']},FORMS[1]:{'RunCA':inventory[FORMS[1]]['methods']['RunCA']['sha256']}},derivative_path=path,derivative_sha256=sha(ROOT/path),interpretation='Actual prechange pointer/write sequences copied verbatim into host-free methods. CA conversion/reproducer retain exact original bounds/CInt/target indices; original host acquisition remains outside this test oracle. NOT_GOLDEN.'))
    write('implementation-plan.json',dict(scope='04A.18 only; before production edits',existing_files=sorted(CHANGED),methods={FORMS[0]:['RunPCA','RunMCA'],FORMS[1]:['RunCA'],ADAPTER:['PCA/MCA rendering delegates and narrow new layout/labelled-CA bridges']},
        common='Detached semantic axes/shape/section metadata, PCA/MCA synchronous borrowed payload traversal, labelled contingency column mapping and original result prefix; no eigensolver/encoding/geometry',
        gating='Unlabelled CA loop and exception remain unchanged. Correct j-to-j mapping is dormant with a separately pinned unapplied patch. No candidate diagnostic becomes a host rejection.',
        original_layout=dict(PCA='ID row1/body2; optional group col2; original, standardized and reduced blocks offset by input P; variable P separate from score K',MCA='IDs/raw observations start row3; header row2 blank beside raw block; design caption colQ+2; variable/category headers row1/2 colQ+3; indicator body row3',CA='Original Analyzed contingency table prefix; wrapResults order; overlap prompt and five plots at existing destination'),
        tolerance='max(1e-9, abs(expected)*1e-9) finite doubles; exact CLR types, nonfinite classes, labels, metadata, block positions and sequence. Fixed before comparisons; no sign/subspace canonicalization.'))
    print('Frozen',len(inventory),'source inventories and original writer oracle')


def source():
    assert git('branch','--show-current')=='codex/04A-18'
    assert git('rev-parse','HEAD')==git('rev-parse','main')==BASE
    assert not git('diff','--cached','--name-only')
    changed=set(git('diff','--name-only').splitlines());assert changed<=CHANGED,changed-CHANGED
    pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    for p,h in pins.items():
        if p not in changed:assert sha(ROOT/p)==h,p
    maps={}
    for p in FORMS+[ADAPTER]:
        old=members(p,original(p));new=members(p,(ROOT/p).read_text(encoding='utf-8-sig').replace('\r\n','\n'))
        deltas={n:dict(before=hashlib.sha256(t.encode()).hexdigest(),after=hashlib.sha256(new[n].encode()).hexdigest()) for n,t in old.items() if t!=new[n]}
        maps[p]=dict(changed=deltas,unchanged=sorted(set(old)-set(deltas)),added=sorted(set(new)-set(old)))
    assert set(maps[FORMS[0]]['changed'])=={'RunPCA','RunMCA'}
    assert set(maps[FORMS[1]]['changed'])=={'RunCA'}
    old=members(FORMS[1],original(FORMS[1]))['RunCA'];new=members(FORMS[1],(ROOT/FORMS[1]).read_text(encoding='utf-8-sig'))['RunCA']
    start='        Else\n            ReDim TableData';end='        End If'
    legacy=old[old.index(start):old.index(end,old.index(start))];current=new[new.index(start):new.index(end,new.index(start))]
    assert legacy==current,'Unlabelled CA defect branch must be unchanged'
    provenance=json.loads((OUT/'legacy-layout-provenance.json').read_bytes());assert sha(ROOT/provenance['derivative_path'])==provenance['derivative_sha256']
    write('source-preservation.json',dict(base=BASE,protected_initial_tracked_count=len(pins)-len(changed),private_initial_map=dict(alias='LOCAL_INITIAL:source-map',sha256=sha(RUN/'baseline.json'),bytes=(RUN/'baseline.json').stat().st_size),changed={p:dict(before=pins[p],after=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in sorted(changed)},new_sources={p:dict(sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in git('ls-files','--others','--exclude-standard').splitlines() if p.endswith(('.vb','.py','.patch'))},methods=maps,unlabelled_branch_sha256=hashlib.sha256(legacy.encode()).hexdigest(),index_empty=True))
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
    for p,pin in registry['files'].items():assert sha(ROOT/p)==pin['sha256'] and (ROOT/p).stat().st_size==pin['bytes'],p
    retained={p:dict(sha256=h,bytes=(ROOT/p).stat().st_size) for p,h in pins.items() if p.startswith(('evidence/04A14/','evidence/04A15/','evidence/04A16/','evidence/04A17/'))}
    write('frozen-capture-hashes.json',dict(status='NOT_GOLDEN',captures=registry['files'],prior_evidence=retained,multivariate17_records=1224,mixed_lineage='177 / 69 OPEN / 67 restricted',P='NO',N1='NO GO',N2='NO',C2='UNMET'))
    write('documentation-audit.json',{p:dict(sha256=sha(ROOT/p),disposition='Same-batch axis/layout/availability clarification; existing algorithms/screenshots unchanged') for p in PAGES})
    print('PASS protected initial sources/evidence; exact PCA/MCA/CA method scope; unlabelled branch unchanged')


if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('action',choices=['audit','source']);globals()[p.parse_args().action]()
