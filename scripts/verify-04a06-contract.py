"""04A.06 successor source/restriction proof. Read-only check; build writes new evidence only."""
import argparse
import csv
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-06-20261102-run01'
OUT='docs/architecture/04A06/source-restriction-crosswalk-v1.json'
UI='BESHstatNG/src/UI/UibyID.vb'
AMENDMENT='docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json'
LEDGER='docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json'
CSV='docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv'
CASE='04A02-NORMALITY-NATIVE-01'
CHANGED={'RunNormality','RunOutliers','RunSymmetry','RunHomogeneityVar','RunDescStat','ComputeVarianceHomogeneity','ComputeDescriptiveStats','GetHomogeneityMethods'}

def sha(name): return hashlib.sha256((ROOT/name).read_bytes()).hexdigest()
def members(source):
    return {m[2]:m[0] for m in re.finditer(r'^    (?:Public|Private|Friend)?\s*(?:(?:Shared|Async|Overrides|Overloads)\s+)*(Sub|Function)\s+(\w+)\b.*?^    End \1',source,re.M|re.S)}
def original():
    return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show','4a3d2e16b4d76a559427d596a424d382bcb0b915:'+UI],cwd=ROOT).decode('utf-8-sig').replace('\r\n','\n')
def current(): return (ROOT/UI).read_text(encoding='utf-8-sig')

def verify_extraction():
    before=members(original());after=members(current())
    assert set(after)-set(before)=={'GetHomogeneityMethods'}
    assert set(before)-set(after)==set()
    assert {n for n in before if before[n]!=after[n]}==CHANGED-{'GetHomogeneityMethods'}
    for name in ['RunNormality','RunOutliers','RunSymmetry','RunHomogeneityVar','RunDescStat']:
        assert before[name].split("        'Dump outputs",1)[1]==after[name].split("        'Dump outputs",1)[1], name+' native tail changed'
        assert 'WindowsAssumptionReportAdapter.Prepare' in after[name]
        assert not re.search(r'assumptions\.|New DescriptiveStat|SetBody\(|AddPvalueCellToFormat|wrapResults\(',after[name])
    for name in ['ComputeVarianceHomogeneity','ComputeDescriptiveStats']:
        assert 'WindowsAssumptionReportAdapter.' in after[name]
    return before,after

def build():
    before,after=verify_extraction()
    assert sha(AMENDMENT)=='8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307'
    assert sha(LEDGER)=='363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10'
    amendment=json.loads((ROOT/AMENDMENT).read_bytes())
    ledger=json.loads((ROOT/LEDGER).read_bytes()); originals={r['obligation_id']:r for r in ledger}
    with (ROOT/CSV).open(encoding='utf-8-sig',newline='') as stream:
        rows={r['control_id']:r for r in csv.DictReader(stream)}
    transfers=amendment['obligation_transfers']; case=[r for r in transfers if CASE in r['case_ids']]
    assert len(case)==160 and sum(r['in_blocked_control_set'] for r in case)==141
    assert len({r['original_control_id'] for r in transfers if r['in_blocked_control_set']})==218
    assert len({r['obligation_id'] for r in transfers})==239 and len(ledger)==1514
    assert len({r['original_id'] for r in ledger if r['affected_live_golden_blocked'] and r['id_kind']=='control'})==802
    controls=set()
    for name in CHANGED-{'GetHomogeneityMethods'}:
        controls.update(re.findall(r'Me\.(\w+)\.(?:Checked|Value)',before[name]))
    controls.update(['btCompute','ckFlignerKilleen','ckLevene','ckSquaredRanks','ckBartlett'])
    qualified=[]
    for r in case:
        assert r['closure_status']=='OPEN' and r['capture_authorized'] is False
        frozen=originals[r['obligation_id']]
        assert r['source_record']['row_sha256']==frozen['source_record_sha256']
        row=rows[r['original_control_id']]
        assert hashlib.sha256(json.dumps(row,sort_keys=True,ensure_ascii=False).encode()).hexdigest()==r['source_record']['row_sha256']
        if r['original_control_id'].rsplit('.',1)[1] not in controls: continue
        qualified.append(dict(obligation_id=r['obligation_id'],case_ids=r['case_ids'],blocked=r['in_blocked_control_set'],risk_tiers=r['risk_tiers'],
            original_row_sha256=frozen['source_record_sha256'],original_codebehind_sha256=r['source_evidence']['codebehind_source_sha256'],
            original_designer_sha256=r['source_evidence']['designer_source_sha256'],source_record=r['source_record'],pre_change_gate=r['pre_change_gate'],
            status='OPEN',owner_released=False,native_executed=False,
            disposition='Behavior-preserving report extraction; source/managed characterization only. Native pre-change gate retained for ANY behavior-changing successor. No timing exception inferred.',
            native_missing=['saved/rendered normality and NA/warnings','descriptive/box table and chart geometry','owned PID/HWND and loaded XLL/assembly attribution','independent Windows/COM risk review and exact N authority']))
    assert qualified
    nonledger=sorted(controls-{r['original_control_id'].rsplit('.',1)[1] for r in case})
    assert not nonledger,'Do not invent mappings: '+str(nonledger)
    geometry=json.loads((ROOT/'docs/architecture/04A27/geometry-owner-disposition-2026-11-02.json').read_bytes())
    assert len(geometry['original_44_obligations'])==44
    assert all(r['disposition']=='OPEN' and not r['owner_released'] and not r['native_executed'] for r in geometry['original_44_obligations'])
    pins=json.loads((RUN/'baseline.json').read_bytes())['source_pins']
    unchanged=[p for p in pins if p.startswith(('BESHStat.Core/','planning/','docs/')) or '/ExcelUDFs/' in p or p.endswith('.Designer.vb') or '/UI/Refedit/' in p or p=='BESHstatNG/src/DM/ExcelDnaDataImporter.vb']
    assert all(sha(p)==pins[p] for p in unchanged),'Protected source/authority changed'
    fixture='BESHStatNG.Test/src/LegacyAssumption06Characterization.vb'
    characterization=json.loads((ROOT/'docs/architecture/04A06/baseline-v1.json').read_bytes())
    assert sha(fixture)==characterization['fixture_sha256']
    sources=['BESHStat.Excel.Common/src/Results/AssumptionReportPolicy.vb','BESHstatNG/src/DM/WindowsAssumptionReportAdapter.vb',UI,
        'BESHStat.Excel.Common.Tests/src/AssumptionReport06Tests.vb','BESHStatNG.Test/src/AssumptionReport06AdapterTests.vb',fixture]
    return dict(schema='04A06-report-extraction-restrictions-v1',base='4a3d2e16b4d76a559427d596a424d382bcb0b915',
        authorities={p:sha(p) for p in [AMENDMENT,LEDGER,CSV,'docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json','docs/architecture/04A03/pre-change-crosswalk.json','docs/architecture/04A04/compatibility-crosswalk.json','docs/architecture/04A05/owner-completion-dispositions-v1.json','docs/architecture/04A27/geometry-owner-disposition-2026-11-02.json']},
        source_pins={p:sha(p) for p in sources},before_source_sha256=pins[UI],
        methods={n:dict(before_sha256=hashlib.sha256(before[n].encode()).hexdigest() if n in before else None,after_sha256=hashlib.sha256(after[n].encode()).hexdigest()) for n in sorted(CHANGED)},
        all_other_methods_identical=True,native_tails_identical=True,designer_and_lifecycle_unchanged=True,
        populations=dict(shared_blocked=141,shared_obligations=160,transferred_blocked=218,transferred_obligations=239,restricted_live=802,original_obligations=1514,importer_timing_obligations=44),
        affected=qualified,all_shared_open_ids=[r['obligation_id'] for r in case],
        released_controls=0,discharged_obligations=0,native_executed=False,P='NO',N1='NO GO',N2='NO',C2='UNMET',
        gate_disposition='Only behavior-preserving production extraction applied; host-free report/metadata/mutation parity. No native validation or behavioral timing exception.',
        scientific_candidates=['SC-06-SHAPIRO-BOUNDS: shipping n>3,n<5000 vs Core documentation inclusive; unchanged',
            'SC-06-DAGOSTINO-OVERFLOW: integer denominator (n-2)*(n+5)*(n+7)*(n+9); 4999/5000/5001 legacy and Core OverflowException; unchanged',
            'SC-06-CONSTANT-SHAPIRO: Core returns Nothing; old UI dereferences; structured failure plus legacy exception retained',
            'SC-06-EMPTY-HOMOGENEITY: no selected method/supplement yields no tables; unchanged native writer tail still runs. Any no-write UI correction needs exact-case owner disposition.'],
        provenance='Original input mappings remain immutable; legacy grouped object has no RowIds, so Windows report bridge asserts none. Core sorting scratch is never relabeled with source rows.')

def check():
    expected=json.loads((ROOT/OUT).read_bytes())
    assert build()==expected,'Versioned successor differs'
    assert expected['released_controls']==expected['discharged_obligations']==0
    assert all(r['status']=='OPEN' and not r['owner_released'] and not r['native_executed'] for r in expected['affected'])
    return expected

if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('action',choices=['build','check']);args=parser.parse_args()
    if args.action=='build':
        result=build() # validate completely before creating the successor
        target=ROOT/OUT
        if target.exists() and target.stat().st_size: raise ValueError('Successor already sealed')
        with target.open('w' if target.exists() else 'x',encoding='utf-8',newline='\n') as stream: json.dump(result,stream,indent=2,ensure_ascii=False);stream.write('\n')
    else: check()
    print('04A.06 successor source/restriction proof:',args.action,'OK')
