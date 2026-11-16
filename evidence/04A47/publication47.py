"""Original full-candidate/physical scans;exact non-circular external delivery seal."""
from common47 import *
import base64,re,stat,unicodedata,zipfile,zlib
sys.path.insert(0,str(ROOT/'evidence/04A39/physical-witness-v1'))
import physical
STATUS='NOT_READY_04A47_SAFE_SOFTWARE'
class ProbeMember:
 def __init__(self,text):self.text=text
 def is_file(self):return True
 def read_text(self,encoding):assert encoding=='utf-8-sig';return self.text
class ProbeRoot:
 parents=ROOT.parents;name=ROOT.name
 def __init__(self,text):self.text=text
 def __truediv__(self,p):return ProbeMember(self.text)
 def as_posix(self):return ROOT.as_posix()
 def __fspath__(self):return str(ROOT)

def controls(phase):
 rows=[]
 for i,token in enumerate(sorted(physical.tokens())):
  variants=[token,token.swapcase(),''.join(chr(ord(c)+0xfee0) if 33<=ord(c)<=126 else c for c in token),base64.b64encode(('public prefix '*20+token+' public suffix '*20).encode()).decode()]
  for value,mode in zip(variants,['literal','case','NFKC','Base64']):
   m=physical.scanner(phase);m.ROOT=ProbeRoot(json.dumps(dict(value=value)))
   a,ar=m.scan(['positive.json']);m.re=re;b,br=m.scan(['positive.json'])
   assert a==b and ar==br and any(r['category']=='known_private_identifier' for r in a)
   rows.append(dict(identity_ordinal=i,mode=mode,detected=True,expected_positive=True,original_regex_and_guard_decisions_exact=True))
 m=physical.scanner(phase);m.ROOT=ProbeRoot('ordinary public content');f,_=m.scan(['negative.txt']);assert not f
 return dict(positive=rows,negative_clean=True,raw_tokens_published=False)

def candidate_scan(label):
 result=[]
 for phase in ['13','14']:
  m=physical.scanner(phase);assert m.ROOT==ROOT and physical.tokens()
  names=m.paths();pins=[pin(p) for p in names];f,r=m.scan(names)
  save(PRIVATE/(label+'-'+phase+'.json'),dict(inputs=pins,findings=f,reviewed=r,scanner=pin(m.__file__)))
  assert pins==[pin(p) for p in names] and not f,'UNRESOLVED_CANDIDATE_PRIVACY_FINDING'
  result.append(dict(scanner=phase,source=pin(m.__file__),scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),paths_AST_sha256=sha(physical.function_ast(m,'paths').encode()),members=pins,candidates=len(pins),findings=0,errors=0,scope='ORIGINAL_UNCHANGED_PATHS_FUNCTIONS;complete actual changed/new Git candidate,not a staged or ZIP surrogate',controls=controls(phase)))
  print('Original scanner'+phase,len(pins),'actual candidates:0 findings/0 errors',flush=True)
 return result

def physical_scan(path,label):
 results=[]
 with zipfile.ZipFile(path) as z:
  names=z.namelist();members=physical.inventory(z)
  assert len(names)==len({unicodedata.normalize('NFKC',n).casefold() for n in names})
  for phase in ['13','14']:
   m=physical.scanner(phase);m.ROOT=physical.ZipRoot(z,'utf-8-sig');f,r=m.scan(names)
   save(PRIVATE/(label+'-physical-'+phase+'.json'),dict(findings=f,reviewed=r,physical_members=members))
   assert not f,'UNRESOLVED_PHYSICAL_PRIVACY_FINDING'
   results.append(dict(scanner=phase,source=pin(m.__file__),members=len(members),includes_index=True,findings=0,errors=0,controls=controls(phase)))
   print('Original scanner'+phase,'all',len(members),'physical members:0 findings/0 errors',flush=True)
 return members,results

def preservation_readback():
 # Repeat only physical readback,not managed/Python scientific campaigns.
 evidence=load(HERE/'preservation/readback-v1.json');rows={}
 def record(r):
  if r['path'] in rows:assert rows[r['path']]=={k:r[k] for k in ['path','bytes','sha256']}
  rows[r['path']]={k:r[k] for k in ['path','bytes','sha256']}
 allowed=set(evidence['allowed_changes'])
 for r in load(PRIVATE/'tracked-entry-inventory-v1.json')['files']:
  if r['path'] in allowed:
   p=PRIVATE/'source-preimages'/r['path'] if r['path'].endswith('.vbproj') else ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan2.23.md'
   assert pin(p)['sha256']==r['sha256'] and pin(p)['bytes']==r['bytes']
  else:record(r)
 for population in evidence['raw_populations']+evidence['accepted45_46_populations']:
  exact(population['current_inventory'])
  for r in load(ROOT/population['current_inventory']['path'])['files']:record(r)
 for r in load(ROOT/'.test-rationalization-04A39-correction-v1.tmp/checkpoint-files.json')['files']:record(dict(r,path='.test-rationalization-04A39-correction-v1.tmp/checkpoint/'+r['path']))
 for r in rows.values():exact(r)
 return dict(scope='Fresh post-delivery tracked-entry reconciliation plus468 checkpoint copies and full41-46 previously read-back store snapshots. Earlier8984/raw40/original source-history proof in unchanged preservation report. Not a fresh W3 union or continuous physical-identity proof.',individually_rehashed_paths=len(rows),all_exact=True,preservation_report=pin(HERE/'preservation/readback-v1.json'),historical_drifts_retained=788,continuous_physical_identity='NOT_VERIFIED')

def publish(version):
 lock();assert not git('diff','--check');label='delivery-'+version
 freeze=load(HERE/'matrix/source-freeze-v1.json');assert freeze['source_pins']==runner().__class__.build.__globals__['source_pins']()
 manifest=load(HERE/'final/package-members-v1.json');members=manifest['members'];assert len(members)==len(set(members))
 for n in members:physical.safe_name(n)
 candidates=candidate_scan(label+'-full-candidate')
 folder=PRIVATE/('publication-'+version);folder.mkdir();archive=folder/('BESHStatNG_04A47_SAFE_LONGRUN_GUI_WIRING_REVIEW_'+version+'.zip')
 contents={n:(ROOT/n).read_bytes() for n in members}
 contents['.review-index.json']=(json.dumps(dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in sorted(contents.items())]),indent=2)+'\n').encode()
 with zipfile.ZipFile(archive,'x',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
  for n,b in sorted(contents.items()):z.writestr(n,b)
 reader=module('reader47_final',HERE/'review_reader.py');verified=reader.verify(archive);adverse=reader.adverse(archive)
 neutral=module('neutral47_final',HERE/'neutral_privacy_replay.py').replay(archive)
 physical_members,physical_reports=physical_scan(archive,label)
 preserved=preservation_readback();post=[]
 for r in candidates:
  m=physical.scanner(r['scanner']);assert m.paths()==[p['path'] for p in r['members']]
  for p in r['members']:exact(p)
  post.append(dict(scanner=r['scanner'],membership_and_bytes_exact=True,candidates=r['candidates']))
 assert not git('diff','--cached','--name-only') and not git('diff','--check');lock()
 receipt=folder/('external-delivery-receipt-'+version+'.json')
 save(receipt,dict(status=STATUS,utc=utc(),archive=pin(archive),physical_members=physical_members,full_actual_candidate_scans=candidates,physical_scans=physical_reports,adverse_controls=adverse,neutral_reviewer_replay=neutral,failed_attempts=[pin(p) for p in sorted((PRIVATE/'failed-attempts').glob('*.json'))],independent_reader_result=verified,primary_reports=[pin(HERE/p) for p in ['final/owner-review-v1.md','final/readiness-v1.json','final/software-verification-v2.json','scope/family-review-v3.json','register/interim-delta-v2.json','preservation/readback-v1.json','python/dispositions-v1.json','native-risk/owner-decision-addendum-v1.md','abi-cil/UI-and-attribution-v1.json']],postdelivery_candidate_readback=post,postdelivery_index=pin('.git/index'),postdelivery_git=dict(branch=git('branch','--show-current').decode().strip(),HEAD=MAIN,main=MAIN,merge_base=MAIN,staged_empty=True,diff_check_exit=0,modified=git('diff','--name-only').decode().splitlines(),added=git('ls-files','--others','--exclude-standard').decode().splitlines()),postdelivery_preservation=preserved,native='NOT_RUN',actual_HWND_PID_XLL='NOT_VERIFIED',gates=GATES))
 assert reader.verify(archive,receipt)['status']=='PASS_EXTERNAL_AND_SEMANTIC'
 for phase in ['13','14']:
  m=physical.scanner(phase);m.ROOT=ProbeRoot(receipt.read_text(encoding='utf-8'));f,_=m.scan(['delivery-receipt.json']);assert not f,'EXTERNAL_RECEIPT_PRIVACY_FAILURE'
 print('Archive',pin(archive)['bytes'],pin(archive)['sha256'],flush=True)
 print('External receipt',pin(receipt)['bytes'],pin(receipt)['sha256'],flush=True)
if __name__=='__main__':publish(sys.argv[1] if len(sys.argv)>1 else 'v1')
