"""Versioned final41 reporting and fail-closed actual-candidate/physical privacy seal."""
from common42 import *
import collections,re,stat,unicodedata,zipfile,zlib
sys.path.insert(0,str(ROOT/'evidence/04A39/physical-witness-v1'))
import physical
STATUS='READY_FOR_OWNER_04A42_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
class ProbeMember:
    def __init__(self,text):self.text=text
    def is_file(self):return True
    def read_text(self,encoding):assert encoding=='utf-8-sig';return self.text
class ProbeRoot:
    parents=ROOT.parents;name=ROOT.name
    def __init__(self,text):self.text=text
    def __truediv__(self,path):return ProbeMember(self.text)
    def as_posix(self):return ROOT.as_posix()
    def __fspath__(self):return str(ROOT)
def identity_controls(phase):
    controls=[]
    for i,token in enumerate(sorted(physical.tokens())):
        variants=[token,token.swapcase(),''.join(chr(ord(c)+0xfee0) if 33<=ord(c)<=126 else c for c in token)]
        for text,mode in zip(variants,['literal','case','NFKC']):
            m=physical.scanner(phase);m.ROOT=ProbeRoot(json.dumps(dict(value=text)))
            a,ar=m.scan(['identity-control.json']);m.re=re;b,br=m.scan(['identity-control.json'])
            assert a==b and ar==br and any(r['category']=='known_private_identifier' for r in a)
            controls.append(dict(identity_ordinal=i,mode=mode,detected=True,direct_original_regex_and_guard_exact=True))
    return controls
def candidate_scan(label):
    lock();reports=[]
    for phase in ['13','14']:
        m=physical.scanner(phase);assert m.ROOT==ROOT and physical.tokens()
        paths=m.paths();inputs=[pin(p) for p in paths];found,reviewed=m.scan(paths)
        assert inputs==[pin(p) for p in paths]
        raw=PRIVATE/(label+'-'+phase+'.json');save(raw,dict(candidate_inputs=inputs,findings=found,reviewed=reviewed,scanner=pin(m.__file__)))
        assert not found,'UNRESOLVED_ACTUAL_CANDIDATE_PRIVACY_FINDING'
        reports.append(dict(scanner=phase,source=pin(m.__file__),scan_AST_sha256=sha(physical.function_ast(m,'scan').encode()),guard=pin(ROOT/'evidence/04A39/physical-witness-v1/dependencies/privacy_regex_guard.py'),members=inputs,candidates=len(paths),findings=0,unresolved=0,identity_controls=identity_controls(phase),raw=pin(raw),scope='Actual unchanged paths() over every changed/new Git candidate, including original phase-specific metadata. No filtered/staged/ZIP surrogate.',normalization='Original NFKC, paths/content/decodedJSON/base64 rules;Git relative-path spelling retained.'))
        print('Original repository scanner',phase,len(paths),'candidates;0 findings',flush=True)
    return reports
def check_sources():
    lock();freeze=load(HERE/'matrix/source-freeze-v2.json')
    for p,h in freeze['source_pins'].items():assert pin(p)['sha256']==h
    exact(freeze['plan']);exact(freeze['plan_snapshot'])
    allowed={r['after']['path'] for r in freeze['source_delta']}|{freeze['plan']['path']}
    assert set(git('diff','--name-only').decode().splitlines())==allowed
    assert not git('diff','--check')
    assert not any((ROOT/p).exists() for p in ['evidence/04A40/baseline/Debug-build-v1.json','evidence/04A40/entry/tool-runtime-final-v1.json'])
    return freeze