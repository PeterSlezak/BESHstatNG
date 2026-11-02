"""04A.07 additive source/native-obligation successor. Never edits old ledgers/checkers."""
import argparse
import csv
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT=Path(__file__).resolve().parents[1]
RUN=ROOT/'TestResults/04A-07-20261102-run01'
BASE='b984e20bd5a8112a4f22ade94df8851cca3a61e8'
OUT=ROOT/'docs/architecture/04A07/source-restriction-crosswalk-v2.json'
AMENDMENT='docs/architecture/04A02/04A02-final-amended-acceptance-PROPOSED.json'
LEDGER='docs/architecture/WindowsMigrationLedger/04A01-approved-deferred-obligations.json'
TRIAGE='docs/architecture/WindowsMigrationLedger/continuation28-control-triage.csv'
CONTROLS='docs/architecture/WindowsMigrationLedger/ui-controls.csv'
FORMS={
 'BESHstatNG/src/UI/UibyID.vb':['RunOneWayANOVAThroughSharedLayer','RunKruskallWalis'],
 'BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb':['Run1WayRmANOVA'],
 'BESHstatNG/src/UI/Ui9ANOVA2nested.vb':['Run2WayNested']}
def sha(path):return hashlib.sha256((ROOT/path).read_bytes()).hexdigest()
def rowsha(row):return hashlib.sha256(json.dumps(row,sort_keys=True,ensure_ascii=False).encode()).hexdigest()
def original(path):
    return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+path],cwd=ROOT).decode('utf-8-sig').replace('\r\n','\n')
def members(source):
    return {m[2]:m[0] for m in re.finditer(r'^    (?:Public|Private|Friend)?\s*(?:(?:Shared|Async|Overrides|Overloads)\s+)*(Sub|Function)\s+(\w+)\b.*?^    End \1',source,re.M|re.S)}
def verify_sources():
    result={}
    for path,changed in FORMS.items():
        before=original(path);after=(ROOT/path).read_text(encoding='utf-8-sig')
        old=members(before);new=members(after)
        assert set(old)==set(new)
        assert {n for n in old if old[n]!=new[n]}==set(changed)
        masked_before=before;masked_after=after
        for name in changed:
            masked_before=masked_before.replace(old[name],'<'+name+'>')
            masked_after=masked_after.replace(new[name],'<'+name+'>')
            tail='        If Not RunSharedOutput(' if path.endswith('UibyID.vb') else "        'Dump outputs"
            old_tail=old[name].split(tail,1)[1];new_tail=new[name].split(tail,1)[1]
            if name=='Run2WayNested':
                new_tail=new_tail.replace('WriteRes.write(prepared.Banner)','WriteRes.write({{"Two-Way Nested ANOVA"}, {$"Balanced design = {nest.balancedDesign}"}})')
                new_tail=new_tail.replace('        rr.SetSeparators(prepared.Separators)',
                    '        Dim sep As New List(Of Object(,))\n        sep.Add({{"", ""}, {"Satterthwaite approximation", ""}})\n        rr.SetSeparators(sep)')
            assert old_tail==new_tail,name+' native output/chart tail changed'
            assert 'WindowsAnovaWorkflowAdapter.' in new[name]
            assert not re.search(r'New parametric\.|assumptions\.|\.compute\(|\.wrapResults\(|\.GreenhouseGeisser\(|\.HuyhnFeldt\(|\.TukeyKramerRM2\(|SetBody\(',new[name])
        assert masked_before==masked_after,'Changes outside exact report members: '+path
        result[path]=dict(before=old,after=new)
    return result

def build():
    sources=verify_sources();baseline=json.loads((RUN/'baseline.json').read_bytes())
    assert sha(AMENDMENT)=='8ba5614757a20cd9ca7d075529c058072a250e4ae97fc00d66228c9703e22307'
    assert sha(LEDGER)=='363ae57510b613eef06faebba7bc7e4152b928e4d3bbad7c1f67eea589c56d10'
    amendment=json.loads((ROOT/AMENDMENT).read_bytes());ledger=json.loads((ROOT/LEDGER).read_bytes())
    original_rows={r['obligation_id']:r for r in ledger};transfers={r['obligation_id']:r for r in amendment['obligation_transfers']}
    with (ROOT/TRIAGE).open(encoding='utf-8-sig',newline='') as stream:triage={r['control_id']:r for r in csv.DictReader(stream)}
    with (ROOT/CONTROLS).open(encoding='utf-8-sig',newline='') as stream:controls={r['control_id']:r for r in csv.DictReader(stream)}
    assert len(ledger)==1514 and sum(r['id_kind']=='control' and r['affected_live_golden_blocked'] for r in ledger)==802
    assert len(transfers)==239 and sum(r['in_blocked_control_set'] for r in transfers.values())==218
    anova=[r for r in transfers.values() if '04A02-ANOVA-NATIVE-01' in r['case_ids']]
    assert len(anova)==160 and sum(r['in_blocked_control_set'] for r in anova)==141
    assert {r['obligation_id'] for r in anova}=={r['obligation_id'] for r in transfers.values() if '04A02-NORMALITY-NATIVE-01' in r['case_ids']}
    assert all(r['closure_status']=='OPEN' and not r['capture_authorized'] for r in transfers.values())
    affected={}
    for path,names in FORMS.items():
        cls=Path(path).stem;selected=set()
        for name in names:selected.update(re.findall(r'Me\.(\w+)\.(?:Checked|Value)',sources[path]['before'][name]))
        selected.add('btCompute')
        if cls=='UibyID':selected.update(['ckBartlett','ckFlignerKilleen','ckLevene','ckSquaredRanks','RefEdit1','RefEdit2','RefEditOutput'])
        elif cls=='Ui0OneRefeditMulticol':selected.update(['RefEdit1','RefEditOutput'])
        else:selected.update(['RefEdit1_Group','RefEdit2_nested','RefEdit3_Data','RefEditOutput'])
        for name in sorted(selected):
            control='BESHStatNG.'+cls+'.'+name
            assert control in controls,'Do not invent source control ID: '+control
            row=controls[control];deferred=original_rows.get('control:'+control);transfer=transfers.get('control:'+control)
            item=dict(control_id=control,source_record=CONTROLS,source_record_file_sha256=sha(CONTROLS),source_row_sha256=rowsha(row),declaration_path=row['path'],declaration_line=row['line'],
                obligation_id=None if deferred is None else deferred['obligation_id'],case_ids=[] if transfer is None else transfer['case_ids'],
                triage_row_sha256=None,risk_tiers=[] if deferred is None else deferred['risk_tiers'],
                original_source_pins=None,pre_change_gate=None,status='NOT_IN_DEFERRED_LEDGER' if deferred is None else 'OPEN',
                owner_released=False,native_executed=False,
                disposition='Source/managed behavior-preserving orchestration only; native bindings/defaults/writer/chart bodies unchanged. No timing exception claimed.')
            if deferred is not None:
                assert control in triage
                assert rowsha(triage[control])==deferred['source_record_sha256']
                item.update(triage_row_sha256=deferred['source_record_sha256'],original_source_pins=deferred['source_evidence'],
                    original_source_record=deferred['source_record_path'],pre_change_gate=transfer['pre_change_gate'] if transfer else deferred['safe_capture_preconditions'])
            affected[control]=item
    # Capture only declared option values; the source-to-option mappings are explicit and unchanged.
    grouped=(ROOT/'BESHstatNG/src/UI/UibyID.vb').read_text(encoding='utf-8-sig')
    for argument,control in [('welch','ckWelch'),('lsd','ckLSD'),('bonferroni','ckBonferroni'),('tukey','ckTukey'),('gamesHowell','ckGamesHowell')]:assert argument+':=Me.'+control+'.Checked' in grouped
    assert 'dunn:=True' in grouped and grouped.count('executionRun.Checkpoint(ExecutionStage.Analysis, 60)')==original('BESHstatNG/src/UI/UibyID.vb').count('executionRun.Checkpoint(ExecutionStage.Analysis, 60)')
    pins=baseline['source_pins']
    protected=[p for p in pins if p.startswith(('BESHStat.Core/','planning/','docs/','BESHStat.OfficeAddin/','BESHStat.WasmSmoke/')) or '/ExcelUDFs/' in p or p.endswith('.Designer.vb') or '/UI/Refedit/' in p or p.startswith('BESHStat.Excel.Common/src/') or p=='BESHstatNG/src/DM/ExcelDnaDataImporter.vb' or p=='BESHstatNG/src/DM/WindowsAssumptionReportAdapter.vb' or p.startswith('scripts/tests/')]
    assert all(sha(p)==pins[p] for p in protected),'Protected source/evidence drift'
    assert sha('BESHstatNG/src/DM/ExcelDnaDataImporter.vb')=='32942d3034c2fb80befe23e6bb317b9dde3097774b9cef0373536d9199f36fc9'
    fixture='BESHStatNG.Test/src/LegacyAnovaWorkflow07.vb';assert sha(fixture)==json.loads((ROOT/'docs/architecture/04A07/baseline-v1.json').read_bytes())['fixture_sha256']
    original_geometry=json.loads((ROOT/'docs/architecture/04A27/geometry-owner-disposition-2026-11-02.json').read_bytes())
    assert len(original_geometry['original_44_obligations'])==44 and all(r['disposition']=='OPEN' and not r['owner_released'] for r in original_geometry['original_44_obligations'])
    paths=list(FORMS)+['BESHstatNG/src/DM/WindowsPairedInputAdapter.vb','BESHstatNG/src/DM/WindowsUnivariateInputAdapter.vb',
        'BESHstatNG/src/DM/WindowsAnovaWorkflowAdapter.vb','BESHStat.Excel.Common/src/Analysis/AnovaWorkflowPolicy.vb',
        'BESHStat.Excel.Common.Tests/src/AnovaWorkflow07Tests.vb','BESHStatNG.Test/src/AnovaWorkflow07AdapterTests.vb',fixture]
    return dict(schema='04A07-source-restriction-successor-v2',base=BASE,
        superseded_v1_sha256=sha('docs/architecture/04A07/source-restriction-crosswalk-v1.json'),
        development_repair='Preserve raw nested parser evidence separately from actual legacy Core-input values after DataObj numeric-text column stringification; no Core input, algorithm or native behavior change.',
        source_pins={p:sha(p) for p in paths},initial_source_pins={p:pins[p] for p in paths if p in pins},
        authorities={p:sha(p) for p in [AMENDMENT,LEDGER,CONTROLS,TRIAGE,'docs/architecture/04A02/04A02-owner-acceptance-2026-11-02.json','docs/architecture/04A03/pre-change-crosswalk.json','docs/architecture/04A04/compatibility-crosswalk.json','docs/architecture/04A05/owner-completion-dispositions-v1.json','docs/architecture/04A06/source-restriction-crosswalk-v1.json','docs/architecture/04A27/geometry-owner-disposition-2026-11-02.json']},
        changed_members={p:{n:dict(before_sha256=hashlib.sha256(v['before'][n].encode()).hexdigest(),after_sha256=hashlib.sha256(v['after'][n].encode()).hexdigest()) for n in FORMS[p]} for p,v in sources.items()},
        unaffected_form_sources_identical=True,native_tails_identical_except_nested_metadata_arguments=True,affected_controls=list(affected.values()),
        original_open_anova_normality_ids=[r['obligation_id'] for r in anova],original_open_transfer_ids=list(transfers),
        counts=dict(original_obligations=1514,restricted_live=802,transferred_obligations=239,transferred_controls=218,shared_obligations=160,shared_controls=141,importer_timing_obligations=44,
            source_associated_controls=len(affected),source_associated_deferred_obligations=sum(r['obligation_id'] is not None for r in affected.values())),
        gate_disposition='Only behavior-preserving extraction applied. Uncaptured mandatory pre-change evidence remains mandatory before any future behavior-changing native patch. C05/C27 timing exceptions NOT generalized.',
        candidate_findings=['SC07-NESTED-LABEL-DOMAIN: typed hierarchy parser permits repeated child labels across parents; Core explicitly rejects them. Existing rejection retained, no recoding/model change.',
            'Existing RM Tukey checkbox invokes both no-sphericity TukeyKramerRM2 and pooled-sphericity Tukey, in that order; both remain.',
            'Standalone grouped APIs preserve invalid-alpha default/warning; repeated typed API rejects invalid alpha. Actual Windows alpha comes from existing global normalizing setter.'],
        released_controls=0,discharged_obligations=0,native_executed=False,P='NO',N1='NO GO',N2='NO',C2='UNMET')

def check():
    record=json.loads(OUT.read_bytes());assert build()==record,'Successor source/ledger proof differs';return record
if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('action',choices=['build','check']);args=parser.parse_args()
    if args.action=='build':
        result=build()
        with OUT.open('x',encoding='utf-8',newline='\n') as f:json.dump(result,f,indent=2,ensure_ascii=False);f.write('\n')
    else:check()
    print('04A.07 source/restriction proof',args.action,'PASS')
