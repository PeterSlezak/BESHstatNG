"""Independent fixed-authority/typed-source reader. No generating harness imports."""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4]
PREFIX='evidence/04A38/late-archived-entry-supplement/v5-mmrm-unmatched-profile/'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
INPUT_SEAL='4d8621915832c5a795838e9b2282b75bf75503e6d1dd98b1fa97cb098953c2cb'
FACTORY='7f0bf7491d95b6214420b6ee502694b11f6d69d0964dd33089a39fad9c4b4ded'
QUOTE=('I approve the additional two-case-per-configuration MMRM unmatched-profile archived-entry supplement for Phase 04A.38, limited to `MMRM_LSMEANS` with the wide-form `visit=99.0` profile, one case in Invariant and one in sk-SK, in both Debug and Release.\n\n'
 'The original source and typed inputs must be authenticated and sealed, the actual original no-matching-rows guard must be reached, and original outputs must be sealed before current-code comparisons. Preserve all previous evidence and stop for separate authorization if another essential reference gap emerges.')
KIND='mmrm-lsmeans-wide-visit-99-unmatched'
class InvalidEvidence(ValueError):pass
def need(value,message):
 if not value:raise InvalidEvidence(message)
def digest(data):return hashlib.sha256(data).hexdigest()
def load(path):
 def unique(pairs):
  d={}
  for k,v in pairs:need(k not in d,'Duplicate JSON key');d[k]=v
  return d
 return json.loads(path.read_bytes(),object_pairs_hook=unique)
def check_pin(root,row):
 p=(root/row['path']).resolve();need(p.is_relative_to(root.resolve()),'Escaping evidence path')
 data=p.read_bytes();need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Frozen source/input field changed: '+row['path'])
def inspect(root=ROOT):
 root=Path(root);here=root/PREFIX
 need(len(INPUT_SEAL)==64 and digest((here/'input-seal.json').read_bytes())==INPUT_SEAL,'Changed input seal')
 seal=load(here/'input-seal.json');need(seal['status']=='INPUT_SEALED' and seal['sequence']==1 and seal['source_commit']==BASE and seal['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT','Wrong original/input/late identity')
 need(seal['original_executions']==seal['current_executions']==0 and seal['not_preedit_frozen'] is True and seal['cases_per_configuration']==2,'False input seal chronology/scope')
 for row in seal['files']:check_pin(root,row)
 approval=load(here/'owner-authorization.json')
 need(approval['approval']=='E' and approval['owner_quote']==QUOTE and approval['owner_decision_date']=='2026-11-09','Missing/fake separate Approval E')
 need(approval['cases_per_configuration']==2 and approval['case_kinds']==[KIND] and approval['cultures']==['Invariant','sk-SK'] and approval['configurations']==['Debug','Release'],'Unapproved matrix')
 need(approval['public_entry']=='BESH.REGR.MMRM_LSMEANS','Wrong approved family')
 for key in ['preceding_proposal','approvalA','approvalB','approvalC','approvalD']:check_pin(root,approval[key])
 need(digest((here/'factory-inputs.json').read_bytes())==FACTORY,'Changed successful original factory')
 factory=load(here/'factory-inputs.json')
 need((here/'factory-inputs.json').read_bytes()==(root/'evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile/factory-inputs.json').read_bytes(),'Factory differs from successful original')
 for config in ['Debug','Release']:
  cases=[json.loads(line) for line in (here/('inputs-'+config+'.jsonl')).read_bytes().splitlines()]
  need([c['case_id'] for c in cases]==['AUTHORIZED-MMRM-UNMATCHED/'+culture+'/'+KIND for culture in ['Invariant','sk-SK']],'Missing/extra/reordered case identities')
  materialized=load(here/('materialized-'+config+'.json'))
  need(materialized['status']=='MATERIALIZED_NO_MODEL_OR_QUERY_EXECUTION' and materialized['model_executions']==materialized['query_executions']==0 and materialized['culture_restored'] is True,'No genuine pre-execution materialization')
  need(len(materialized['cases'])==2,'Lost materialized case')
  for i,c in enumerate(cases):
   need(c['culture']==c['ui_culture']==('' if i==0 else 'sk-SK') and c['configuration']==config,'Wrong culture/UI/configuration')
   need(c['case_kind']==KIND and c['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT' and len(c['operations'])==2,'Wrong kind/reference/count')
   fit,query=c['operations'];need(fit==dict(role='PUBLIC_EXPORT',target='BESH.REGR.MMRM_FIT',arguments=factory['fit_arguments']),'Wrong/foreign fitted-model inputs')
   need(query['target']=='BESH.REGR.MMRM_LSMEANS' and [a['name'] for a in query['arguments']]==['handle','group','alpha'],'Wrong public path/order')
   args=[a['value'] for a in query['arguments']]
   need(args[0]==dict(kind='HandleReference',session='fresh-case') and args[2]==dict(kind='Nothing'),'Fake handle/changed alpha')
   need(args[1]==dict(kind='Array',element_type='System.Object',rank=2,lengths=[2,1],lower_bounds=[0,0],upper_bounds=[1,0],values=[dict(kind='String',value='visit'),dict(kind='Double',bits='4058C00000000000')]),'Not exact zero-based typed Object2x1 visit99')
   typed=materialized['cases'][i]
   need(typed['case_id']==c['case_id'] and typed['culture']==typed['ui_culture']==c['culture'] and typed['group_runtime_type']=='System.Object[,]','Wrong actual .NET array/culture')
   need(typed['rank']==2 and typed['lengths']==[2,1] and typed['lower_bounds']==[0,0] and typed['upper_bounds']==[1,0] and typed['cell_types']==['System.String','System.Double'] and typed['group']=='System.Object[0:2,0:1]{SdmlzaXQ=|D4058C00000000000}' and typed['alpha']=='N','Materialized types/bounds/bits differ')
   need(c['session']['expected_parameter_names']==['Intercept','treatment_active','visit2','visit3','treatment_active:visit2','treatment_active:visit3'] and c['session']['required_fitted_visit_histogram']=={'1.0':18,'2.0':18,'3.0':18},'Lost original fitted context')
 entry=load(here/'entry-inspection.json');need(entry['source_commit']==BASE and len(entry['original_tree_files'])==4695 and len(entry['udf_snapshots'])==41,'Original provenance incomplete')
 for row in entry['udf_snapshots']:check_pin(root,row['snapshot'])
 source=next(r for r in entry['udf_snapshots'] if r['path'].endswith('/MixedModelUDFs.vb'))
 need(source['snapshot']['bytes']==92704 and source['snapshot']['sha256']=='596071f9fdb00fb94585e929a6899494293d2dc7ee0064faadf098b0d95dc13d','Wrong original mask snapshot')
 for config in ['Debug','Release']:
  built=load(here/('build-original-'+config+'.json'));need(built['current_production_assemblies_consumed'] is False and built['source_commit']==BASE,'Current dependencies in original build')
  info=load(here/('compile-original-'+config+'-trace.json'))
  guards=[r for r in info['instrumentation'] if r['event']=='guard.unmatched']
  need(len(guards)==1 and guards[0]['member']=='TryBuildMmrmAtProfileRowMask' and guards[0]['archived_source_line']==1426 and guards[0]['source_anchor']=='If matchedCount <= 0 Then' and guards[0]['after_anchor'] is True,'Missing original1426 true-arm probe')
 return dict(status='UNMATCHED_TYPED_INPUT_SOURCE_SEAL_PASS',cases_per_configuration=2)
if __name__=='__main__':print(json.dumps(inspect()))
