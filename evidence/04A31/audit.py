"""04A31 offline source authority and publication checks; no historical writes."""
from pathlib import Path
import collections,csv,hashlib,json,os,re,subprocess,sys,zipfile
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
RUN=ROOT/'.test-rationalization-04A31.tmp'
BASE='f840b1695fd95be25faba6fd5408c9498a31d804'
WINDOWS=['BESHstatNG/src/Graphics/'+n+'.vb' for n in ['ROC','NormalPlot']]
NEW_CORE=['BESHStat.Core/src/Graphics/'+n+'.vb' for n in ['RocPlotSeriesData','NormalPlotSeriesData']]
TESTS=['BESHStat.Core.Tests/src/Graphics/'+n+'.vb' for n in ['Graphics31RocTests','Graphics31NormalPlotTests','Graphics31Evidence']]+['BESHStat.Excel.Common.Tests/src/Graphics31SourceIdentityTests.vb','BESHStatNG.Test/src/Charts/Graphics31AdapterTests.vb']
PROJECT='BESHStatNG.Test/BESHStatNG.Tests.vbproj'
def git(*args):
    return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode('utf-8-sig').strip()
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def pin(p):
    p=Path(p);return dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size,sha256=sha(p))
def save(name,value):
    (HERE/name).write_text(json.dumps(value,ensure_ascii=False,allow_nan=False,indent=2)+'\n',encoding='utf-8')
def public_path(p):
    if re.search(r'DESKTOP-|LAPTOP-|S-1-5-|[A-Za-z]:',p,re.I) or os.environ.get('COMPUTERNAME','!unset!').lower() in p.lower():
        return '<PRIVATE_PROTECTED_PATH:'+hashlib.sha256(p.encode()).hexdigest()[:24]+'>'
    return p
def entry():
    assert not (HERE/'entry-state.json').exists()
    assert git('rev-parse','HEAD','main').splitlines()==[BASE,BASE]
    assert git('branch','--show-current')=='codex/04A-31'
    assert not git('diff','--cached','--name-only') and not git('diff','--name-only')
    RUN.mkdir(exist_ok=False)
    paths=git('ls-files').splitlines()
    records=[]
    for p in paths:
        if not (ROOT/p).is_file():continue
        row=pin(ROOT/p);row['path']=public_path(p);records.append(row)
    save('source-pins.json',dict(schema='04A31-source-pins-v1',base=BASE,scope='All entry tracked files; ignored historical TestResults payload NOT_REHASHED',files=records))
    old=json.loads((ROOT/'evidence/04A30/publication-manifest.json').read_bytes())
    archive=ROOT/'.04a30-original-byte-final-v1.tmp/BESHStatNG_04A30_FullSolution_Review.zip'
    receipt=json.loads((archive.parent/'PACKAGE_RECEIPT.json').read_bytes())
    assert sha(archive)==receipt['sha256']
    correspondence=[]
    with zipfile.ZipFile(archive) as z:
        assert len(z.namelist())==len(set(z.namelist()))
        for row in old['files']:
            if row['sha256'] is None:continue
            b=z.read(row['path']);current=(ROOT/row['path']).read_bytes()
            assert len(b)==row['bytes'] and hashlib.sha256(b).hexdigest()==row['sha256'],row['path']
            relation='DIRECT' if b==current else 'EXACT_ORIGINAL_CRLF_TO_LF'
            assert b==current or b.replace(b'\r\n',b'\n')==current,row['path']
            committed=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+row['path']],cwd=ROOT)
            assert current==committed,row['path']
            correspondence.append(dict(path=row['path'],original_sha256=row['sha256'],committed=pin(ROOT/row['path']),relation=relation))
    qualification=json.loads((ROOT/'evidence/04A30/04A29-eol-qualification-v2.json').read_bytes())
    assert qualification['externally_authenticated_original_entries']==120 and qualification['counts']=={'DIRECT_ORIGINAL_HASH_MATCH':27,'ORIGINAL_BYTES_CRLF_TO_LF_PROVEN':93,'MANIFEST_SELF_ENTRY':1}
    successor=json.loads((ROOT/'evidence/04A30/04A28-source-transition-successor-v2.json').read_bytes())
    assert (successor['total_old_pins'],successor['unchanged'],successor['authorized_graphics_transitions'])==(278,272,6)
    save('entry-state.json',dict(schema='04A31-entry-v1',branch='codex/04A-31',head=BASE,merged_main=BASE,parent=git('rev-parse',BASE+'^'),index='EMPTY',tracked_edits=[],untracked_owner_files=[],branch_authority='Actual merged main; archive used ONLY to qualify manifest byte normalization',writers='CIM process classification: three idle /nodemode:1 MSBuild nodes; no dotnet/vstest/python observed; other Codex/Node/GitHubDesktop processes not proved globally idle. No Git index lock. Isolated outputs required.',git_safety='Per-command safe.directory only; no global configuration changed. Branch creation required sandbox metadata approval.',roadmap_discrepancy='Revision 2.11 heading/count/handoff still says 04A29 next; official 04A30-33 rows agree. Explicit owner acceptance and merged source control entry; plan retained unchanged.',authority=[pin(ROOT/p) for p in ['AGENTS.md','planning/BESHStatNG_CrossPlatform_MasterPlan.md','docs/architecture/batch-04A-29-owner-review.md','docs/architecture/batch-04A-30-owner-review.md','evidence/04A30/publication-manifest.json','evidence/04A30/04A29-eol-qualification-v2.json','evidence/04A30/04A28-source-transition-successor-v2.json']],prior_original_byte_qualification='FROZEN_ACCEPTED_RECEIPT: 120 originals authenticated;27 direct/93 exact transformations; original ZIP not requested or rerun',postmerge_04A30_manifest=dict(archive_bytes=receipt['bytes'],archive_sha256=receipt['sha256'],counts=dict(collections.Counter(x['relation'] for x in correspondence)),unexplained=0,files=correspondence),restrictions=dict(P='NO',N1='NO GO',N2='NO',Gate_C2='UNMET',Gate_C3='NOT_APPROVED',native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN',native_releases=0,original_control_discharges=0)))
    members=list(csv.DictReader((ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig')))
    previous=list(csv.DictReader((ROOT/'evidence/04A30/migration-operation-map-v1.csv').open(encoding='utf-8-sig')))
    operations=[('ROC','compute','RocDataCore.ComputeCurve -> ComputeDeLong','Numerics and copied curve arrays already Core','RocPlotSeriesData result projection','Graphics30Tests.RocFrozenSource'),('ROC','addROCplot','Windows chart + copied ROC arrays','Numerics already Core; diagonal literal Windows','RocPlotSeriesData X/Y, cutoff indexes and diagonal; chart presentation Windows','Graphics31RocTests / Graphics31AdapterTests'),('ROC','wrapResults','Windows tables','Table shape/format/p-values already accepted','Unchanged Windows: result formatting is host presentation','Graphics30AdapterTests.RocWindowsAdapterRetainsFirstAndRepeatedStateAndReportShape'),('NormalPlot','.ctor','NormalPlotDataCore.SortedCopy; OrderedPlotGroups','Sorted copy and labels already portable','NormalPlotObservationOrder + existing OrderedPlotGroups.PointOrder','Graphics31SourceIdentityTests'),('NormalPlot','compute','NormalPlotDataCore.Scores','All three exact methods already Core','NormalPlotSeriesData.Project; no new scoring engine','Graphics30Tests.NormalFrozenSource'),('NormalPlot','computeRefLIne','NormalPlotDataCore.ReferenceLine','All line algorithms already Core','NormalPlotSeriesData reference endpoint snapshot','Graphics31NormalPlotTests.NineCombinations'),('NormalPlot','addChart','Windows chart and reference matrix columns','Numeric scores/line already Core','NormalPlotSeriesData X=observed,Y=expected; reference X/Y','Graphics31AdapterTests')]
    rows=[]
    for typ,name,owner,done,destination,test in operations:
        path='BESHstatNG/src/Graphics/'+typ+'.vb';text=(ROOT/path).read_text(encoding='utf-8-sig');lines=text.splitlines()
        marker='Sub New(' if name=='.ctor' else ('Function '+name+'(' if name in ['wrapResults','computeRefLIne'] or (name=='compute' and typ=='NormalPlot') else 'Sub '+name+'(')
        start=next(i for i,l in enumerate(lines) if marker in l)
        end=next(i for i in range(start+1,len(lines)) if re.match(r'\s*End (Sub|Function)\s*$',lines[i]))
        old=next(m for m in members if m['path']==path and m['name']==name and m['kind'] in ['SubBlock','FunctionBlock','ConstructorBlock'])
        prior=[r.get('destination','') for r in previous if r['accepted_04A29_member_id']==old['member_id']]
        rows.append(dict(family=typ,source=path,entry_sha256=sha(ROOT/path),member=name,entry_lines=f'{start+1}-{end+1}',member_sha256=hashlib.sha256(('\n'.join(lines[start:end+1])).encode()).hexdigest(),accepted_04A29_member_id=old['member_id'],accepted_04A29_operation_id=old['operation_id'],accepted_04A30_map='evidence/04A30/migration-operation-map-v1.csv',accepted_04A30_destination=';'.join(prior) or 'Retained Windows renderer/table; no 04A30 member edit',current_owner=owner,already_migrated=done,missing='Coordinate ownership/provenance only' if name!='wrapResults' else 'No missing work',proposed_destination=destination,regression=test,windows_reason='Native creation/styling/lifetime or tables remain host presentation',deferred_owner='Numerics 04A39-41; native 04A45-48; detrending 04A68-69',restriction='OPEN;P=NO;N1=NO GO;N2=NO;C2=UNMET;NOT_GOLDEN;NATIVE_NOT_RUN'))
    with (HERE/'roc-qq-operation-map-entry.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    print('Entry frozen:',len(records),'tracked pins;',len(rows),'operations;',len(correspondence),'prior publication originals authenticated')
def protection():
    entry=json.loads((HERE/'source-pins.json').read_bytes())['files']
    paths=git('ls-files').splitlines(); aliases={public_path(p):p for p in paths}
    changed=[];missing=[];unchanged=0
    for row in entry:
        p=ROOT/aliases[row['path']]
        if not p.is_file():missing.append(row['path']);continue
        if p.stat().st_size!=row['bytes'] or sha(p)!=row['sha256']:
            changed.append(dict(path=row['path'],entry_sha256=row['sha256'],final_sha256=sha(p)))
        else:unchanged+=1
    assert not missing and {r['path'] for r in changed}==set(WINDOWS+[PROJECT]),changed
    registry=json.loads((ROOT/'evidence/04A14/baseline-registry.json').read_bytes())
    captures=[dict(path=public_path(p),sha256=sha(ROOT/p),matches_registry=sha(ROOT/p)==v['sha256']) for p,v in registry['files'].items()]
    assert len(captures)==100 and all(r['matches_registry'] for r in captures)
    old=json.loads((ROOT/'evidence/04A28/source-preservation.json').read_bytes())['protected_compiled_sources']
    prior=json.loads((ROOT/'evidence/04A30/04A28-source-transition-successor-v2.json').read_bytes())
    original_changes={x['path']:x for x in prior['transitions']}
    bypath={aliases[x['path']]:x for x in entry}
    rows=[]
    for p,original in sorted(old.items()):
        entry_sha=bypath[p]['sha256']; final=sha(ROOT/p)
        assert (entry_sha!=original)==(p in original_changes),(p,'unexpected original transition')
        assert entry_sha==final or p in WINDOWS
        if p in original_changes:
            assert original_changes[p]['old_sha256']==original
            corr=next(x for x in json.loads((HERE/'entry-state.json').read_bytes())['postmerge_04A30_manifest']['files'] if x['path']==p)
            assert corr['original_sha256']==original_changes[p]['new_sha256']
        rows.append(dict(path=p,original_04A28_sha256=original,merged_04A30_sha256=entry_sha,final_04A31_sha256=final,relation='UNCHANGED_ORIGINAL' if original==final else ('AUTHORIZED_04A31_SUCCESSOR_OF_04A30' if p in WINDOWS else 'RETAINED_AUTHORIZED_04A30')))
    assert len(rows)==278 and sum(x['relation']=='UNCHANGED_ORIGINAL' for x in rows)==272
    save('source-transition-successor.json',dict(schema='04A31-source-transition-v1',status='PASS',historical_04A28_pins=278,unchanged_original=272,retained_04A30_transitions=4,further_04A31_transitions=2,prior_successor=pin(ROOT/'evidence/04A30/04A28-source-transition-successor-v2.json'),accepted_eol_qualification=pin(ROOT/'evidence/04A30/04A29-eol-qualification-v2.json'),postmerge_eol_qualification='entry-state.json: original 04A30 publication bytes authenticated before exact LF comparison to merged Git blobs',pins=rows,new_production_sources=[pin(ROOT/p) for p in NEW_CORE],test_project_transition=next(x for x in changed if x['path']==PROJECT),old_six_source_assertions='RETAINED_UNCHANGED_FAIL: test_04a28_output whole-source protection; no old expected SHA/assertion rewritten',scientific='NOT_GOLDEN',native='NATIVE_NOT_RUN'))
    cores=[p for p in paths if p.startswith('BESHStat.Core/src/Graphics/') and p.endswith('.vb')]
    windows=[p for p in paths if p.startswith('BESHstatNG/src/Graphics/') and p.endswith('.vb')]
    assert len(cores)==18 and all(sha(ROOT/p)==bypath[p]['sha256'] for p in cores)
    assert len(windows)==38 and all(sha(ROOT/p)==bypath[p]['sha256'] for p in windows if p not in WINDOWS)
    save('source-preservation.json',dict(schema='04A31-source-preservation-v1',status='PASS',tracked_entry_files=len(entry),unchanged=unchanged,changed=changed,missing=missing,all_historical_tracked_artifacts_byte_preserved=True,all_36_other_windows_graphics_unchanged=True,all_18_existing_core_graphics_unchanged=True,original_12_core_graphics_unchanged=True,all_100_captures_match_registry=True,captures=captures,original_ledgers_and_uncertainties='04A01 decisions,04A29 canonical-v2 lineage and171/172 uncertainties retained; zero discharges',historical_results='Prior accepted metadata receipt pinned in entry. One final scoped stat/membership comparison, separately reported; ignored138GB payload NOT_REHASHED.',targets_ribbon_udfs_installers_and_signing_unchanged=True))
    rows=list(csv.DictReader((HERE/'roc-qq-operation-map-entry.csv').open(encoding='utf-8-sig')))
    ownership=json.loads((HERE/'source-ownership.json').read_bytes())['members']
    for row in rows:
        name='Sub New' if row['member']=='.ctor' else row['member']+' ('
        current=[x for x in ownership if x['path']==row['source'] and name in x['member']]
        assert len(current)==1,(row['member'],current)
        row.update(final_sha256=sha(ROOT/row['source']),final_lines=f"{current[0]['final_start']}-{current[0]['final_end']}",final_token_sha256=current[0]['final_token_sha256'],status='IMPLEMENTED_SOFTWARE_ONLY' if row['member']!='wrapResults' else 'RETAINED_TOKEN_IDENTICAL')
    with (HERE/'roc-qq-operation-map-final.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    print('PASS scoped source/history bytes,278-pin lineage,100 captures,18 Core engines,36 other Windows modules')
if __name__=='__main__':
    {'entry':entry,'protect':protection}[sys.argv[1] if len(sys.argv)>1 else 'entry']()
