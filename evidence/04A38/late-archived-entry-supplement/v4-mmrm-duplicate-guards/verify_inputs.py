"""Independent fixed-authority/source/type reader; no producer imports."""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4]
PREFIX='evidence/04A38/late-archived-entry-supplement/v4-mmrm-duplicate-guards/'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
INPUT_SEAL='5a78297ef1b45c95f3c4f23dd079a9bcc55ec79c067f1ee27465f6a50bb59708'
FACTORY='7f0bf7491d95b6214420b6ee502694b11f6d69d0964dd33089a39fad9c4b4ded'
QUOTE='I approve the additional four-case-per-configuration MMRM duplicate-guard archived-entry supplement for Phase 04A.38, limited to the two specified guards in Invariant and sk-SK. Please prepare next codex prompt'
KINDS=['duplicate-visit-time','duplicate-fitted-column']
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
 need(seal['original_executions']==seal['current_executions']==0 and seal['not_preedit_frozen'] is True,'False seal chronology')
 for row in seal['files']:check_pin(root,row)
 approval=load(here/'owner-authorization.json')
 need(approval['approval']=='D' and approval['owner_quote']==QUOTE and approval['owner_decision_date']=='2026-11-09','Missing/fake separate Approval D')
 need(approval['cases_per_configuration']==4 and approval['case_kinds']==KINDS and approval['cultures']==['Invariant','sk-SK'],'Unapproved matrix')
 need(digest((here/'factory-inputs.json').read_bytes())==FACTORY,'Changed successful original factory/spec')
 factory=load(here/'factory-inputs.json')
 need((here/'factory-inputs.json').read_bytes()==(root/'evidence/04A38/late-archived-entry-supplement/v3-mmrm-headered-profile/factory-inputs.json').read_bytes(),'Factory not exact v3 original')
 for config in ['Debug','Release']:
  cases=[json.loads(line) for line in (here/('inputs-'+config+'.jsonl')).read_bytes().splitlines()]
  need([c['case_id'] for c in cases]==['AUTHORIZED-MMRM-DUPLICATE/'+culture+'/'+kind for culture in ['Invariant','sk-SK'] for kind in KINDS],'Missing/extra/reordered case identities')
  for i,c in enumerate(cases):
   need(c['culture']==c['ui_culture']==('' if i<2 else 'sk-SK') and c['configuration']==config,'Wrong culture/UI/configuration')
   need(c['case_kind']==KINDS[i%2] and c['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT' and len(c['operations'])==2,'Wrong kind/reference/family count')
   fit,query=c['operations'];need(fit==dict(role='PUBLIC_EXPORT',target='BESH.REGR.MMRM_FIT',arguments=factory['fit_arguments']),'Wrong/foreign fitted-model inputs')
   need(query['target']=='BESH.REGR.MMRM_LSMESTIMATE' and [a['name'] for a in query['arguments']]==['handle','spec','alpha','at'],'Wrong public argument order/path')
   args=[a['value'] for a in query['arguments']]
   need(args[:3]==[dict(kind='HandleReference',session='fresh-case'),factory['spec'],dict(kind='Nothing')],'Fake handle/changed spec/default alpha')
   a=args[3];need(a['kind']=='Array' and a['element_type']=='System.Object' and a['rank']==2 and a['lengths']==[3,2] and a['lower_bounds']==[0,0] and a['upper_bounds']==[2,1],'Not zero-based rank2 Object3x2')
   names=['visit','time'] if i%2==0 else ['treatment_active','TREATMENT_ACTIVE']
   expected=[dict(kind='String',value='Name'),dict(kind='String',value='Value'),dict(kind='String',value=names[0]),dict(kind='Double',bits='3FF0000000000000'),dict(kind='String',value=names[1]),dict(kind='Double',bits='4000000000000000')]
   need(a['values']==expected,'Wrong header/cell type/case/IEEE bits/row order')
   need(c['session']['expected_parameter_names']==['Intercept','treatment_active','visit2','visit3','treatment_active:visit2','treatment_active:visit3'],'Missing real fixed-design provenance')
 entry=load(here/'entry-inspection.json');need(entry['source_commit']==BASE and len(entry['original_tree_files'])==4695 and len(entry['udf_snapshots'])==41,'Original provenance incomplete')
 for row in entry['udf_snapshots']:check_pin(root,row['snapshot'])
 source=next(r for r in entry['udf_snapshots'] if r['path'].endswith('/UdfDataImportMixedModel.vb'))
 need(source['snapshot']['bytes']==38595 and source['snapshot']['sha256']=='5297574cab59e631cf880807903a206f76dfb1509818a1825c1ca6798332071a','Wrong original snapshot')
 for config in ['Debug','Release']:
  built=load(here/('build-original-'+config+'.json'));need(built['current_production_assemblies_consumed'] is False and built['source_commit']==BASE,'Current dependencies in original build')
  info=load(here/('compile-original-'+config+'-trace.json'))
  guards=[r for r in info['instrumentation'] if r['event'] in ['guard.duplicate_visit_time','guard.duplicate_design']]
  need([(r['archived_source_line'],r['source_anchor'],r['after_anchor']) for r in guards]==[(726,'If atProfile.VisitSpecified Then',True),(744,'If existing.ColumnIndex = idx Then',True)],'Missing/wrong original true-arm probes')
 return dict(status='DUPLICATE_TYPED_INPUT_SOURCE_SEAL_PASS',cases_per_configuration=4)
if __name__=='__main__':print(json.dumps(inspect()))
