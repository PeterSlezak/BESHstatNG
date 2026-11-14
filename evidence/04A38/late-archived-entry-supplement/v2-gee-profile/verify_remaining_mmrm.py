"""Independent read-only reanalysis of the missing MMRM original branch.

Does not import the audit producer, capture harness or its token parser.
"""
import base64,csv,json,re
from collections import Counter
from pathlib import Path
from verify_gee import ROOT,PREFIX,BASE,load,need,digest,check_pin,PREVIOUS,PREVIOUS_HASH

def array(token):
 if not re.match(r'^System\.[\w.]+\[[\d,:-]+\]\{',token):return None
 start=token.index('{');header=token[:start];body=token[start+1:-1]
 shape=[tuple(map(int,item.split(':'))) for item in header[header.index('[')+1:-1].split(',')]
 depth=0;values=[];last=0
 for i,ch in enumerate(body):
  if ch=='{':depth+=1
  elif ch=='}':depth-=1
  elif ch=='|' and depth==0:values.append(body[last:i]);last=i+1
 if body:values.append(body[last:])
 need(depth==0,'Unbalanced canonical array')
 from math import prod
 need(len(values)==prod(n for _,n in shape),'Canonical array cardinality')
 return shape,values

def inspect(root=ROOT):
 root=Path(root);here=root/PREFIX
 raw=(root/PREVIOUS).read_bytes();need(digest(raw)==PREVIOUS_HASH,'Changed old authority')
 for row in json.loads(raw)['entries']:
  if row['sha256'] is not None:check_pin(root,row)
 report=load(here/'remaining-mmrm-profile-coverage.json')
 need(report['status']=='NOT_READY_04A38' and report['new_reference_execution']=='NOT_AUTHORIZED_NOT_EXECUTED' and report['production_changes_after_gee_gate']==0,'Unapproved MMRM capture/production/READY')
 need(report['expected_output']=='UNKNOWN_NOT_CAPTURED','Invented MMRM output')
 source=(root/'evidence/04A38/preedit/source/UDFdataImport/UdfDataImportMixedModel.vb.txt').read_text(encoding='utf-8-sig')
 need('If nameCol >= 0 AndAlso valueCol >= 0 AndAlso nameCol <> valueCol Then' in source and 'TryGetMmrmHeaderlessNameValueAtSpec' in source,'Lost source branch distinction')
 entry=load(root/'evidence/04A38/entry-members.json')['members']
 names=['TryGetMmrmLsmEstimateAtSpec','AddMmrmLsmEstimateAtValue','TryBuildMmrmAtProfileRowMask']
 members={name:next(m for m in entry if m['name']==name and ('UdfDataImportMixedModel' in m['path'] or name=='TryBuildMmrmAtProfileRowMask')) for name in names}
 need(report['member_id']==members[names[0]]['symbol_id'],'Wrong remaining member identity')
 corpora=[('original-v1','frozen-v2'),('public-options-v1','attempts/public-options-v1'),('public-primitives-v1','attempts/public-primitives-v1'),('model-inputs-v2','attempts/model-inputs-v2'),('math-edges-v1','attempts/math-edges-v1')]
 observed={};witnesses={}
 for config in ['Debug','Release']:
  counts=Counter()
  for corpus,relative in corpora:
   base=root/'evidence/04A38/preedit'/relative
   cases=[json.loads(line) for line in (base/'cases.jsonl').read_bytes().splitlines()]
   trace=(root/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')) if corpus=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   hits={r['case_id']:r['reached'] for r in map(json.loads,trace.read_bytes().splitlines())}
   output=base/('original-'+config+'.tsv' if corpus=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as stream:outputs={r['case_id']:r for r in csv.DictReader(stream,delimiter='\t')}
   for case in cases:
    key=case['case_id'];target=case.get('target')
    entered=[name for name in names if members[name]['symbol_id'] in hits[key]]
    if target not in ['BESH.REGR.MMRM_LSMEANS','BESH.REGR.MMRM_LSMESTIMATE']:
     need(not entered,'Unreviewed indirect profile case');continue
    counts['public_cases']+=1
    shape,args=array(outputs[key]['byref_arguments'])
    slot=1 if target.endswith('_LSMEANS') else 3
    need(len(args)==(3 if slot==1 else 4),'Incorrect archived public signature')
    token=args[slot]
    if token=='N':counts['default_profile']+=1;continue
    a=array(token)
    if a is None:counts['scalar_profile']+=1;continue
    counts['array_profile']+=1
    for name in entered:counts['member_entry/'+name]+=1
    dims,values=a
    need(len(dims)==2,'Unexpected original at rank')
    rows,cols=[n for _,n in dims]
    header=[''.join(ch for ch in base64.b64decode(v[1:]).decode().lower() if ch.isalnum()) if v.startswith('S') else '' for v in values[:cols]]
    n=[i for i,h in enumerate(header) if h in ['name','variable','effect','column','profile','at']]
    v=[i for i,h in enumerate(header) if h in ['value','val','setting','atvalue']]
    headered=any(i!=j for i in n for j in v)
    if headered:counts['headered_name_value_candidate']+=1
    witnesses[(config,key)]=(token,headered,entered,outputs[key]['outcome'])
  counts['headered_name_value_candidate']+=0
  observed[config]=dict(counts)
 need(observed==report['counts'],'Coverage report differs from independent raw corpus analysis')
 for config in ['Debug','Release']:
  need(observed[config]=={'public_cases':154,'default_profile':140,'array_profile':12,'scalar_profile':2,'headered_name_value_candidate':0,
       'member_entry/TryGetMmrmLsmEstimateAtSpec':12,'member_entry/AddMmrmLsmEstimateAtValue':10,'member_entry/TryBuildMmrmAtProfileRowMask':4},'Wrong frozen profile coverage')
 need(len(report['witnesses'])==len(witnesses)==24,'Omitted/extra witnesses')
 for row in report['witnesses']:
  need((row['recorded_profile'],row['headered_name_value_candidate'],row['source_members_entered'],row['outcome'])==witnesses[(row['configuration'],row['case_id'])],'Changed witness classification')
 for row in report['inputs']:check_pin(root,row)
 return dict(status='VALID_NOT_READY_MMRM_REFERENCE_GAP',configurations=observed,member_id=report['member_id'],new_references=0,new_production_changes=0)
if __name__=='__main__':print(json.dumps(inspect()))
