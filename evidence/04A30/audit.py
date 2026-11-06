"""04A.30 offline evidence helpers. Never writes historical or product files."""
from pathlib import Path
import csv, hashlib, json, subprocess, sys, time, os, re

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
BASE = '45466a88e7567c9f4711ba4fbbe295a7a2e69bb4'
def git(*args):
    return subprocess.check_output(['git', '-c', 'safe.directory='+ROOT.as_posix(), *args], cwd=ROOT).decode('utf-8-sig').strip()
def sha(p):
    h=hashlib.sha256()
    with Path(p).open('rb') as f:
        for block in iter(lambda:f.read(1024*1024), b''): h.update(block)
    return h.hexdigest()
def save(name,value):
    (HERE/name).write_text(json.dumps(value,ensure_ascii=False,allow_nan=False,indent=2)+'\n',encoding='utf-8')
def pin(p):
    p=Path(p); return dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size,sha256=sha(p))
def entry():
    if (HERE/'entry-state.json').exists():raise RuntimeError('Entry already frozen; do not overwrite it.')
    assert git('rev-parse','HEAD','main').splitlines()==[BASE,BASE]
    assert git('branch','--show-current')=='codex/04A-30'
    assert not git('diff','--cached','--name-only')
    paths=git('ls-files').splitlines()
    # One streaming pass over versioned authority. TestResults payload is not read.
    pin_path=HERE/'entry-pins.json'
    if pin_path.exists():
        pins=json.loads(pin_path.read_bytes())['files']
    else:
        pins=[pin(ROOT/p) for p in paths if (ROOT/p).is_file()]
        save('entry-pins.json',dict(files=pins,scope='All versioned files; no ignored historical TestResults payload rehash'))
    manifest=json.loads((ROOT/'evidence/04A29/publication-manifest.json').read_bytes())
    mismatches=[]
    for p in manifest['files']:
        if p['sha256'] is not None:
            actual=pin(ROOT/p['path'])
            if actual['sha256']!=p['sha256'] or actual['bytes']!=p['bytes']:
                committed=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+p['path']],cwd=ROOT)
                mismatches.append(dict(path=p['path'],recorded=p,actual=actual,matches_committed_blob=hashlib.sha256(committed).hexdigest()==actual['sha256']))
    save('inherited-04A29-manifest-discrepancies.json',dict(status='INHERITED_COMMITTED_MANIFEST_MISMATCH',count=len(mismatches),files=mismatches,action='Preserve committed authority unchanged. Owner qualification requested; not repaired in 04A30.'))
    ancestry={}
    for name,commit in [('04A29',BASE),('04A28','ed94086e449f22ee1a0fc3888d6209ecba85a8b5'),('test-rationalization','033cbe9033cc10bce3e19322e5582e5ea389fe22')]:
        subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'merge-base','--is-ancestor',commit,BASE],cwd=ROOT,check=True)
        ancestry[name]=commit
    save('entry-state.json',dict(branch='codex/04A-30',head=BASE,main=BASE,ancestry=ancestry,index='EMPTY',owner_tracked_edits=[],authority_04A29_manifest=pin(ROOT/'evidence/04A29/publication-manifest.json'),authority_04A29_files_verified=len(manifest['files']),plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md'),agents=pin(ROOT/'AGENTS.md'),plan_heading='Revision 2.11 handoff stale; owner expressly accepted 04A29',writer_check='Get-Process: no dotnet/MSBuild/testhost/python/Excel before our runner; six Node processes. CIM command-line inspection denied. Isolated outputs, no exclusive global-writer claim.',gates=dict(P='NO',N1='NO GO',N2='NO',Gate_C2='UNMET'),native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN',historical_results='Accepted immutable metadata receipt plus final scoped stat comparison; payload NOT_REHASHED',historical_helper_uncertainty=[171,172]))
    state=json.loads((HERE/'entry-state.json').read_bytes())
    state['authority_04A29_files_enumerated']=state.pop('authority_04A29_files_verified')
    state['authority_04A29_payload_pins_matched']=sum(p['sha256'] is not None for p in manifest['files'])-len(mismatches)
    state['authority_04A29_payload_pins_mismatched']=len(mismatches)
    save('entry-state.json',state)
    print('Entry pins and accepted 04A29 manifest correspondence checked',len(pins),'mismatches',len(mismatches))

MAP=[
 ('ROC','ROC.vb','compute','RocDataCore.Compute','ROC.compute','Move numeric state on copies; seed legacy AUC accumulator explicitly, both CI families and endpoints retained','04A.31','ROC source paired tests; ties, alpha, errors, repeats'),
 ('ROC','ROC.vb','DeLongSE','RocDataCore.DeLongSE','ROC.DeLongSE','Same ranks, variances, CI and n<2 exception','04A.31','ROC source paired tests'),
 ('ROC','ROC.vb','MidRanks','RocDataCore.MidRanks','ROC.MidRanks','Original-order ranks and equality tie blocks','04A.31','ties/nonfinite source comparison'),
 ('Normal Q-Q','NormalPlot.vb','.ctor','NormalPlotDataCore.SortedCopy','NormalPlot.New','Intentional copy-safe boundary replaces caller sort; sorted values unchanged','04A.31','legacy constructor mutation / portable copies'),
 ('Normal Q-Q','NormalPlot.vb','compute','NormalPlotDataCore.Scores','NormalPlot.compute','Literal method labels and zero fallback; X sorted data / Y scores','04A.31','3 score x 3 line source comparisons'),
 ('Normal Q-Q','NormalPlot.vb','computeRefLIne','NormalPlotDataCore.ReferenceLine','NormalPlot.computeRefLIne','Existing statistical routines, R semantics, 2x2 endpoints retained','04A.31','3 score x 3 line source comparisons'),
 ('Box','BoxPlot.vb','CalcForPlotting','BoxSegmentsCore.Compute','BoxPlot.CalcForPlotting','Only five signed stacked segments move; original DescriptiveStat, whisker scans and accumulation stay','04A.32','signed/zero/nonfinite scalar segments from old method'),
 ('Histogram','Histogram.vb','compute','HistogramDataCore.BinsCopy / OverlayCopy','Histogram.compute','Call existing engine using clones; PrettyBreaks always on. Deliberate removal of indirect FD/overlay source sort','04A.32','existing histogram tests + copy-safe adapter comparison'),
 ('Histogram','Histogram.vb','addChart','HistogramDataCore.HorizontalBounds','Histogram.addChart','Move midpoint-edge bound calculation only; secondary Y, Tukey, events and Shapes stay native','04A.32','one/two/empty/nonfinite midpoint source comparison'),
 ('Axis','ChartingFunc.vb','ChartScaling','ChartAxisScalingCore.Compute (existing)','ChartingFunc.ChartScaling (existing)','No new scaling calculator; exact CHARTscale Min/Max/Scale adapter retained','04A.32','existing axis tests + Windows adapter parity'),
 ('Scatter matrix','ScatterPlotMatrix.vb','transformData','ScatterMatrixDataCore.Normalize','ScatterPlotMatrix.transformData','Move n x p scaling and diagonal label vector; .1 overwritten margin retained. Constant spans keep IEEE behavior','04A.33','frozen panel transform + rectangular/constant cases'),
 ('XYZ','XYZscatter.vb','getnorm','XyzDataCore.NormalizeAxis','XYZscatter.getnorm','Move raw axis normalization [-.5,.5] and direction only; ranges/manual bounds/rotations/cage remain','04A.33','frozen normalization with reversal/zero spans'),
 ('Labels','BoxPlot.vb','.ctor','OrderedPlotGroups (Common)','BoxPlot.New','Ordered supplied groups and raw/display names retained verbatim; nullable physical RowIds absent unless provided','04A.32','Common ordered group fixtures with duplicates, whitespace, identities'),
 ('Labels','NormalPlot.vb','.ctor','OrderedPlotGroups (Common)','NormalPlot.New','One raw group descriptor; sorted view distinct from original observation axis','04A.31','source/plotted-index mapping and copies')
]
def operation_map():
    with (ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig',newline='') as f: members=list(csv.DictReader(f))
    rows=[]
    for family,file,name,dest,consumer,behavior,deferred,test in MAP:
        candidates=[m for m in members if m['path']=='BESHstatNG/src/Graphics/'+file and m['name']==name and m['kind'] in ('SubBlock','FunctionBlock','ConstructorBlock')]
        assert candidates,(file,name)
        for m in candidates:
            rows.append(dict(family=family,accepted_04A29_member_id=m['member_id'],accepted_04A29_operation_id=m['operation_id'],old_source=m['path'],old_source_sha256=m['source_sha256'],old_body_sha256=m['body_sha256'],old_lines=m['start_line']+'-'+m['end_line'],current_owner=m['ownership'],destination=dest,windows_consumer=consumer,behavior=behavior,copy_missing_labels='Copies at new portable boundaries; no finite filtering; supplied labels/order preserved; no fabricated physical RowIds',status='PLANNED_BEFORE_EDIT',remaining_owner=deferred,intended_test=test,original_controls='OPEN; zero discharges; NOT_GOLDEN; NATIVE_NOT_RUN'))
    with (HERE/'migration-operation-map-v1.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    save('implementation-map-entry.json',dict(status='RECORDED_BEFORE_PRODUCT_EDITS',base=BASE,operation_records=len(rows),source_members_verified=True,planned_product_files=['BESHStat.Core/src/Graphics/'+f for f in ['RocDataCore.vb','NormalPlotDataCore.vb','BoxSegmentsCore.vb','HistogramDataCore.vb','ScatterMatrixDataCore.vb','XyzDataCore.vb']]+['BESHStat.Excel.Common/src/Analysis/OrderedPlotGroups.vb']+['BESHstatNG/src/Graphics/'+f for f in ['ROC.vb','NormalPlot.vb','BoxPlot.vb','Histogram.vb','ScatterPlotMatrix.vb','XYZscatter.vb']],already_migrated=['ChartAxisScalingCore','HistogramBinningCore'],native_methods='Retained; no renderer rewrite',label_scope='Existing supplied ordered group path in Box and single raw-group path in Normal; no 04A03 parser or equality normalization changes',callers=['UI/UibyID.vb ROC/Normal/Histogram/Box','UI/Ui0OneRefeditMulticol.vb Box','UI/Ui11PCA.vb ScatterPlotMatrix','UI/Ui3XYZplot.vb XYZ','DM/WindowsAssumptionReportAdapter.vb Box','DM/WindowsAnovaWorkflowAdapter.vb Box']))
    print('Pre-edit operation map',len(rows))

def qualify_privacy():
    p=HERE/'entry-pins.json';raw=p.read_bytes();value=json.loads(raw)
    private=ROOT/'.test-rationalization-04A30.tmp/private-entry-pins.json'
    if not private.exists():private.write_bytes(raw)
    computer=os.environ.get('COMPUTERNAME','!unset!').casefold()
    count=0
    for item in value['files']:
        path=item['path']
        if re.search(r'DESKTOP-|LAPTOP-|S-1-5-|[A-Za-z]:',path,re.I) or computer in path.casefold():
            item['path']='<PRIVATE_PROTECTED_PATH:'+hashlib.sha256(path.encode()).hexdigest()[:24]+'>'
            count+=1
    if count:
        save('entry-pins.json',value)
        save('entry-privacy-qualification.json',dict(raw_receipt_sha256=hashlib.sha256(raw).hexdigest(),public_receipt=pin(p),private_path_aliases=count,resolution='Match SHA256(relative Git path) first24 against entry frozen Git population; do not print private names.',raw_diagnostic='Fresh ignored run root; not published'))
    print('Entry private path aliases',count)

if __name__=='__main__':
    {'entry':entry,'map':operation_map,'qualify-privacy':qualify_privacy}[sys.argv[1]]()
