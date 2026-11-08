"""Assemble review metadata from completed receipts; never edits predecessors or captures."""
from pathlib import Path
from collections import Counter
import csv
import hashlib
import json
import subprocess
import sys
from preserve import SOURCES

ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
RUN=ROOT/'.test-rationalization-04A37.tmp'


def load(name):return json.loads((HERE/name).read_bytes())
def sha(data):return hashlib.sha256(data).hexdigest()
def pin(path):
    data=(ROOT/path).read_bytes();return dict(path=path,bytes=len(data),sha256=sha(data))
def save(name,value):(HERE/name).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8',newline='\n')


def main():
    validation=load('validation.json');source=load('source-and-node-validation.json');nonpasses=load('nonpasses.json')
    allowed={"(path='BESHstatNG/src/Graphics/"+n+"PlotExcel.vb')" for n in ['Pca','FactorAnalysis','ClusterAnalysis','CorrespondenceAnalysis']}
    assert len(source['new_nonpasses'])==5
    for row in source['new_nonpasses']:
        if row['outcome']=='FAIL' and row['subtest'] in allowed and row['identity'].startswith('test_core_numerics_udfs_designers_importer_and_canonical_spc_preserved '):
            row['classification']='QUALIFIED_AUTHORIZED_04A37_WINDOWS_ADAPTER_WHOLE_FILE_PIN_TRANSITION'
            row['successor_checks']='lineage-check.json; member-ownership.csv; renderer-preservation.json; exact frozen captures; compiled Graphics37 adapter tests'
        elif row['outcome']=='ERROR' and row['identity'].startswith('test_all_retained_native_methods_and_appearance_remain_exact '):
            row['classification']='INHERITED_UNORDERED_SPC_LOOKUP_FAILURE_ERROR_VARIANT'
            row['successor_checks']='SPC source byte-identical to entry; historical unordered method set chooses missing AddReferenceLine versus an earlier changed-body assertion. Same identity remains nonpassing.'
        else:raise AssertionError('Unqualified source nonpass')
    source['unclassified']=[];save('source-and-node-validation.json',source)
    nonpasses['python_new']=source['new_nonpasses'];save('nonpasses.json',nonpasses)
    validation['unclassified_python']=0;validation['status']='EXECUTED_WITH_INHERITED_AND_QUALIFIED_SOURCE_NONPASSES';save('validation.json',validation)
    comparisons=[]
    for config in ['Debug','Release']:
        for family,pre,post,count in [('plot-prefixes','preedit/capture-'+config+'.tsv','postedit/capture-'+config+'.tsv',5376),('dendrogram-series','preedit/series.tsv','postedit/series-'+config+'.tsv',1536)]:
            a=pin('evidence/04A37/'+pre);b=pin('evidence/04A37/'+post);assert a['sha256']==b['sha256'] and a['bytes']==b['bytes']
            comparisons.append(dict(configuration=config,family=family,rows=count,pre=a,post=b,mismatches=0))
    with (HERE/'preedit/capture-Debug.tsv').open(encoding='utf-8',newline='') as f:rows=list(csv.DictReader(f,delimiter='\t'))
    save('compatibility-comparison.json',dict(status='PASS_EXACT_SOFTWARE_COMPATIBILITY',rows_per_configuration=6912,comparisons=comparisons,
        raw_operation_counts=dict(Counter(r['operation'] for r in rows)),raw_rejections=sum(r['error']!='None' for r in rows),
        family_accounting=dict(PCA_including_grouping=1792,Factor=768,CA_MCA=2048,Dendrogram_including_supplement=2048,Shared_caption_expressions=256),
        comparison='Exact file bytes, IEEE Double bits/signed zero/NaN payloads, array element types/ranks/bounds, strings/culture/order, exception type/parameter/message and recorded source phase. No tolerance or culture normalization.',
        original_configurations='Main capture executed in entry Debug and Release. Supplement1536 originally executed against entry Debug, reused unchanged for both post configurations; no assertion of a separate pre-edit Release supplement run.',
        limits='Executed pure source slices/expressions, including malformed or omitted-MCA-row slice calls unreachable after some original guards. Native operation invariants are source/compiled-call proofs, not COM execution or visual equivalence.',scientific='NOT_GOLDEN'))
    builds=[]
    for config in ['Debug','Release']:
        b=json.loads((RUN/f'builds/final/{config}/build.json').read_bytes());m=json.loads((RUN/f'final-{config}-managed.json').read_bytes())
        expected=set(SOURCES)-{'docs/architecture/batch-04A-37-owner-review.md'}
        pins=[dict(path=p,sha256=b['source_pins'][p]) for p in sorted(expected)]
        assert all(pin(p['path'])['sha256']==p['sha256'] for p in pins)
        assemblies=[]
        for name in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll','BESHStatNG.dll','BESHStatNG.Test.dll']:
            p=RUN/f'builds/final/{config}/legacy'/name
            if p.is_file():assemblies.append(dict(name=name,bytes=p.stat().st_size,sha256=sha(p.read_bytes())))
        builds.append(dict(configuration=config,build_source_unchanged=b['source_unchanged'],managed_source_unchanged=m['source_unchanged'],build_exits=b['exit_codes'],approved_source_files=pins,assemblies=assemblies))
    save('build-source-pins.json',dict(configurations=builds,native='Assemblies compiled and managed seams tested; no live Excel load or native execution. Binaries excluded from proposed publication.'))
    save('deferred-issues.json',dict(status='REGISTERED_NOT_CORRECTED',charter='planning/BESHStatNG_CrossPlatform_MasterPlan.md section5; approved04A39-48 investigations, not part37',issues=[
        dict(id='G37-PCA-BIPLOT-LEGACY-01',candidate='Original sqrt(eigenvalue)*sqrt(n) exponent, score/loadings orientation and PercentExpl/threshold behavior; NaN c passes legacy range predicate.',disposition='PRESERVED_NOT_GOLDEN; scientific investigation39-41'),
        dict(id='G37-PCA-GROUP-IDENTITY-01',candidate='Ordinal distinct group names but culture/text membership can overlap A/a and width variants; actual Core RowIds fallback and model order preserved.',disposition='PRESERVED; no source-row or grouping cleanup'),
        dict(id='G37-FACTOR-PERCENT-01',candidate='Pattern*structure column sum divided by WorkingMatrix trace, <=0 yields zero; extraction/rotation/sign/format semantics remain authoritative.',disposition='PRESERVED; no factor estimator/rotation correction'),
        dict(id='G37-CA-PERCENT-SYMMETRY-01',candidate='Percents(2,1)/(1,1), late caught short-percent access, MCA omitted rows still in axes; row/column label asymmetries.',disposition='PRESERVED; original CA/MCA engine and labels unchanged'),
        dict(id='G37-DENDROGRAM-BOUNDS-01',candidate='NaN/separator bounds, degenerate/one-element cut arrays influence bounds even when cut not drawn, source leaf order versus physical rows.',disposition='PRESERVED; base paths/cuts/membership untouched'),
        dict(id='G37-NATIVE-GUI-UDF-01',candidate='COM creation/errors/events/reentrancy, measured native labels/axes and GUI/UDF reconciliation require later authorized native/scientific work.',disposition='NOT_RUN; no original controls discharged'),
        dict(id='G29-HISTORICAL-LINEAGE-171-172',candidate='Retain earlier graphics-audit helper uncertainties and 04A33/34/35/36 native candidates.',disposition='UNCHANGED; this extraction closes no original scientific/native obligation')],gates=load('entry-authority.json')['gates']))
    save('not-run.json',dict(native='NOT_RUN',operations=['live Excel/COM/UIA/RefEdit','GUI automated or manual Windows chart execution','R oracle/scientific-golden adjudication','Office/Mac/WASM graphics UI','UDF/ribbon/help catalog changes','installer/packaging/signing/deployment','original shared-output verify wrappers'],reason='Native and release work require separate owner authorization. Original wrappers share historical outputs and Legacy Build includes packaging hooks; accepted isolated offline compile-only runner executed equivalent managed scopes.',gates=load('entry-authority.json')['gates']))
    save('tooling-corrections.json',dict(python=sys.version.split()[0],dotnet=subprocess.check_output(['dotnet','--version'],text=True).strip(),node=subprocess.check_output(['node','--version'],text=True).strip(),
        corrections=['Git dubious ownership handled with command-scoped safe.directory; no global configuration changed.',
            'Requested branch creation needed sandbox escalation because .git was read-only; approved Git switch only. No permissions changed.',
            'Windows Store Python alias was unavailable; existing PythonExe environment binding used, no install.',
            'Entry path-label inventory initially exposed inherited private labels; original retained only privately,70 labels redacted before corrected baseline and production.',
            'Reserved VB identifier / Friend helper binding corrected before first successful frozen capture; invalid sources retained.',
            'Project-link insertion error, same-name VB function parameter and new test compilation issues corrected; failed private command receipts retained.',
            'Explicit UTF-8 fixes affected post-capture/test bindings only; first bad post corpus retained. Frozen valid captures never rewritten.'],
        package_network='No package install, external restore, installer or permissions change. New Roslyn helper restore used local SDK with local-only RestoreSources.'))
    save('readiness.json',dict(status='NOT_READY_04A37',blockers=['INDEPENDENT_PACKET_AUDITS_PENDING'],in_scope_open=0,manifest_verified_required=True,gates=load('entry-authority.json')['gates'],scope='Software portability/compatibility only; native/scientific controls unchanged',next_batch='04A38_NOT_STARTED'))
    save('audit-results.json',dict(status='PENDING',total=0,passed=0,failed=0,errors=0))
    print('Assembled completed matrix, exact6912/config comparisons, source-bound build pins and honest source nonpasses; packet approval still pending')


if __name__=='__main__':main()
