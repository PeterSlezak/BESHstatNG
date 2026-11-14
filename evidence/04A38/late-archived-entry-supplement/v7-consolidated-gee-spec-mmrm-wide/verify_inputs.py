"""Independent Approval G/source/CLR packet reader; no producer imports."""
import hashlib,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4];PREFIX='evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide/'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d';INPUT_SEAL='0cce11e2b2de60c5691df4d52d00d535f74df7b1526f63b7f81dc9451751dcdf'
KINDS=['GEE_SPEC_INTERCEPT_ROLE','MMRM_WIDE_MULTIROW_REJECTION']
QUOTE=('I approve the consolidated four-case-per-configuration archived-entry supplement for Phase 04A.38, covering the GEE `spec` intercept-role rejection at original lines 1506–1509 and the MMRM wide-profile multiple-row rejection at original lines 592–594, once per Invariant and sk-SK culture in Debug and Release.\n\n'
 'Codex must authenticate the original models and sources, seal typed inputs and original plain/repeat/trace outputs before current comparisons, and prove the actual original guards. Preserve all earlier evidence and stop for separate approval if another essential gap emerges. If both gates pass, continue W2 and W3 of the existing 04A.38 plan.')
class InvalidEvidence(ValueError):pass
def need(v,m):
 if not v:raise InvalidEvidence(m)
def digest(b):return hashlib.sha256(b).hexdigest()
def load(p):
 def unique(pairs):
  d={}
  for k,v in pairs:need(k not in d,'Duplicate JSON key');d[k]=v
  return d
 return json.loads(p.read_bytes(),object_pairs_hook=unique)
def check_pin(root,row):
 p=(root/row['path']).resolve();need(p.is_relative_to(root.resolve()),'Escaping path');b=p.read_bytes();need(len(b)==row['bytes'] and digest(b)==row['sha256'],'Changed protected bytes: '+row['path'])
def arr(r,c,values):return dict(kind='Array',element_type='System.Object',rank=2,lengths=[r,c],lower_bounds=[0,0],upper_bounds=[r-1,c-1],values=values)
def S(s):return dict(kind='String',value=s)
def D(b):return dict(kind='Double',bits=b)
def inspect(root=ROOT):
 root=Path(root);here=root/PREFIX;need(digest((here/'input-seal.json').read_bytes())==INPUT_SEAL,'Modified input seal')
 seal=load(here/'input-seal.json');need(seal['status']=='INPUT_SEALED' and seal['sequence']==1 and seal['source_commit']==BASE and seal['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT' and seal['cases_per_configuration']==4,'Incorrect input source/chronology/scope')
 need(seal['original_executions']==seal['current_executions']==0 and seal['not_preedit_frozen'] is True,'Post-execution seal/preedit claim')
 for p in seal['files']:check_pin(root,p)
 approval=load(here/'owner-authorization.json');need(approval['approval']=='G' and approval['owner_quote']==QUOTE and approval['owner_decision_date']=='2026-11-09','Missing/fake Approval G')
 need(approval['cases_per_configuration']==4 and approval['case_kinds']==KINDS and approval['cultures']==['Invariant','sk-SK'],'Unapproved case matrix')
 for key in ['approvalA','approvalB','approvalC','approvalD','approvalE','approvalF','preceding_proposal']:check_pin(root,approval[key])
 factories={f:load(here/('factory-'+f+'.json')) for f in ['GEE','MMRM']}
 for f,old in [('GEE','v2-gee-profile'),('MMRM','v3-mmrm-headered-profile')]:need((here/('factory-'+f+'.json')).read_bytes()==(root/'evidence/04A38/late-archived-entry-supplement'/old/'factory-inputs.json').read_bytes(),'Changed original factory '+f)
 for config in ['Debug','Release']:
  cases=list(map(json.loads,(here/('inputs-'+config+'.jsonl')).read_bytes().splitlines()));need([c['case_id'] for c in cases]==['AUTHORIZED-G/'+culture+'/'+kind for culture in ['Invariant','sk-SK'] for kind in KINDS],'Missing/extra/reordered case IDs')
  mat=load(here/('materialized-'+config+'.json'));need(mat['model_executions']==mat['query_executions']==0 and mat['culture_restored'] is True and len(mat['cases'])==4,'Missing real .NET pre-execution materialization')
  for i,c in enumerate(cases):
   f='GEE' if i%2==0 else 'MMRM';need(c['case_kind']==KINDS[i%2] and c['culture']==c['ui_culture']==('' if i<2 else 'sk-SK') and c['configuration']==config and c['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT','Wrong kind/culture/configuration/provenance')
   fit,query=c['operations'];need(fit==dict(role='PUBLIC_EXPORT',target='BESH.REGR.'+f+'_FIT',arguments=factories[f]['fit_arguments']),'Fake/changed fitted-model input')
   names=['handle','spec','covarianceType','alpha','at','scale'] if f=='GEE' else ['handle','spec','alpha','at'];need(query['target']=='BESH.REGR.'+f+'_LSMESTIMATE' and [p['name'] for p in query['arguments']]==names,'Wrong public method/argument order')
   args=[p['value'] for p in query['arguments']];need(args[0]==dict(kind='HandleReference',session='fresh-case'),'Fake handle')
   if f=='GEE':
    need(args[1]==arr(2,3,[S('Label'),S('Weight'),S('Intercept'),S('L'),D('3FF0000000000000'),D('3FF0000000000000')]) and args[4]==dict(kind='Nothing'),'Wrong GEE typed spec/at')
    defaults={p['name']:p['value'] for p in factories[f]['query_defaults']}
    need(all(args[names.index(n)]==defaults[n] for n in ['covarianceType','alpha','scale']),'Changed GEE defaults')
   else:need(args[1]==factories[f]['spec'] and args[2]==dict(kind='Nothing') and args[3]==arr(3,1,[S('visit'),D('3FF0000000000000'),D('4000000000000000')]),'Wrong MMRM typed spec/at/alpha')
   typed=mat['cases'][i];need(typed['case_id']==c['case_id'] and typed['culture']==typed['ui_culture']==c['culture'] and typed['query_runtime_types'][0]=='System.Object[,]','Wrong actual CLR materialization')
   need(c['expected_result']=='UNKNOWN_NOT_CAPTURED','Invented original expected output')
 entry=load(here/'entry-inspection.json');need(len(entry['original_tree_files'])==4695 and len(entry['udf_snapshots'])==41 and entry['source_commit']==BASE,'Wrong original source graph')
 for row in entry['udf_snapshots']:check_pin(root,row['snapshot'])
 for path,size,sha in [('Regression/GeeUDFs.vb.txt',114843,'35a3a35ddf4dfdb074a082dfe92495969dd1a8532f86af6650d6bde33bcf7649'),('UDFdataImport/UdfDataImportMixedModel.vb.txt',38595,'5297574cab59e631cf880807903a206f76dfb1509818a1825c1ca6798332071a')]:
  b=(root/'evidence/04A38/preedit/source'/path).read_bytes();need(len(b)==size and digest(b)==sha,'Original UDF snapshot mismatch')
 for config in ['Debug','Release']:
  built=load(here/('build-original-'+config+'.json'));need(built['source_commit']==BASE and built['current_production_assemblies_consumed'] is False,'Current dependencies in original')
  info=load(here/('compile-original-'+config+'-trace.json'));maps=info['instrumentation']
  for event,line,after in [('G1.guard.intercept',1507,True),('G2.guard.multirow',592,True),('G2.headerless.column_guard_true',652,False)]:
   rows=[r for r in maps if r['event']==event];need(len(rows)==1 and rows[0]['archived_source_line']==line and rows[0]['after_anchor'] is after,'Missing/wrong source-path witness '+event)
 return dict(status='G_TYPED_SOURCE_INPUT_SEAL_PASS',cases_per_configuration=4)
if __name__=='__main__':print(json.dumps(inspect()))
