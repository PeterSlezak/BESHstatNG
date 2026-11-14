"""Independent partial-publication gate; cannot approve a completed 04A.38."""
import hashlib,json,re
from pathlib import Path,PurePosixPath
import verify_p0,verify_p1_v5
from verify_preservation_v4 import inspect as preservation
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
SELF=PREFIX+'publication-manifest-v1.json'
def need(v,m):
 if not v:raise ValueError(m)
def load(p):
 def pairs(items):
  out={}
  for k,v in items:need(k not in out,'Duplicate JSON key');out[k]=v
  return out
 return json.loads(p.read_text(encoding='utf-8-sig'),object_pairs_hook=pairs)
def sha(b):return hashlib.sha256(b).hexdigest()
def inspect(root=ROOT,manifest_path=SELF):
 root=Path(root);manifest=load(root/manifest_path);rows=manifest['entries'];paths=[r['path'] for r in rows]
 need(manifest['status']=='NOT_READY_04A38','False READY')
 need(paths==sorted(paths) and len(paths)==len(set(p.casefold() for p in paths)),'Duplicate/casefold/unsorted payload')
 need(paths.count(manifest_path)==1,'Missing manifest self entry')
 nulls=0
 for row in rows:
  p=row['path'];pure=PurePosixPath(p)
  need(not pure.is_absolute() and pure.as_posix()==p and '\\' not in p and ':' not in p,'Unsafe/noncanonical path')
  need(all(x not in {'','.','..'} and not x.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',x) and not re.match(r'(?i)^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',x) for x in p.split('/')),'Reserved/escaping path')
  need((root/p).resolve().is_relative_to(root.resolve()),'Symlink escape')
  if row['sha256'] is None or row['bytes'] is None:
   nulls+=1;need(p==manifest_path and row['sha256'] is None and row['bytes'] is None,'Illegal null/self entry');continue
  raw=(root/p).read_bytes();need((len(raw),sha(raw))==(row['bytes'],row['sha256']),'Payload changed '+p)
  if p.startswith(PREFIX):
   matches=re.findall(rb'(?i)[a-z]:[\\/]+users[\\/]+[^\\/\s"\x27]+',raw)
   if matches:
    need(p==PREFIX+'test_publication.py' and sha(raw)=='77db10fd2989f6f8b688f603e89ed280d1582fe397f82aa31f8ba0b460f174f1' and all(m.endswith(b'EXAMPLE_PRIVATE') for m in matches),'Private home path in continuation payload '+p)
    # Exact, unchanged adversarial source literal is fictional. A generated
    # leak in another payload or any changed source hash remains rejected.
   need(not re.search(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',raw),'Private key')
 need(nulls==1,'Wrong self entry count')
 inventory=load(root/(PREFIX+manifest['inventory']))
 need(inventory['paths']==paths,'File inventory mismatch')
 protected=preservation(root,False);p0=verify_p0.inspect(root);p1=verify_p1_v5.inspect(root)
 need(p0==load(root/(PREFIX+'p0/independent-proof.json')) and p1==load(root/(PREFIX+'p1/independent-proof.json')),'Independent operation/edge proof differs')
 # Carry old full-body proofs only if complete fresh IL records still match.
 old=load(root/'evidence/04A38/completion/w2/independent-delegated-proof-v2.json');expected=set()
 former_by_config={config:{r['method']:r for r in load(root/('evidence/04A38/completion/w2/method-bodies-v2-current-'+config+'.json'))} for config in ['Debug','Release']}
 current_by_config={config:{r['method']:r for r in load(root/(PREFIX+'inspection/p1-v5/MethodBodies38-'+config+'.json'))} for config in ['Debug','Release']}
 for row in old['rows']:
  if row['status'].startswith('OPEN'):continue
  for config in ['Debug','Release']:
   former=former_by_config[config];current=current_by_config[config]
   ids=[row['method'],row['target']];ctor=row['method'].split('(')[0].rsplit('.',1)[0]+'..cctor()'
   if ctor in former:ids.append(ctor)
   need(all(i in current and former[i]==current[i] for i in ids),'Carried body/static initialization changed')
  expected.add(row['original_id'])
 profile=load(root/'evidence/04A38/completion/w2/independent-profile-proof.json')
 initial=(root/(PREFIX+'source-before/BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb.txt')).read_text(encoding='utf-8-sig')
 current=(root/verify_p1_v5.PROFILE).read_text(encoding='utf-8-sig')
 for name in ['TryGetGeeAtSpec','AddGeeAtValue','TryGetMmrmAtSpec','AddMmrmAtValue','TryGetMmrmHeaderlessNameValueAtSpec','TryGetMmrmDesignColumnValues','TryGetGeeSpec','TryGetMmrmSpec']:
  need(verify_p0.tokens(verify_p0.member(initial,name))==verify_p0.tokens(verify_p0.member(current,name)),'Prior profile preparation changed')
 expected.update(r['canonical_original_id'] for r in profile['targets'])
 historical=load(root/(PREFIX+'historical-ownership.json'));universe=load(root/'evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members']
 for suffix in p0['original_ids']:expected.add(next(r['canonical_original_id'] for r in universe if r['canonical_original_id'].endswith(suffix)))
 expected.update(r['original_id'] for r in p1['proofs'])
 need(len(historical['members'])==704 and {r['canonical_original_id'] for r in historical['members']}=={r['canonical_original_id'] for r in universe},'Omitted/extra historical identity')
 need({r['canonical_original_id'] for r in historical['members'] if r['proof']}==expected and historical['closed']==len(expected) and historical['OPEN']==704-len(expected),'Unsupported body closure/host assignment')
 ready=load(root/(PREFIX+manifest.get('readiness','readiness.json')))
 need(ready['status']=='NOT_READY_04A38' and ready['W2']=='IN_PROGRESS' and ready['W3']=='NOT_STARTED' and ready['historical_OPEN']==historical['OPEN'],'Incomplete batch promoted')
 need(ready['gates']==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'),'Native/scientific promotion')
 need(ready['new_original_queries']==0 and ready['owner_decision_needed'] is False,'New reference authority invented')
 return dict(status='VALID_PARTIAL_W2_CONTINUATION_NOT_READY',hashed_payloads=len(rows)-1,proof_supported_historical=len(expected),historical_OPEN=704-len(expected),independent_current_operations_and_IL='PASS_BOUNDED_SCOPE',preservation=protected,copy_limitations='No live Git/index, omitted owner documents, private loaded DLL/package cache or ignored history proof from a derivative archive')
if __name__=='__main__':
 import sys
 try:print(json.dumps(inspect(manifest_path=sys.argv[1] if len(sys.argv)>1 else SELF)))
 except (ValueError,OSError,KeyError,TypeError,StopIteration) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
