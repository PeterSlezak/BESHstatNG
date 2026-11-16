"""Original full-candidate + physical-member scans and independent external seal."""
from common45 import *
import collections,io,re,stat,unicodedata,zipfile,zlib
sys.path.insert(0,str(ROOT/'evidence/04A39/physical-witness-v1'))
import physical
STATUS='READY_FOR_OWNER_04A45_SAFE_HARNESS_REVIEW__NATIVE_GATE_NOT_AUTHORIZED'
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
  variants=[token,token.swapcase(),''.join(chr(ord(c)+0xfee0) if 33<=ord(c)<=126 else c for c in token)]
  for value,mode in zip(variants,['literal','case','NFKC']):
   m=physical.scanner(phase);m.ROOT=ProbeRoot(json.dumps(dict(value=value)))
   a,ar=m.scan(['positive.json']);m.re=re;b,br=m.scan(['positive.json'])
   assert a==b and ar==br and any(r['category']=='known_private_identifier' for r in a)
   rows.append(dict(identity_ordinal=i,mode=mode,detected=True,known_positive_control=True,original_regex_and_guard_exact=True))
 m=physical.scanner(phase);m.ROOT=ProbeRoot('ordinary public content')
 findings,_=m.scan(['negative.txt']);assert not findings
 return dict(positive=rows,negative_clean=True,raw_tokens_published=False)
def candidate_scan(label):
 result=[]
 for phase in ['13','14']:
  m=physical.scanner(phase);assert m.ROOT==ROOT and physical.tokens()
  names=m.paths();pins=[pin(p) for p in names];f,r=m.scan(names)
  save(PRIVATE/(label+'-'+phase+'.json'),dict(inputs=pins,findings=f,reviewed=r,scanner=pin(m.__file__)))
  assert pins==[pin(p) for p in names]
  assert not f,'UNRESOLVED_CANDIDATE_PRIVACY_FINDING'
  result.append(dict(scanner=phase,source=pin(m.__file__),scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),members=pins,candidates=len(pins),findings=0,errors=0,scope='ORIGINAL_UNCHANGED_PATHS_FUNCTIONS;complete actual changed/new Git candidate;not staged or ZIP surrogate',controls=controls(phase)))
  print('Original scanner'+phase,len(pins),'actual candidates:0 findings/0 errors',flush=True)
 return result
def physical_scan(path):
 results=[]
 with zipfile.ZipFile(path) as z:
  names=z.namelist();members=physical.inventory(z)
  assert len(names)==len({unicodedata.normalize('NFKC',n).casefold() for n in names})
  for phase in ['13','14']:
   m=physical.scanner(phase);m.ROOT=physical.ZipRoot(z,'utf-8-sig');f,r=m.scan(names)
   save(PRIVATE/('physical-'+phase+'-v1.json'),dict(findings=f,reviewed=r,physical_members=members))
   assert not f,'UNRESOLVED_PHYSICAL_PRIVACY_FINDING'
   results.append(dict(scanner=phase,source=pin(m.__file__),members=len(members),includes_index=True,findings=0,errors=0,controls=controls(phase)))
 return members,results
def publish():
 lock();assert not git('diff','--check')
 freeze=load(HERE/'matrix/source-freeze-v1.json');assert freeze['source_pins']==runner().__class__.build.__globals__['source_pins']()
 manifest=load(HERE/'final/package-members-v1.json');members=manifest['members'];assert len(members)==len(set(members))
 for n in members:physical.safe_name(n)
 final_candidates=candidate_scan('final-full-candidate-v1')
 folder=PRIVATE/'publication';folder.mkdir();archive=folder/'BESHStatNG_04A45_SAFE_HARNESS_REVIEW_v1.zip'
 contents={n:(ROOT/n).read_bytes() for n in members}
 index=dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in sorted(contents.items())])
 contents['.review-index.json']=(json.dumps(index,indent=2)+'\n').encode()
 with zipfile.ZipFile(archive,'x',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
  for n,b in sorted(contents.items()):z.writestr(n,b)
 reader=module('independent45_reader',HERE/'review_reader.py');verified=reader.verify(archive)
 adverse=reader.adverse(archive)
 neutral=module('publication45_neutral',HERE/'neutral_privacy_replay.py').replay(archive)
 physical_members,physical_reports=physical_scan(archive)
 # Fresh final scope readback after all delivery work;no public candidate created here.
 post=[]
 for r in final_candidates:
  m=physical.scanner(r['scanner']);assert m.paths()==[p['path'] for p in r['members']]
  for p in r['members']:exact(p)
  post.append(dict(scanner=r['scanner'],membership_and_bytes_exact=True,candidates=r['candidates']))
 assert not git('diff','--cached','--name-only') and not git('diff','--check');lock()
 receipt=folder/'external-delivery-receipt-v1.json'
 save(receipt,dict(status=STATUS,utc=utc(),archive=pin(archive),physical_members=physical_members,full_actual_candidate_scans=final_candidates,physical_scans=physical_reports,adverse_controls=adverse,neutral_reviewer_replay=neutral,failed_attempts=[pin(p) for p in sorted((PRIVATE/'failed-attempts').glob('*.json'))],independent_reader_result=verified,primary_reports=[pin(HERE/p) for p in ['final/owner-review-v1.md','final/readiness-v1.json','final/software-verification-v1.json','scope/ribbon-v1.json','scope/event-map-review-v1.json','register/interim-delta-v1.json','preservation/readback-v1.json','python/dispositions-v1.json','native-risk/owner-decision-v1.md']],postdelivery_candidate_readback=post,postdelivery_index=pin('.git/index'),postdelivery_git=dict(branch=git('branch','--show-current').decode().strip(),HEAD=MAIN,main=MAIN,merge_base=MAIN,staged_empty=True,diff_check_exit=0,modified=git('diff','--name-only').decode().splitlines(),added=git('ls-files','--others','--exclude-standard').decode().splitlines()),preservation=pin(HERE/'preservation/readback-v1.json'),postdelivery_preservation='Protected raw/tracked pins freshly read back before publication;all candidate/index bytes exact after. Continuous physical identity NOT_VERIFIED;W3 full union not freshly read.',native='NOT_RUN',actual_HWND_PID_XLL='NOT_VERIFIED',gates=GATES))
 assert reader.verify(archive,receipt)['status']=='PASS_EXTERNAL_AND_SEMANTIC'
 print('Archive',pin(archive)['bytes'],pin(archive)['sha256'],flush=True)
 print('External receipt',pin(receipt)['bytes'],pin(receipt)['sha256'],flush=True)
if __name__=='__main__':publish()
