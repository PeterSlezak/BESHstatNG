"""C27 source completion, individual control adjudications and preservation.

Read-only --check/--guard never publish, approve deferrals or change strict rules.
"""
from __future__ import annotations
import argparse,csv,hashlib,json,pathlib
import amendment as a,catalog,semantic
from continuation25_report import IDENTITY,CONTROL,EDGE,checked_files,exact_ids,prefix_errors,schema_errors,member_errors,control_errors,readiness_exit
R=catalog.ROOT;L=catalog.DEFAULT;T=semantic.TOOLING;H=R/'TestResults/04A-01-continuation-27'
QUALIFIED_NAMES=frozenset('grpDescriptiveStat grpBinSize grpReferenceLine grpNormalScores grpSymmetryTest grpOutlierTests lblAlphaOutliers grpHypothesisType_UTT grpVarianceModel_UTT grpCatHistBinSize grpCatHistAppearance grpCatHistPlotType lblCatHistGapWidth lblCatHistPalette grpAppearnceViolin lblViolinPalette lblViolinFillTransparency grpViolinDensity lblViolinBandwidth lblViolinDensityPoints grpScalingDisplay lblViolinScaling lblViolinChartHeight lblViolinChartWidth grpAppearnceBox lblBoxPalette grpBoxDisplay grpCDFPlot lblCDFPlotType lblCDFYScale grpCDFReference grpCDFAppearance lblCDFChartHeight lblCDFChartWidth lblCDFPalette'.split())
def digest(v):return hashlib.sha256(json.dumps(v,sort_keys=True,ensure_ascii=False).encode()).hexdigest()
def decisions(group=None):return [d for g in 'ABCDE' if group is None or group==g for d in a.read(H/('group-'+g+'-control-evidence.json'))]
def target_errors(targets):
 errors=[];f=a.read(H/'frozen-source.json');m=targets['members'];c=[c for g in targets['groups'] for c in g['controls']]
 errors+=exact_ids(m,{s['symbol_id'] for s in f['members']},'symbol_id')
 if len(m)!=2 or {s['name'] for s in m}!={'Dispose','InitializeComponent'}:errors.append('Incorrect two Designer bodies')
 if [(g['group'],len(g['controls'])) for g in targets['groups']]!=[(g,39) for g in 'ABCDE']:errors.append('Five39 partition drift')
 errors+=exact_ids(c,{x['control_id'] for x in f['full_form_controls']},'control_id')
 if c!=sorted(f['full_form_controls'],key=lambda c:(int(c['line']),c['control_id'])):errors.append('Declaration order/default/ID drift')
 for s in m:
  expected=next((x for x in f['members'] if x['symbol_id']==s['symbol_id']),None)
  if s!=expected:errors.append('Swapped/stale Designer body/span')
 return errors
def source_errors(symbols,members):
 f=a.read(H/'frozen-source.json');si={s['symbol_id']:s for s in symbols}
 errors=exact_ids(members,{s['symbol_id'] for s in f['members']},'symbol_id')
 for s in f['full_form_members']:
  now=si.get(s['symbol_id'])
  if now is None or any(now.get(k)!=s[k] for k in IDENTITY+('kind','profile','span_start','span_length')):errors.append('Missing/stale mode/handler/delegate source '+s['symbol_id'])
 expected={s['symbol_id']:si[s['symbol_id']] for s in f['members'] if s['symbol_id'] in si}
 if len(expected)==2:errors+=member_errors(members,expected,{s['symbol_id']:s for s in f['members']},a.read(H/'member-entry-evidence.json'))
 return errors
def admission(rows,expected,contexts,frozen):
 errors=control_errors(rows,expected,contexts,frozen);cm={c['control_id']:c for c in contexts}
 mode=a.read(H/'mode-policy.json');mp=H/'mode-policy.json'
 for d in rows:
  ctx=cm.get(d['control_id'])
  if ctx is None:continue
  if not ctx.get('creation') or not ctx.get('ancestor_chain') or ctx.get('mode_policy_pin',{}).get('sha256')!=a.sha(mp):errors.append('Unpinned creation/Me.Tag/tab reparenting/mode')
  if not mode['constructor_lines'] or len(mode['modes'])!=16:errors.append('Missing mode branches')
  if not d.get('acceptance_status') or not d.get('owner_approval_marker'):errors.append('Missing explicit acceptance/approval marker')
  if d.get('review_contract')=='04A.01-control-v1' and any(e['kind'] in ('Handles','AddHandler') for e in ctx['associated_edges']):errors.append('Unsupported active control promotion')
  if semantic.control_qualified(d) and d['control_id'].split('.')[-1] not in QUALIFIED_NAMES:errors.append('No individual source-qualified static adjudication for this control')
  if d.get('intent_completeness')=='deferred' and (d.get('review_status')!='deferred' or not all(d.get(k) for k in ('unsupported_contract_field','future_batch','verification','owner_role'))):errors.append('Unowned or falsely credited deferral')
 return errors
def proposal_errors(policy):
 return [] if policy.get('status')=='PROPOSAL ONLY; NOT APPROVED' and policy.get('owner_approval_marker')=='PENDING' and policy.get('strict_profile_exit_while_open')==2 and policy.get('implementation_authorized') is False else ['Proposal cannot confer acceptance or strict zero']
def wiring_errors(rows,edges,symbols):
 errors=exact_ids(rows,{e['edge_id'] for e in edges},'edge_id');si={s['symbol_id'] for s in symbols};ei={e['edge_id']:e for e in edges}
 for row in rows:
  if any(row.get(k)!=ei.get(row['edge_id'],{}).get(k) for k in EDGE):errors.append('Scoped wiring pin drift')
  if row['kind'] in ('Handles','AddHandler') and (not row.get('handler_source_ids') or not set(row['handler_source_ids'])<=si):errors.append('Missing genuine handler/delegate dependency')
 return errors
def verify_current(symbols,controls,edges,store):
 errors=target_errors(a.read(H/'targets.json'));members=[d for d in store if d.get('checkpoint_id')=='04A.01-continuation-27'];errors+=source_errors(symbols,members)
 if members!=a.read(H/'member-reviews.json'):errors.append('C27 authored member/store drift')
 rows=decisions();f=a.read(H/'frozen-source.json');errors+=admission(rows,{c['control_id']:c for c in f['full_form_controls']},a.read(H/'control-contexts.json'),f)
 ci={c['control_id']:c for c in controls};ei={e['edge_id']:e for e in edges}
 for c in f['full_form_controls']:
  if any(ci.get(c['control_id'],{}).get(k)!=c[k] for k in CONTROL):errors.append('Current control declaration/default/Handles drift')
 for e in f['full_form_wiring']:
  if any(ei.get(e['edge_id'],{}).get(k)!=e[k] for k in EDGE):errors.append('Current scoped edge drift')
 intents={d['control_id']:d for d in semantic.read_store('control-review-decisions.json')}
 for d in rows:
  if semantic.control_qualified(d) and intents.get(d['control_id'])!=d:errors.append('C27 qualified authored/store drift')
 errors+=proposal_errors(a.read(H/'acceptance-policy-proposal.json'))
 return errors
def guard(history=False,group=None):
 b=a.read(H/'before.json');errors=target_errors(a.read(H/'targets.json'))
 assert a.git('branch','--show-current')==b['branch'] and a.git('rev-parse','HEAD')==a.git('rev-parse','main')==b['head']
 assert not a.git('diff','--name-only') and not a.git('diff','--cached','--name-only')
 for p in b['decision_prefixes']:
  name=p['store']+'.json';old=(H/'original/scripts/windows-migration-audit'/name).read_bytes();new=(T/name).read_bytes();errors+=prefix_errors(old,new,p['objects'])
  if p['store']=='wiring-review-decisions' and new!=old:errors.append('Unexpected wiring-store change')
 errors+=checked_files(a.read(H/'scope-freeze-manifest.json'))
 for label in ('tracked','gate','compiled'):errors+=checked_files(b[label])
 errors+=checked_files(b['original_artifacts'],H/'original')
 for e in b['baseline']['entries']:
  if a.sha(R/e['retained_path'])!=e['sha256']:errors.append('Immutable baseline drift')
 for e in b['original_artifacts']:
  name=pathlib.PurePosixPath(e['path']).name
  if '/baseline/' in e['path'] or any(name.startswith('continuation'+str(n).zfill(2)) for n in range(1,27)):errors+=checked_files([e])
 for g in 'ABCDE' if group is None else group:
  errors+=checked_files(a.read(H/('checkpoint-'+g+'.json'))['immutable_evidence'])
  f=a.read(H/'frozen-source.json');expected={c['control_id']:c for x in a.read(H/'targets.json')['groups'] if x['group']==g for c in x['controls']}
  errors+=admission(decisions(g),expected,a.read(H/'control-contexts.json'),f)
 if history:
  manifest=a.read(H/'prior-raw-evidence-hashes.json')['files'];errors+=checked_files(manifest)
  actual={p.relative_to(R).as_posix() for root in (R/'TestResults').glob('04A-01*') if root!=H for p in root.rglob('*') if p.is_file()}
  expected={e['path'] for e in manifest if e['path'].split('/')[1].startswith('04A-01')}
  if actual!=expected:errors.append('Historical path membership drift')
 assert not errors,errors
 return dict(status='PASS',history_checked=history,historical_paths=b['historical_paths'],protected_counts={k:len(b[k]) for k in ('tracked','gate','compiled')},old_index_artifacts=541,old_index_sha256=b['index_sha256'])
def render(check=False):
 errors=verify_current(catalog.load(L,'symbols.csv'),catalog.load(L,'ui-controls.csv'),catalog.load(L,'ui-wiring.csv'),semantic.read_store('review-decisions.json'));assert not errors,errors
 sem=a.read(L/'semantic-review-coverage.json');assert sem['qualified_source_reviews']==3866 and sem['unreviewed_for_acceptance']==0
 assert catalog.load(L,'remaining-member-review.csv')==[]
 controls=decisions();q=sum(semantic.control_qualified(d) for d in controls);f=a.read(H/'frozen-source.json');cs={c['control_id']:c for c in f['full_form_controls']};symbols={s['symbol_id']:s for s in catalog.load(L,'symbols.csv')}
 def write(name,raw):
  if isinstance(raw,str):raw=(raw.rstrip()+'\n').encode()
  p=L/name
  if check:assert p.read_bytes()==raw,'C27 report drift '+name
  else:p.write_bytes(raw)
 def emit(name,v):write(name,json.dumps(v,indent=2,ensure_ascii=False))
 def csvout(name,rows,fields=None):
  if fields:
   import io
   stream=io.StringIO(newline='');w=csv.DictWriter(stream,fields,quoting=csv.QUOTE_ALL,lineterminator='\n');w.writeheader();w.writerows(rows);write(name,stream.getvalue())
  else:write(name,catalog.render([{k:json.dumps(v,sort_keys=True,ensure_ascii=False) if isinstance(v,(dict,list)) else v for k,v in r.items()} for r in rows]))
 with (L/'symbols.csv').open(encoding='utf-8-sig',newline='') as stream:header=next(csv.reader(stream))
 assert len(header)==81
 csvout('continuation27-member-reviews.csv',[{**symbols[d['symbol_id']],**d} for d in a.read(H/'member-reviews.json')],header)
 csvout('continuation27-control-contracts.csv',[{**cs[d['control_id']],**d} for d in controls])
 csvout('continuation27-control-adjudications.csv',[{k:d[k] for k in ('control_id','review_status','review_contract','reviewed_intent','intent_completeness','qualification_reason','unsupported_contract_field','source_control_references','owner_role','future_batch','verification','confidence','acceptance_status','owner_approval_marker')} for d in controls])
 cross=[]
 for e in f['full_form_wiring']:
  owners=[s['symbol_id'] for s in f['full_form_members'] if s['path']==e['path'] and int(s['start_line'])<=int(e['start_line'])<=int(s['end_line'])]
  cross.append(dict(e,handler_source_ids=owners,associated_control_ids=[d['control_id'] for d in controls if e['edge_id'] in json.loads(d['associated_control_edge_ids'])],adjudication='DEFERRED global intent' if 'unresolved' in e['target_status'] else 'Existing static resolution retained; runtime not proven',owner_role='Windows UibyID wiring owner',future_batch='04A.45; 04A.46',fixture='C27-edge-'+e['edge_id']+': source association then fake-host lifecycle/dispatch; NOT RUN'))
 assert not wiring_errors(cross,f['full_form_wiring'],f['full_form_members'])
 csvout('continuation27-wiring-crosswalk.csv',cross)
 csvout('continuation27-mode-control-matrix.csv',a.read(H/'mode-control-matrix.json'))
 csvout('continuation27-gui-core-alignment.csv',a.read(H/'alignment-reviewed.json'))
 csvout('continuation27-fixture-crosswalk.csv',[dict(source_id=d.get('control_id',d.get('symbol_id')),fixture=d.get('verification',d.get('proposed_test')),owner=d['owner_role'],future_batch=d['future_batch'],status='NOT RUN') for d in a.read(H/'member-reviews.json')+controls])
 csvout('continuation27-portable-comparisons.csv',a.read(H/'portable-declarations.json'))
 prior=a.read(H/'prior-codebehind-operation-supplement.json')
 csvout('continuation27-operation-members.csv',prior['members'])
 csvout('continuation27-candidate-operations.csv',prior['groups'])
 emit('continuation27-targets.json',a.read(H/'targets.json'))
 emit('continuation27-count-deltas.json',dict(before=dict(members=3864,ui_members=1545,qualified_controls=1129,incomplete_controls=927,structural_investigations=0,unresolved_wiring=622),after=dict(members=3866,ui_members=1547,qualified_controls=1129+q,incomplete_controls=927-q,structural_investigations=0,unresolved_wiring=sum(sem['remaining_unresolved_edges_by_kind'].values())),scoped_controls=195,new_qualified=q,new_deferred=195-q,source_denominator=3866,ui_denominator=1547,control_denominator=2056,remaining_member_ids=[],new_member_candidates=0,new_deduplicated_operations=0,prior_codebehind_candidates='Existing C19/C20 decisions retained; see prior-codebehind-reviews.json; no duplicate credit',strict_exit_expected=2,acceptance='NOT READY',gate_c2='UNMET'))
 for suffix,rawname in [('preservation','preservation-summary.json'),('validation','validation-summary.json')]:emit('continuation27-'+suffix+'.json',a.read(H/rawname))
 emit('continuation27-acceptance-policy-proposal.json',a.read(H/'acceptance-policy-proposal.json'))
 emit('continuation27-deferral-record-schema-proposal.json',a.read(H/'deferral-record-schema-proposal.json'))
 write('continuation27-acceptance-amendment-proposal.md',(H/'acceptance-amendment-proposal.md').read_bytes())
 write('continuation27.md',(H/'narrative.md').read_bytes())
 files=[p for p in L.glob('continuation27*') if p.name!='continuation27-artifacts.csv']+[T/'continuation27_report.py',T/'test_continuation27.py']
 csvout('continuation27-artifacts.csv',[dict(path=p.relative_to(R).as_posix(),bytes=p.stat().st_size,sha256=a.sha(p)) for p in sorted(files)])
 members={p.relative_to(R).as_posix() for p in L.rglob('*') if p.is_file()}|{p.relative_to(R).as_posix() for p in T.iterdir() if p.is_file()}|{'docs/architecture/WindowsMigrationLedger.md'}
 proposal=a.read(L/'proposed-tracked-files.json');proposal['paths']=sorted(members);proposal['comparison']='C27 exact future staging proposal only; no staging executed';emit('proposed-tracked-files.json',proposal)
 write('future-staging.txt','# PROPOSAL ONLY; DO NOT EXECUTE WITHOUT OWNER APPROVAL\n'+'\n'.join(('git add -f -- ' if p.startswith('docs/architecture/WindowsMigrationLedger/') else 'git add -- ')+json.dumps(p) for p in sorted(members)))
 print(json.dumps(dict(members=3866,controls=195,qualified=q,deferred=195-q,strict_exit_expected=2,status='AUDIT CHECKPOINT; NOT ACCEPTANCE')))
def main():
 p=argparse.ArgumentParser(__doc__);p.add_argument('--check',action='store_true');p.add_argument('--guard',action='store_true');p.add_argument('--history',action='store_true');p.add_argument('--group',choices=list('ABCDE'));args=p.parse_args()
 if args.guard:print(json.dumps(guard(args.history,args.group)))
 else:render(args.check)
if __name__=='__main__':main()
