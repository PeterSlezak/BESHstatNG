from common47 import *
import csv,collections,re,shutil,stat
lock();targets=['UiGLM','Ui13GEE','Ui18MMRM','Ui19LMM','Ui11PCA','Ui20PropensityScoreMatching','Ui21ControlCharts','Ui22MultivariateControlCharts','Ui4Cox','Ui4KMandLogRank']
prior=ROOT/'.test-rationalization-04A45.tmp/syntax-current-v3'
def table(n):return list(csv.DictReader((prior/n).open(encoding='utf-8-sig',newline='')))
old=load(ROOT/'evidence/04A45/scope/controls-v2.json');sources={r['path']:r for r in old['sources']}
for r in sources.values():exact(r)
owners={'BESHStatNG.'+n for n in targets};symbols=table('symbols.csv');fields=table('state.csv');edges=table('ui-wiring.csv');members=[]
for r in symbols:
 if r['containing_type'] not in owners:continue
 p=r['path'];exact(sources[p]);body='\n'.join((ROOT/p).read_text(encoding='utf-8-sig').splitlines()[int(r['start_line'])-1:int(r['end_line'])])
 members.append(dict(member_id=r['symbol_id'],owner=r['containing_type'],name=r['name'],kind=r['kind'],signature=r['signature'],path=p,start=int(r['start_line']),end=int(r['end_line']),body_sha256=r['body_sha256'],source_sha256=pin(p)['sha256'],normalized_line_body_sha256=sha(body.encode()),handles=[e['edge_id'] for e in edges if e['owner_id']==r['symbol_id'] and e['kind']=='Handles'],branch_conditions=re.findall(r'^\s*(?:If|ElseIf|Case) (.+)',body,re.M),state_targets=sorted(set(re.findall(r'Me\.([A-Za-z0-9_]+)\.(?:Enabled|Visible|Checked|Value|Text|SelectedIndex)',body))),classification='WINDOWS_HOST_NATIVE_PENDING',reason='Pre-edit source/compiled reference;not yet safely executed. Native acquisition/compute/dialog/writer paths never invoked.',pre_edit_materialization='UNAVAILABLE_BECAUSE_NATIVE_GATE' if re.search(r'\b(ExcelDnaDataImporter|Worksheet|Workbook|Range|MsgBox|ShowDialog|CreateChart|GetResultWriter)\b',body) else 'SOURCE_DERIVED_FAKE_ELIGIBLE',evidence='evidence/04A47/entry/compiled-identities-v1.json'))
selected_fields=[r for r in fields if r['containing_type'] in owners];selected_edges=[r for r in edges if r['containing_type'] in owners]
obligations=load(ROOT/'evidence/04A45/scope/original-obligations-v1.json')['rows'];original=[r for r in obligations if any(r['original_id'].startswith(o+'.') for o in owners)]
save('scope/family-census-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A47',owners=targets,members=members,fields=selected_fields,edges=selected_edges,original_obligations=original,counts=dict(owners=len(targets),members=len(members),fields=len(selected_fields),candidate_edges=len(selected_edges),obligations=len(original)),all45_census=dict(owners=30,fields=2386,edges=19596,chains=599,callbacks=85),production_sources=list({p:pin(p) for p in {r['path'] for r in members}}.values()),outside47=['Ui11PCA','UiGLM','Ui13GEE','Ui18MMRM','Ui19LMM','Ui20PropensityScoreMatching','Ui21ControlCharts','Ui22MultivariateControlCharts','Ui4Cox','Ui4KMandLogRank','Ui99ExportChart','UiUpdateAvailable'],coverage_rule='Every source element remains native pending unless a named event/assertion fixture proves software execution. Declaration/configuration is not event subscription.',native='NOT_RUN',gates=GATES))
print('Pre-edit family census:',len(targets),'owners;',len(members),'members;',len(selected_fields),'fields;',len(selected_edges),'candidate edges;',len(original),'original obligations',flush=True)
run=runner();compiled=[]
for c in ['Debug','Release']:
 dest=PRIVATE/'entry-builds'/c/'lib';shutil.copytree(ROOT/('.test-rationalization-04A46.tmp/builds/a46-final-v1/'+c+'/legacy'),dest)
 for p in dest.rglob('*'):
  if p.is_file():p.chmod(stat.S_IREAD|stat.S_IWRITE)
 r=command('entry-'+c+'-host',[run.msbuild,'BESHstatNG/BESHstatNG.vbproj','/t:PrepareForBuild;PrepareResources;Compile;CopyFilesToOutputDirectory','/p:Configuration='+c,'/p:BuildProjectReferences=false','/p:OutputPath='+str(dest)+'\\','/p:IntermediateOutputPath='+str(PRIVATE/'entry-obj'/c)+'\\','/m:4','/v:minimal','/nr:false']);assert not r['exit_code']
 out=dest.parent/'wiring.json';exe=ROOT/'.test-rationalization-04A45.tmp/wiring-inspector-v2/InspectWiring45.exe'
 r=command('entry-'+c+'-metadata',[str(exe),str(dest),str(out)]);assert not r['exit_code']
 before=load(ROOT/('evidence/04A45/scope/compiled-wiring-'+c+'-v2.json'));now=load(out);assert before['types']==now['types']
 compiled.append(dict(configuration=c,current=pin(out),assembly=pin(dest/'BESHStatNG.dll'),MVID=now['mvid'],all_UI_fields_signatures_and_IL_exact_accepted45=True,baseline=pin(ROOT/('evidence/04A45/scope/compiled-wiring-'+c+'-v2.json'))))
save('entry/compiled-identities-v1.json',dict(kind='CURRENT_MAIN_PRE_EDIT_04A47_ACTUAL_COMPILE_AND_METADATA',rows=compiled,scope='Windows compile-only graph reuses authenticated unchanged Core/Common45 dependencies;no product/UI initialization or XLL packaging',native='NOT_RUN'))
