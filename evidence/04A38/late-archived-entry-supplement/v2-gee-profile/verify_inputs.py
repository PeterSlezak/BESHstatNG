"""Independent stdlib reader: fixed owner matrix and immutable input/source pins.

No capture, materializer, generator or implementation module is imported.
"""
import hashlib,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4]
PREFIX='evidence/04A38/late-archived-entry-supplement/v2-gee-profile/'
INPUT_SEAL='447f63197515c8ab460900da32a137f586d032e68d850d60325da87097ed2ba8'
FACTORY='5aab6239b32d5f657f7ee5be8aae94e7c2f1b73e4b84c2932df31be610004a48'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
QUOTE='I approve the additional 12-case-per-configuration GEE archived-entry supplement for 04A.38.'
MATRIX={
 'wide-time':[['time'],[1.0]],
 'name-value-time':[['Name','Value'],['time',1.0]],
 'name-value-visit':[['Name','Value'],['visit',1.0]],
 'duplicate-time-visit':[['Name','Value'],['time',1.0],['visit',2.0]],
 'duplicate-design':[['Name','Value'],['X1',1.0],['x1',2.0]],
 'wide-multiple-rows':[['time'],[1.0],[2.0]]}
class InvalidEvidence(ValueError):pass
def need(value,message):
 if not value:raise InvalidEvidence(message)
def digest(data):return hashlib.sha256(data).hexdigest()
def load(path):
 def unique(pairs):
  result={}
  for k,v in pairs:
   need(k not in result,'Duplicate JSON key');result[k]=v
  return result
 return json.loads(path.read_bytes(),object_pairs_hook=unique)
def inspect(root=ROOT):
 root=Path(root);here=root/PREFIX
 seal=load(here/'input-seal.json')
 need(digest((here/'input-seal.json').read_bytes())==INPUT_SEAL,'Input seal changed')
 need(seal['status']=='INPUT_SEALED' and seal['sequence']==1 and seal['source_commit']==BASE,'Wrong seal/source')
 need(seal['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT' and seal['not_preedit_frozen'] is True,'Late classification lost')
 need(seal['original_reference_executions']==seal['current_comparisons']==0,'Input seal after execution')
 for row in seal['files']:
  p=(root/row['path']).resolve();need(p.is_relative_to(root.resolve()),'Input seal path escape')
  data=p.read_bytes();need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Changed frozen field/source: '+row['path'])
 approval=load(here/'owner-authorization.json')
 need(approval['owner_quote']==QUOTE and approval['owner_decision_date']=='2026-11-09','Separate GEE authorization missing')
 need(approval['cases_per_configuration']==12 and approval['case_kinds']==6 and approval['cultures']==['Invariant','sk-SK'],'Wrong authorized matrix')
 factory=load(here/'factory-inputs.json');need(digest((here/'factory-inputs.json').read_bytes())==FACTORY,'Lost original fitted-model inputs')
 names=['handle','spec','covarianceType','alpha','at','scale']
 expected_ids=['PROPOSED-GEE-PROFILE/'+culture+'/'+suffix for culture in ['Invariant','sk-SK'] for suffix in MATRIX]
 for config in ['Debug','Release']:
  rows=[json.loads(line) for line in (here/('inputs-'+config+'.jsonl')).read_text(encoding='utf-8').splitlines()]
  need([r['case_id'] for r in rows]==expected_ids,'Wrong case count/ID/order/culture')
  for row in rows:
   culture,suffix=row['case_id'].split('/')[1:]
   need(row['culture']==('' if culture=='Invariant' else 'sk-SK') and row['configuration']==config,'Mismatched culture/configuration')
   need(row['public_entry']=='BESH.REGR.GEE_LSMESTIMATE' and row['reference_class']=='LATE_ARCHIVED_ENTRY_SUPPLEMENT','Unapproved UDF/reference class')
   need(row['operations'][0]==dict(role='PUBLIC_EXPORT',target='BESH.REGR.GEE_FIT',arguments=factory['fit_arguments']),'Lost fitted model factory')
   need(len(row['operations'])==2 and row['operations'][1]['target']=='BESH.REGR.GEE_LSMESTIMATE','Additional/unapproved execution')
   args=row['operations'][1]['arguments'];need([r['name'] for r in args]==names,'Incorrect public argument order')
   need(args[0]['value']==dict(kind='HandleReference',session='fresh-case') and args[1]['value']==factory['spec'],'Fake handle/spec')
   need(all(args[i]['value']=={'kind':'Nothing'} for i in [2,3,5]),'Defaults changed')
   a=args[4]['value'];values=MATRIX[suffix];r=len(values);c=len(values[0])
   need(a['kind']=='Array' and a['element_type']=='System.Object' and a['rank']==2,'Not actual Object[,]')
   need(a['lengths']==[r,c] and a['lower_bounds']==[0,0] and a['upper_bounds']==[r-1,c-1],'Wrong array shape/bounds')
   flattened=[v for line in values for v in line]
   import struct
   expected=[dict(kind='String',value=v) if type(v) is str else dict(kind='Double',bits=struct.pack('>d',v).hex().upper()) for v in flattened]
   need(a['values']==expected,'Changed at value/runtime cell type/IEEE bits')
   need(row['session']['expected_parameter_names']==['Intercept','X1','X2'] and row['session']['creation_order'][0]=='clear private GEE cache','Lost session/design context')
 entry=load(here/'entry-inspection.json')
 need(entry['source_commit']==BASE and len(entry['original_tree_files'])==4695 and len(entry['udf_snapshots'])==41,'Lost original provenance')
 for row in entry['udf_snapshots']:
  data=(root/row['snapshot']['path']).read_bytes();need(digest(data)==row['snapshot']['sha256'],'Archived source changed')
 gee=next(r for r in entry['udf_snapshots'] if r['source_path'].endswith('/GeeUDFs.vb'))
 need(gee['git_bytes']==114843 and gee['relation']=='EXACT' and gee['git_sha256']=='35a3a35ddf4dfdb074a082dfe92495969dd1a8532f86af6650d6bde33bcf7649','Wrong GEE original')
 return dict(status='TYPED_INPUT_AND_SOURCE_SEAL_PASS',cases_per_configuration=12)
if __name__=='__main__':print(json.dumps(inspect()))
