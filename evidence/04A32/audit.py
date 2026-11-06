"""04A32 scoped offline authority. Historical files are read-only."""
from pathlib import Path
import csv,io,collections,hashlib,json,subprocess,os,re,sys
ROOT=Path(__file__).resolve().parents[2];HERE=Path(__file__).resolve().parent
RUN=ROOT/'.test-rationalization-04A32.tmp';BASE='43a752b169fb996ed65387d88d380b1386e92240'
WINDOWS=['BESHstatNG/src/Graphics/'+x+'.vb' for x in ['BoxPlot','Histogram','ViolinPlot']]
CORE_OLD=['BESHStat.Core/src/Graphics/'+x+'.vb' for x in ['HistogramBinningCore','HistogramDataCore','ViolinPlotCore']]
CORE_NEW=['BESHStat.Core/src/Graphics/'+x+'.vb' for x in ['BoxPlotDataCore','HistogramPlotDataCore','DistributionPlotGeometryCore']]
PROJECT='BESHStatNG.Test/BESHStatNG.Tests.vbproj'
def git(*args):return subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT).decode('utf-8-sig').strip()
def sha(p):return hashlib.sha256(Path(p).read_bytes()).hexdigest()
def pin(p):
 p=Path(p);return dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size,sha256=sha(p))
def load(p):return json.loads(Path(p).read_bytes())
def save(name,v): (HERE/name).write_text(json.dumps(v,ensure_ascii=False,allow_nan=False,indent=2)+'\n',encoding='utf-8')
def public_path(p):
 return '<PRIVATE_PROTECTED_PATH:'+hashlib.sha256(p.encode()).hexdigest()[:24]+'>' if re.search(r'DESKTOP-|LAPTOP-|S-1-5-|[A-Za-z]:',p,re.I) or os.environ.get('COMPUTERNAME','!unset!').lower() in p.lower() else p
def entry():
 assert not (HERE/'entry-state.json').exists()
 assert git('rev-parse','HEAD','main').splitlines()==[BASE,BASE] and git('branch','--show-current')=='codex/04A-32'
 assert not git('diff','--name-only') and not git('diff','--cached','--name-only')
 RUN.mkdir(exist_ok=False)
 records=[]
 for p in git('ls-files').splitlines():
  if (ROOT/p).is_file():r=pin(ROOT/p);r['path']=public_path(p);records.append(r)
 save('entry-pins.json',dict(schema='04A32-entry-pins-v1',files=records,scope='All tracked entry files;ignored historical payload NOT_REHASHED'))
 prior=load(ROOT/'evidence/04A31/publication-manifest.json');qualified=[]
 for r in prior['files']:
  if r['sha256'] is None:continue
  b=(ROOT/r['path']).read_bytes();cr=b.replace(b'\n',b'\r\n');variants={'DIRECT':b,'EXACT_CRLF_RECONSTITUTION':cr,'CRLF_EXCEPT_LITERAL_FINAL_LF':cr[:-2]+b'\n' if cr.endswith(b'\r\n') else cr}
  if r['path'].endswith('.csv'):
   s=io.StringIO(newline='');csv.writer(s).writerows(csv.reader(io.StringIO(b.decode(),newline='')));variants['CSV_RECORD_CRLF_EMBEDDED_LF']=s.getvalue().encode()
  match=[k for k,v in variants.items() if len(v)==r['bytes'] and hashlib.sha256(v).hexdigest()==r['sha256']]
  assert match,r['path']
  committed=subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+r['path']],cwd=ROOT)
  assert b==committed
  qualified.append(dict(path=r['path'],relation=match[0],original_sha256=r['sha256'],merged=pin(ROOT/r['path'])))
 save('entry-state.json',dict(schema='04A32-entry-v1',branch='codex/04A-32',base=BASE,main=BASE,head=BASE,parent=git('rev-parse',BASE+'^'),index='EMPTY',tracked_owner_changes=[],untracked_owner_files=[],writers='No dotnet/testhost/python/Excel observed;two MSBuild nodes visible. No index lock. Other writers not globally excluded;isolated outputs and source hashes used.',authority='Actual merged main controls branch;owner acceptance supersedes stale plan handoff',plan_discrepancies=['Revision2.11 handoff/counts stale;official04A32 row agrees','Live standalone DescriptiveStat,Histogram and Violin all call the same StatFunc.QuartilesComp. No differing-quartile fixture is fabricated;distinct call paths,finite policies and whisker algorithms retained.'],previous_publication_qualification=qualified,qualification_limit='Digest-exact inverse EOL/CSV reconstruction against frozen manifest plus committed blobs;no fresh inspection of pre-normalization original31 archive is claimed. Accepted29 original-byte receipt remains frozen.',protected_authorities=[pin(ROOT/p) for p in ['AGENTS.md','planning/BESHStatNG_CrossPlatform_MasterPlan.md','docs/architecture/batch-04A-31-owner-review.md','evidence/04A30/graphics-roadmap-handoff-v1.json','evidence/04A31/source-transition-successor.json','evidence/04A31/publication-manifest.json']],planned_production=WINDOWS+CORE_OLD+CORE_NEW,why_existing_core_changes={'HistogramBinningCore':'Expose actual classification breaks from SAME calculator;old signature retained;no duplicated bin/PrettyBreaks logic','HistogramDataCore':'Add copied bins+edges entry to same engine','ViolinPlotCore':'Capture accepted original observation ordinal alongside existing group value append;add copied metadata only'},protected='All other sources,ROC/QQ,old tests,evidence,plan,controls,100captures;no04A33',restrictions=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN',releases=0,discharges=0)))
 members=list(csv.DictReader((ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig')))
 rows=[]
 for p in WINDOWS+['BESHstatNG/src/Graphics/ChartingFunc.vb']:
  for m in members:
   if m['path']!=p or m['kind'] not in ('SubBlock','FunctionBlock','ConstructorBlock'):continue
   name=m['name']; wanted={'BoxPlot.vb':['.ctor','Calculate','CalcForPlotting','AddBoxPlot','wrapResults','AddIndividualObservationSeries','DeterministicJitter'],'Histogram.vb':['compute','addChart'],'ViolinPlot.vb':['Compute','ResolveAxisContext','ResolveAutomaticYLimits','AddCategoryLabelSeries','DrawViolinFreeform','DrawInnerSummary','DrawObservationShapes','DeterministicJitter'],'ChartingFunc.vb':['ChartScaling']}
   if name not in wanted[Path(p).name]:continue
   rows.append(dict(path=p,entry_sha256=sha(ROOT/p),member=name,accepted_04A29_member_id=m['member_id'],accepted_04A29_operation_id=m['operation_id'],old_audit_lines=m['start_line']+'-'+m['end_line'],range_authority='Historical ranges only;current ranges and body pins in source inventory/successor',prior_map='evidence/04A30/migration-operation-map-v1.csv',current_owner='Windows numeric/state+native;engines already Core',destination='BoxPlotDataCore / HistogramPlotDataCore / DistributionPlotGeometryCore;ChartScaling already Core',native_reason='COM chart/series,shape,pixel transforms,appearance,events and lifecycle remain Windows',restrictions='OPEN;NOT_GOLDEN;NATIVE_NOT_RUN;zero discharges'))
 with (HERE/'operation-map-entry.csv').open('w',encoding='utf-8',newline='') as f:w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
 print('Frozen',len(records),'tracked files;',len(rows),'operation links;',len(qualified),'prior publication payloads qualified')
def protection():
 entry=load(HERE/'entry-pins.json')['files'];paths=git('ls-files').splitlines();aliases={public_path(p):p for p in paths};by={aliases[r['path']]:r for r in entry}
 changed=[];missing=[]
 for row in entry:
  p=aliases[row['path']]
  if not (ROOT/p).is_file():missing.append(row['path']);continue
  if pin(ROOT/p)!=dict(row,path=p):changed.append(dict(path=p,entry_sha256=row['sha256'],final_sha256=sha(ROOT/p)))
 assert not missing and {r['path'] for r in changed}==set(WINDOWS+CORE_OLD+[PROJECT]),changed
 from integrate import transforms
 original={p:subprocess.check_output(['git','-c','safe.directory='+ROOT.as_posix(),'show',BASE+':'+p],cwd=ROOT).decode('utf-8-sig') for p in WINDOWS+CORE_OLD}
 expected=transforms(original)
 assert all((ROOT/p).read_text(encoding='utf-8-sig')==v for p,v in expected.items()),'Change beyond exact bounded implementation transform'
 prior=load(ROOT/'evidence/04A31/source-transition-successor.json');rows=[]
 assert len(prior['pins'])==prior['historical_04A28_pins']==278
 for row in prior['pins']:
  p=row['path'];actual=sha(ROOT/p);entry_sha=by[p]['sha256']
  assert entry_sha==row['final_04A31_sha256'],(p,'inherited source lineage differs')
  assert entry_sha==actual or p in WINDOWS+CORE_OLD,(p,'unapproved transition')
  rows.append(dict(path=p,original_04A28_sha256=row['original_04A28_sha256'],accepted_04A31_sha256=row['final_04A31_sha256'],merged_main_sha256=entry_sha,final_04A32_sha256=actual,transition='AUTHORIZED_04A32' if entry_sha!=actual else 'RETAINED_04A31_LINEAGE'))
 save('source-transition-successor.json',dict(schema='04A32-source-transition-v1',status='PASS',prior=pin(ROOT/'evidence/04A31/source-transition-successor.json'),pins=rows,total_old_pins=len(rows),original_unchanged=sum(r['original_04A28_sha256']==r['final_04A32_sha256'] for r in rows),new_transitions=[r for r in rows if r['transition']=='AUTHORIZED_04A32'],other_authorized_source_changes=[r for r in changed if r['path'] not in {x['path'] for x in rows}],new_production=[pin(ROOT/p) for p in CORE_NEW],historical_failures='All old assertions/expected values retained;whole-file pin failures for new authorized dependencies remain real nonpasses. This successor checks exact source ownership without rebaselining old tests.',native='NATIVE_NOT_RUN',scientific='NOT_GOLDEN'))
 registry=load(ROOT/'evidence/04A14/baseline-registry.json')
 captures=[dict(path=public_path(p),sha256=sha(ROOT/p),matches=sha(ROOT/p)==v['sha256']) for p,v in registry['files'].items()]
 assert len(captures)==100 and all(x['matches'] for x in captures)
 core=[p for p in paths if p.startswith('BESHStat.Core/src/Graphics/') and p.endswith('.vb')];windows=[p for p in paths if p.startswith('BESHstatNG/src/Graphics/') and p.endswith('.vb')]
 assert len(core)==20 and len(windows)==38
 assert all(sha(ROOT/p)==by[p]['sha256'] for p in core if p not in CORE_OLD)
 assert all(sha(ROOT/p)==by[p]['sha256'] for p in windows if p not in WINDOWS)
 save('source-preservation.json',dict(schema='04A32-source-preservation-v1',status='PASS',tracked_entry=len(entry),unchanged=len(entry)-len(changed),changed=changed,missing=missing,all_35_other_windows_graphics_unchanged=True,all_17_other_core_graphics_unchanged=True,all_accepted_roc_qq_and_31_sources_unchanged=True,all_old_tests_and_evidence_byte_preserved=True,master_plan_agents_ribbon_udfs_office_wasm_installer_targets_unchanged=True,all_100_captures_preserved=True,captures=captures,native_releases=0,control_discharges=0,historical_payload='NOT_REHASHED;separate bounded membership/size/mtime receipt',guard=pin(HERE/'source-ownership.json'),windows_transform='Complete files equal the exact approved coordinate/state substitutions from actual merged main;all native style,shape/event/pixel code outside explicit numeric substitutions unchanged.'))
 inventory=load(HERE/'source-ownership.json')['members'];maps=list(csv.DictReader((HERE/'operation-map-entry.csv').open(encoding='utf-8-sig')));final=[]
 historical={m['member_id']:m for m in csv.DictReader((ROOT/'evidence/04A29/graphics-members-v1.csv').open(encoding='utf-8-sig'))}
 def signature(value):return re.sub(r'\s+','',value).casefold()
 for row in maps:
  name='New' if row['member']=='.ctor' else row['member']
  expected_header=signature(historical[row['accepted_04A29_member_id']]['signature'])
  candidates=[m for m in inventory if m['path']==row['path'] and m['name']==name and signature(m['member'].split('|',1)[1])==expected_header]
  assert len(candidates)==1,(row['accepted_04A29_member_id'],candidates)
  for m in candidates:
   final.append(dict(**row,current_entry_range=str(m['entry_start'])+'-'+str(m['entry_end']),final_range=str(m['final_start'])+'-'+str(m['final_end']),entry_member_sha256=m['entry_body_sha256'],final_member_sha256=m['final_body_sha256'],final_source_sha256=sha(ROOT/row['path']),header_preserved=m['header_preserved'],body_preserved=m['body_preserved']))
 with (HERE/'operation-map-final.csv').open('w',encoding='utf-8',newline='') as f:w=csv.DictWriter(f,fieldnames=list(final[0]));w.writeheader();w.writerows(final)
 print('PASS',len(entry),'tracked entry pins;100 captures;278-pin lineage;',sum(r['transition']=='AUTHORIZED_04A32' for r in rows),'new transitions;bounded transforms')
if __name__=='__main__':{'entry':entry,'protect':protection}[sys.argv[1] if len(sys.argv)>1 else 'entry']()
